using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{


    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserSummaryDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.User)
                .Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.Role.ToString()))
                .ToListAsync();
        }

        public async Task<List<UserSummaryDto>> GetAssignableUsersAsync(int currentUserId, string currentUserRole)
        {
            if (currentUserRole == nameof(UserRole.Admin))
            {
                return await _context.Users
                    .Where(u => u.Role == UserRole.Manager || u.Role == UserRole.User)
                    .Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.Role.ToString()))
                    .ToListAsync();
            }

            if (currentUserRole == nameof(UserRole.Manager))
            {
                var manager = await _context.Users.FindAsync(currentUserId);
                if (manager?.TeamId == null)
                {
                    return new List<UserSummaryDto>();
                }

                return await _context.Users
                    .Where(u => u.TeamId == manager.TeamId
                             && u.Role == UserRole.User
                             && u.Id != currentUserId)
                    .Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.Role.ToString()))
                    .ToListAsync();
            }

            return new List<UserSummaryDto>();
        }
    }
}

