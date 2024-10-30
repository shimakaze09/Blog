using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;
using Web.ViewModels;
using Web.ViewModels.Response;

namespace Web.Controllers;

/// <summary>
/// Authentication
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="loginUser"></param>
    /// <returns></returns>
    [HttpPost]
    public ApiResponse<LoginToken> Login(LoginUser loginUser)
    {
        var user = _authService.GetUserByName(loginUser.Username);
        if (user == null) return ApiResponse.NotFound(Response);
        if (loginUser.Password != user.Password) return ApiResponse.Unauthorized(Response);
        return new ApiResponse<LoginToken>(_authService.GenerateLoginToken(user));
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public ActionResult<User> GetUser()
    {
        var user = _authService.GetUser(User);
        if (user == null) return NotFound();
        return user;
    }
}
