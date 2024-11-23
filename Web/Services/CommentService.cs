﻿using System.Text.RegularExpressions;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels;
using Data.Models;
using FreeSql;
using Microsoft.Extensions.Caching.Memory;
using Share.Utils;
using Web.ViewModels.QueryFilters;
using X.PagedList;

namespace Web.Services;

public class CommentService
{
    private readonly IBaseRepository<AnonymousUser> _anonymousRepo;
    private readonly IBaseRepository<Comment> _commentRepo;
    private readonly EmailService _emailService;
    private readonly ILogger<CommentService> _logger;
    private readonly IMemoryCache _memoryCache;

    public CommentService(ILogger<CommentService> logger, IBaseRepository<Comment> commentRepo,
        IBaseRepository<AnonymousUser> anonymousRepo, IMemoryCache memoryCache, EmailService emailService)
    {
        _logger = logger;
        _commentRepo = commentRepo;
        _anonymousRepo = anonymousRepo;
        _memoryCache = memoryCache;
        _emailService = emailService;
    }

    private List<Comment>? GetCommentsTree(List<Comment> commentsList, string? parentId = null)
    {
        var comments = commentsList.Where(e => e.Visible).ToList();

        if (parentId != null) comments = comments.Where(e => e.ParentId == parentId).ToList();

        if (comments.Count == 0) return null;

        return comments.Select(comment =>
        {
            comment.Comments = GetCommentsTree(commentsList, comment.Id);
            return comment;
        }).ToList();
    }

    public async Task<List<Comment>?> GetAll(string postId)
    {
        var comments = await _commentRepo.Where(a => a.PostId == postId).ToListAsync();
        return GetCommentsTree(comments);
    }

    public async Task<(List<Comment>, PaginationMetadata)> GetPagedList(CommentQueryParameters param,
        bool adminMode = false, bool includePost = false)
    {
        var querySet = _commentRepo.Select;

        if (!adminMode) querySet = querySet.Where(a => a.Visible);

        if (includePost) querySet = querySet.Include(a => a.Post);

        if (!string.IsNullOrWhiteSpace(param.PostId)) querySet = querySet.Where(a => a.PostId == param.PostId);

        if (!string.IsNullOrWhiteSpace(param.Search)) querySet = querySet.Where(a => a.Content.Contains(param.Search));

        // Sorting
        if (!string.IsNullOrEmpty(param.SortBy))
        {
            // Ascending order
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');
            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        var data = await querySet.Page(param.Page, param.PageSize)
            .Include(a => a.AnonymousUser)
            .Include(a => a.Parent.AnonymousUser)
            .ToListAsync();
        IPagedList<Comment> pagedList = new StaticPagedList<Comment>(data, param.Page, param.PageSize,
            Convert.ToInt32(await querySet.CountAsync()));

        return (data, pagedList.ToPaginationMetadata());
    }

    public async Task<Comment?> GetById(string id)
    {
        return await _commentRepo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<Comment> Accept(Comment comment, string? reason = null)
    {
        comment.Visible = true;
        comment.IsNeedAudit = false;
        comment.Reason = reason;
        await _commentRepo.UpdateAsync(comment);
        return comment;
    }

    public async Task<Comment> Reject(Comment comment, string reason)
    {
        comment.Visible = false;
        comment.IsNeedAudit = false;
        comment.Reason = reason;
        await _commentRepo.UpdateAsync(comment);
        return comment;
    }

    public async Task<AnonymousUser?> GetAnonymousUser(string email)
    {
        return await _anonymousRepo.Where(a => a.Email == email).FirstAsync();
    }

    public async Task<AnonymousUser> GetOrCreateAnonymousUser(string name, string email, string? url, string? ip)
    {
        var item =
            await _anonymousRepo.Where(a => a.Email == email).FirstAsync() ??
            new AnonymousUser { Id = GuidUtils.GuidTo16String(), Email = email };

        item.Name = name;
        item.Ip = ip;
        item.Url = url;
        item.UpdatedTime = DateTime.Now;
        return await _anonymousRepo.InsertOrUpdateAsync(item);
    }

    /// <summary>
    ///     Checks if the email address is valid
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || email.Length < 7) return false;

        var match = Regex.Match(email, @"[^@ \t\r\n]+@[^@ \t\r\n]+\.[^@ \t\r\n]+");
        var isMatch = match.Success;
        return isMatch;
    }

    /// <summary>
    ///     Generates an email verification code and sends the verification email
    /// </summary>
    public async Task<(bool, string?)> GenerateOtp(string email, bool mock = false)
    {
        var cacheKey = $"comment-otp-{email}";
        var hasCache = _memoryCache.TryGetValue<string>(cacheKey, out var existingValue);
        if (hasCache) return (false, existingValue);

        var otp = await _emailService.SendOtpMail(email, mock);
        _memoryCache.Set<string>(cacheKey, otp, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return (true, otp);
    }

    /// <summary>
    ///     Verifies the one-time password
    /// </summary>
    /// <param name="clear">Whether to clear after successful verification</param>
    public bool VerifyOtp(string email, string otp, bool clear = true)
    {
        var cacheKey = $"comment-otp-{email}";
        _memoryCache.TryGetValue<string>(cacheKey, out var value);

        if (otp != value) return false;

        if (clear) _memoryCache.Remove(cacheKey);
        return true;
    }

    public async Task<Comment> Add(Comment comment)
    {
        comment.Id = GuidUtils.GuidTo16String();
        return await _commentRepo.InsertAsync(comment);
    }
}