using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Share.Extensions;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Auth;

namespace Web.Apis;

/// <summary>
///     Authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Auth)]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    ///     Login
    /// </summary>
    /// <param name="loginUser">The login user object</param>
    /// <returns>The login response</returns>
    [HttpPost]
    [Route("")]
    [Route("[action]")]
    [ProducesResponseType(typeof(ApiResponse<LoginToken>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> Login(LoginUser loginUser)
    {
        var user = await _authService.GetUserByName(loginUser.Username);
        if (user == null) return ApiResponse.Unauthorized("Username or password incorrect");
        if (loginUser.Password.ToSHA256() != user.Password)
            return ApiResponse.Unauthorized("Username or password incorrect");
        return ApiResponse.Ok(_authService.GenerateLoginToken(user));
    }


    /// <summary>
    ///     Get current user information
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("")]
    [Route("[action]")]
    public ApiResponse<User> GetUser()
    {
        var user = _authService.GetUser(User);
        if (user == null) return ApiResponse.NotFound("User information not found");
        return new ApiResponse<User>(user);
    }
}