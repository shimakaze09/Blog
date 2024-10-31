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
    private readonly SecuritySettings _securitySettings;
    private readonly IBaseRepository<User> _userRepo;

    public AuthService(IOptions<SecuritySettings> options, IBaseRepository<User> userRepo)
    {
        _securitySettings = options.Value;
        _userRepo = userRepo;
    }

    public LoginToken GenerateLoginToken(User user)
    {
        var claims = new List<Claim>
        {
            new("username", user.Name),
            new(JwtRegisteredClaimNames.Name, user.Id), // User.Identity.Name
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securitySettings.Token.Key));
        var signCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(
            _securitySettings.Token.Issuer,
            _securitySettings.Token.Audience,
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: signCredential);

        return new LoginToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            Expiration = TimeZoneInfo.ConvertTimeFromUtc(jwtToken.ValidTo, TimeZoneInfo.Local)
        };
    }

    public User? GetUserById(string userId)
    {
        return _userRepo.Where(a => a.Id == userId).ToOne();
    }

    public User? GetUserByName(string name)
    {
        return _userRepo.Where(a => a.Name == name).ToOne();
    }

    public User? GetUser(ClaimsPrincipal userClaim)
    {
        var userId = userClaim.Identity?.Name;
        var userName = userClaim.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        if (userId == null || userName == null) return null;
        return new User { Id = userId, Name = userName };
    }
}