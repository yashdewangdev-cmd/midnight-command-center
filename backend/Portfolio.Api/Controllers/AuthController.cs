using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IUserProfileRepository userProfileRepository,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new user and returns JWT tokens.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Conflict(new { message = "A user with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = "Registration failed.", errors = result.Errors });

        // Create linked UserProfile
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            IdentityUserId = user.Id,
            DisplayName = request.DisplayName,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _userProfileRepository.CreateAsync(profile);

        // Link profile to identity user
        user.UserProfileId = profile.Id;
        await _userManager.UpdateAsync(user);

        return Ok(await GenerateAuthResponse(user, profile));
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var profile = await _userProfileRepository.GetByIdentityUserIdAsync(user.Id);
        if (profile is null)
            return StatusCode(500, new { message = "User profile not found. Data integrity issue." });

        return Ok(await GenerateAuthResponse(user, profile));
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal is null)
            return Unauthorized(new { message = "Invalid access token." });

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized(new { message = "Invalid token claims." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null ||
            user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        var profile = await _userProfileRepository.GetByIdentityUserIdAsync(user.Id);
        if (profile is null)
            return StatusCode(500, new { message = "User profile not found." });

        return Ok(await GenerateAuthResponse(user, profile));
    }

    // ─── Token Generation Helpers ──────────────────────────────────────

    private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user, UserProfile profile)
    {
        var token = GenerateJwtToken(user, profile);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddHours(2),
            UserProfile = new UserProfileDto
            {
                Id = profile.Id,
                DisplayName = profile.DisplayName,
                Email = profile.Email,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl,
                GitHubUrl = profile.GitHubUrl,
                LinkedInUrl = profile.LinkedInUrl,
                CreatedAt = profile.CreatedAt,
                ProjectCount = profile.Projects?.Count ?? 0
            }
        };
    }

    private string GenerateJwtToken(ApplicationUser user, UserProfile profile)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? _configuration["Jwt:Secret"]
            ?? "PortfolioEcosystem-SuperSecret-Key-Change-In-Production-2024!";
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? _configuration["Jwt:Issuer"]
            ?? "PortfolioEcosystem";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? _configuration["Jwt:Audience"]
            ?? "PortfolioEcosystemClient";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, profile.DisplayName),
            new("UserProfileId", profile.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? _configuration["Jwt:Secret"]
            ?? "PortfolioEcosystem-SuperSecret-Key-Change-In-Production-2024!";

        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false // Allow expired tokens for refresh
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
