using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests
{
    public class AuthServiceTests
    {
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockHasher = new Mock<IPasswordHasher>();
            var mockToken = new Mock<ITokenService>();
            var authService = new AuthService(context, mockHasher.Object, mockToken.Object);

            context.Users.Add(new User { Email = "admin@taskmgmt.com", FullName = "Admin User" });
            await context.SaveChangesAsync();

            var request = new RegisterRequestDto("New Guy", "admin@taskmgmt.com", "Secret123!", UserRole.User);

            // Act
            Func<Task> act = async () => await authService.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already registered*");
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockHasher = new Mock<IPasswordHasher>();
            mockHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            var mockToken = new Mock<ITokenService>();

            var authService = new AuthService(context, mockHasher.Object, mockToken.Object);

            context.Users.Add(new User { Email = "test@company.com", PasswordHash = "hashed_val" });
            await context.SaveChangesAsync();

            var request = new LoginRequestDto("test@company.com", "WrongPassword");

            // Act
            Func<Task> act = async () => await authService.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid credentials*");
        }

        [Fact]
        public async Task RefreshTokenAsync_RevokedToken_ThrowsSecurityException()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockHasher = new Mock<IPasswordHasher>();
            var mockToken = new Mock<ITokenService>();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));
            mockToken.Setup(t => t.GetPrincipalFromExpiredToken(It.IsAny<string>())).Returns(claimsPrincipal);

            var authService = new AuthService(context, mockHasher.Object, mockToken.Object);

            var user = new User { Id = 1, Email = "user@test.com", FullName = "Dev" };
            var revokedToken = new RefreshToken
            {
                Token = "revoked_token_123",
                UserId = 1,
                IsRevoked = true,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            context.Users.Add(user);
            context.RefreshTokens.Add(revokedToken);
            await context.SaveChangesAsync();

            var request = new RefreshTokenRequestDto("expired_access_token", "revoked_token_123");

            // Act
            Func<Task> act = async () => await authService.RefreshTokenAsync(request);

            // Assert
            await act.Should().ThrowAsync<System.Security.SecurityException>()
                .WithMessage("*Invalid or expired refresh token*");
        }
    }
}