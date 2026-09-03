using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _teamService.GetAllTeamsAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeam([FromBody] TeamCreateDto dto)
        {
            var id = await _teamService.CreateTeamAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id }, dto);
        }

        [HttpPost("{teamId}/members")]
        [Authorize(Roles = "Admin,Manager")] 
        public async Task<IActionResult> AssignMember(int teamId, [FromBody] AssignMemberDto dto)
        {
            await _teamService.AssignMemberAsync(teamId, dto.UserId);
            return Ok(new { message = "Member assigned to team successfully." });
        }
    }
}
