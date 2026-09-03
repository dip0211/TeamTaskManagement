using System.ComponentModel.DataAnnotations;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.DTOs
{
    public record RegisterRequestDto(
    [Required, MaxLength(100)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    UserRole Role
);

    public record LoginRequestDto(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

    public record AuthResponseDto(
        int Id,
    string FullName,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
    );

    public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);
}
