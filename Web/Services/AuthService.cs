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
            new(JwtRegisteredClaimNames.Sub, user.Id), // User.Identity.Name
            new(JwtRegisteredClaimNames.GivenName, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_auth.Jwt.Key));
        var signCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(
            issuer: _auth.Jwt.Issuer,
            audience: _auth.Jwt.Audience,
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: signCredential);

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
        var userId = userClaim.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var userName = userClaim.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
        var temp = userClaim.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
        if (userId == null || userName == null) return null;
        return new User { Id = userId, Name = userName };
    }
}