using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTOs;

namespace TaskManagement.Core.Interfaces
{
    public interface ITeamService
    {
        Task<List<TeamResponseDto>> GetAllTeamsAsync();
        Task<int> CreateTeamAsync(TeamCreateDto dto);
        Task AssignMemberAsync(int teamId, int userId);
    }
}
