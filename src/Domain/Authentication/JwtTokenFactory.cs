using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities.Identity;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Domain.Authentication;

public static class JwtTokenFactory
{
    public static string CreateToken(
        User user,
        string signingKey,
        DateTime expireAt,
        string issuer,
        string audience)
    {
        if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(signingKey))
            return string.Empty;

        var claimList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName)
        };

        if (user.UserRoles.All(x => x.Role.Name != null))
            claimList.AddRange(user.UserRoles.Select(ur => ur.Role).Select(r => new Claim(ClaimTypes.Role, r.Name!)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            IssuedAt = DateTime.UtcNow,
            Subject = new ClaimsIdentity(claimList),
            Expires = expireAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}