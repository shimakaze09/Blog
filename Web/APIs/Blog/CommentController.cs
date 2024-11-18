using System.Text.Json;
using CodeLab.Share.Contrib.StopWords;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Comments;
using Web.ViewModels.QueryFilters;

namespace Web.Apis.Blog;

[Route("Api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;
    private readonly TempFilterService _filter;

    public CommentController(CommentService commentService, TempFilterService filter)
    {
        _commentService = commentService;
        _filter = filter;
    }

    /// <summary>
    /// Get paginated comments
    /// </summary>
    [HttpGet]
    public async Task<ApiResponsePaged<Comment>> GetPagedList([FromQuery] CommentQueryParameters @params)
    {
        var (data, meta) = await _commentService.GetPagedList(@params);
        return new ApiResponsePaged<Comment>(data, meta);
    }

    /// <summary>
    /// Get anonymous user information based on email and verification code
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetAnonymousUser(string email, string otp)
    {
        if (!CommentService.IsValidEmail(email)) return ApiResponse.BadRequest("The provided email address is invalid");

        var verified = _commentService.VerifyOtp(email, otp);
        if (!verified) return ApiResponse.BadRequest("The verification code is invalid");

        var anonymous = await _commentService.GetAnonymousUser(email);
        var (_, newOtp) = await _commentService.GenerateOtp(email, true);

        return ApiResponse.Ok(new
        {
            AnonymousUser = anonymous,
            NewOtp = newOtp
        });
    }

    /// <summary>
    /// Get email verification code
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetEmailOtp(string email)
    {
        if (!CommentService.IsValidEmail(email))
        {
            return ApiResponse.BadRequest("The provided email address is invalid");
        }

        var (result, _) = await _commentService.GenerateOtp(email);
        return result
            ? ApiResponse.Ok("Email verification code sent successfully, valid for five minutes")
            : ApiResponse.BadRequest(
                "The previous verification code is still valid, please do not request the code repeatedly");
    }

    [HttpPost]
    public async Task<ApiResponse<Comment>> Add(CommentCreationDto dto)
    {
        if (!_commentService.VerifyOtp(dto.Email, dto.EmailOtp))
        {
            return ApiResponse.BadRequest("The verification code is invalid");
        }

        var anonymousUser = await _commentService.GetOrCreateAnonymousUser(
            dto.UserName, dto.Email, dto.Url,
            HttpContext.GetRemoteIPAddress()?.ToString().Split(":")?.Last()
        );

        var comment = new Comment
        {
            ParentId = dto.ParentId,
            PostId = dto.PostId,
            AnonymousUserId = anonymousUser.Id,
            UserAgent = Request.Headers.UserAgent,
            Content = dto.Content
        };

        string msg;
        if (_filter.CheckBadWord(dto.Content))
        {
            comment.Visible = false;
            msg =
                "The moderator has detected inappropriate language in your comment. It will be displayed after approval.";
        }
        else
        {
            comment.Visible = true;
            msg = "Your comment has been approved by the moderator. Thank you for participating in the discussion.";
        }

        comment = await _commentService.Add(comment);

        return new ApiResponse<Comment>(comment)
        {
            Message = msg
        };
    }

    [HttpGet("[action]")]
    public async Task<ApiResponse> CheckBadWord(string word)
    {
        return ApiResponse.Ok(_filter.CheckBadWord(word).ToString());
    }
}