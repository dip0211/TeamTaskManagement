using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTOs;

namespace TaskManagement.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<UserSummaryDto>> GetAllUsersAsync();
        Task<List<UserSummaryDto>> GetAssignableUsersAsync(int currentUserId, string currentUserRole);
    }
}
