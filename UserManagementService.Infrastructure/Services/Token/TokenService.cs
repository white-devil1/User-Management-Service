using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Infrastructure.Services.Token;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSettings["Key"] ?? throw new Exception("JWT Key not found")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("TenantId", user.TenantId.ToString()),
            new("BranchId", user.BranchId?.ToString() ?? ""),
            new("IsSuperAdmin", user.IsSuperAdmin.ToString()),
            new("UserId", user.Id)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var expirationMinutes = jwtSettings["AccessTokenExpirationMinutes"] ?? "15";
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(expirationMinutes));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Permissions JWT — carries the full access surface (permissions + apps + pages).
    public string GeneratePermissionsToken(string userId, UserAccessDto access)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSettings["Key"] ?? throw new Exception("JWT Key not found")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("token_type", "permissions"),  // ← Identify this as permissions token
            new("UserId", userId)
        };

        foreach (var permission in access.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        // Apps tree (apps -> pages -> permissions) serialized as JSON so the
        // frontend can rebuild navigation without an extra round-trip. JWT
        // claims are flat strings, so one JSON payload beats one claim per node.
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        claims.Add(new Claim(
            "apps",
            JsonSerializer.Serialize(access.Apps, jsonOptions),
            JsonClaimValueTypes.JsonArray));

        // Same expiration as access token
        var expirationMinutes = jwtSettings["AccessTokenExpirationMinutes"] ?? "15";
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(expirationMinutes));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public DateTime GetAccessTokenExpiration()
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var minutes = jwtSettings["AccessTokenExpirationMinutes"] ?? "15";
        return DateTime.UtcNow.AddMinutes(Convert.ToDouble(minutes));
    }

    public DateTime GetRefreshTokenExpiration()
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var minutes = jwtSettings["RefreshTokenExpirationMinutes"] ??
                      jwtSettings["AccessTokenExpirationMinutes"] ?? "15";
        return DateTime.UtcNow.AddMinutes(Convert.ToDouble(minutes));
    }
}