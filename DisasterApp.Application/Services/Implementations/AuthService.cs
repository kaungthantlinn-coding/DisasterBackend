using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Google.Apis.Auth;

namespace DisasterApp.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        // For OAuth users, they don't have a password stored
        if (user.AuthProvider != "local")
            throw new UnauthorizedAccessException("Please use social login for this account");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.AuthId))
            throw new UnauthorizedAccessException("Invalid email or password");

        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhotoUrl = user.PhotoUrl,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponseDto> SignupAsync(SignupRequestDto request)
    {
        if (await _userRepository.ExistsAsync(request.Email))
            throw new InvalidOperationException("User with this email already exists");

        if (!request.AgreeToTerms)
            throw new InvalidOperationException("You must agree to the terms of service");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.FullName,
            Email = request.Email,
            AuthProvider = "local",
            AuthId = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            IsBlacklisted = false
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var roles = new List<string> { "User" }; // Default role
        var accessToken = GenerateAccessToken(createdUser, roles);
        var refreshToken = await GenerateRefreshTokenAsync(createdUser.UserId);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                UserId = createdUser.UserId,
                Name = createdUser.Name,
                Email = createdUser.Email,
                PhotoUrl = createdUser.PhotoUrl,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"] ?? throw new InvalidOperationException("Google Client ID not configured");

            // Verify the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            if (payload == null)
                throw new UnauthorizedAccessException("Invalid Google token");

            // Check if user exists
            var existingUser = await _userRepository.GetByEmailAsync(payload.Email);

            if (existingUser != null)
            {
                // User exists, log them in
                if (existingUser.AuthProvider != "google")
                {
                    // Update existing local user to Google auth
                    existingUser.AuthProvider = "google";
                    existingUser.AuthId = payload.Subject;
                    existingUser.PhotoUrl = payload.Picture;
                    await _userRepository.UpdateAsync(existingUser);
                }

                var roles = await _userRepository.GetUserRolesAsync(existingUser.UserId);
                var accessToken = GenerateAccessToken(existingUser, roles);
                var refreshToken = await GenerateRefreshTokenAsync(existingUser.UserId);

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserDto
                    {
                        UserId = existingUser.UserId,
                        Name = existingUser.Name,
                        Email = existingUser.Email,
                        PhotoUrl = existingUser.PhotoUrl,
                        Roles = roles
                    }
                };
            }
            else
            {
                // Create new user
                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Name = payload.Name,
                    Email = payload.Email,
                    AuthProvider = "google",
                    AuthId = payload.Subject,
                    PhotoUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow,
                    IsBlacklisted = false
                };

                var createdUser = await _userRepository.CreateAsync(newUser);
                var roles = new List<string> { "User" }; // Default role
                var accessToken = GenerateAccessToken(createdUser, roles);
                var refreshToken = await GenerateRefreshTokenAsync(createdUser.UserId);

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserDto
                    {
                        UserId = createdUser.UserId,
                        Name = createdUser.Name,
                        Email = createdUser.Email,
                        PhotoUrl = createdUser.PhotoUrl,
                        Roles = roles
                    }
                };
            }
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            throw new UnauthorizedAccessException("Failed to authenticate with Google", ex);
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.ExpiredAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = refreshToken.User;
        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        var newAccessToken = GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.UserId);

        // Delete old refresh token
        await _refreshTokenRepository.DeleteAsync(request.RefreshToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhotoUrl = user.PhotoUrl,
                Roles = roles
            }
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        return await _refreshTokenRepository.DeleteAsync(refreshToken);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateAccessToken(User user, List<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        return await _refreshTokenRepository.CreateAsync(refreshToken);
    }
}