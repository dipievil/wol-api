using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace WOL_TokenGen;

class Program
{
    static void Main(string[] args)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "super-secret-development-key-please-change";
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "wol-server";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "wol-clients";
        var subject = args.Length > 0 ? args[0] : "user-1";
        var minutes = args.Length > 1 && int.TryParse(args[1], out var m) ? m : 60;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        Console.WriteLine($"JWT_KEY={jwtKey}");
        Console.WriteLine($"JWT_ISSUER={issuer}");
        Console.WriteLine($"JWT_AUDIENCE={audience}");
        Console.WriteLine(tokenString);
    }
}