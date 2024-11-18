using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Comments;

namespace Web.Apis.Blog;

[Route("Api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService)
    {
        _commentService = commentService;
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

        var result = await _commentService.GenerateOtp(email);
        return result
            ? ApiResponse.Ok("Email verification code sent successfully, valid for five minutes")
            : ApiResponse.BadRequest(
                "The previous verification code is still valid, please do not request the code repeatedly");
    }

    [HttpPost]
    public async Task<ApiResponse<Comment>> Add(CommentCreationDto dto)
    {
        if (!CommentService.IsValidEmail(dto.Email))
        {
            return ApiResponse.BadRequest("The provided email address is invalid");
        }

        if (!await _commentService.VerifyOtp(dto.Email, dto.EmailOtp))
        {
            return ApiResponse.BadRequest("The verification code is invalid");
        }

        var anonymousUser = await _commentService.GetOrCreateAnonymousUser(
            dto.UserName, dto.Email, dto.Url,
            HttpContext.GetRemoteIPAddress()?.ToString().Split(":")?.Last()
        );

        var comment = new Comment
        {
            PostId = dto.PostId,
            AnonymousUserId = anonymousUser.Id,
            UserAgent = Request.Headers.UserAgent,
            Content = dto.Content
        };
        return new ApiResponse<Comment>(await _commentService.Add(comment))
        {
            Message = "The comment has been submitted and will be displayed after approval"
        };
    }
}