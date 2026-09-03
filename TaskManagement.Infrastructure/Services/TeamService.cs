using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class TeamService : ITeamService
    {
        private readonly AppDbContext _context;

        public TeamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamResponseDto>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.Members)
                .Select(t => new TeamResponseDto(
                    t.Id,
                    t.Name,
                    t.CreatedAt,
                    t.Members.Select(m => new UserSummaryDto(m.Id, m.FullName, m.Email, m.Role.ToString())).ToList()
                ))
                .ToListAsync();
        }

        public async Task<int> CreateTeamAsync(TeamCreateDto dto)
        {
            var team = new Team { Name = dto.Name };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team.Id;
        }

        public async Task AssignMemberAsync(int teamId, int userId)
        {
            var team = await _context.Teams.FindAsync(teamId)
                       ?? throw new KeyNotFoundException("Team not found.");
            var user = await _context.Users.FindAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            user.TeamId = teamId;
            await _context.SaveChangesAsync();
        }
    }
}
