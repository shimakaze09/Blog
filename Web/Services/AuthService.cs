using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.Models;
using FreeSql;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Web.Models.Config;
using Web.ViewModels.Auth;

namespace Web.Services;

public class AuthService
{
    private const string ClaimUserId = "user_id";
    private const string ClaimUserName = "user_name";
    private readonly Auth _auth;
    private readonly IBaseRepository<User> _userRepo;

    public AuthService(IOptions<Auth> options, IBaseRepository<User> userRepo)
    {
        _auth = options.Value;
        _userRepo = userRepo;
    }

    public LoginToken GenerateLoginToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimUserId, user.Id), // User.Identity.Name
            new(ClaimUserName, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_auth.Jwt.Key));
        // TODO: Use asymmetric encryption for JWT (RSA)
        var signCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(
            _auth.Jwt.Issuer,
            _auth.Jwt.Audience,
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: signCredential
        );

        // TODO: try using jose-jwt to generate JWT
        return new LoginToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            Expiration = TimeZoneInfo.ConvertTimeFromUtc(jwtToken.ValidTo, TimeZoneInfo.Local)
        };
    }

    public async Task<User?> GetUserById(string userId)
    {
        return await _userRepo.Where(a => a.Id == userId).FirstAsync();
    }

    public async Task<User?> GetUserByName(string name)
    {
        return await _userRepo.Where(a => a.Name == name).FirstAsync();
    }

    public User? GetUser(ClaimsPrincipal userClaim)
    {
        var userId = userClaim.FindFirstValue(ClaimUserId);
        var userName = userClaim.FindFirstValue(ClaimUserName);
        if (userId == null || userName == null) return null;
        return new User { Id = userId, Name = userName };
    }
}