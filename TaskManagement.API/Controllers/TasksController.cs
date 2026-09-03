using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role)!;
        private string CurrentUserName => User.FindFirstValue(ClaimTypes.Name)!;

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskItemStatus? status, [FromQuery] TaskPriority? priority, [FromQuery] DateTime? deadline)
        {
            var tasks = await _taskService.GetTasksAsync(CurrentUserId, CurrentUserRole, status, priority, deadline);
            return Ok(tasks);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
        {
            var id = await _taskService.CreateTaskAsync(CurrentUserId, CurrentUserName, CurrentUserRole, dto);
            return CreatedAtAction(nameof(GetTasks), new { id }, id);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] TaskUpdateStatusDto dto)
        {
            await _taskService.UpdateTaskStatusAsync(id, CurrentUserId, CurrentUserRole, dto.Status);
            return Ok(new { message = "Status updated successfully." });
        }

        [HttpPost("{taskId}/comments")]
        public async Task<IActionResult> AddComment(int taskId, [FromBody] CommentCreateDto dto)
        {
            await _taskService.AddCommentAsync(taskId, CurrentUserId, dto.Content);
            return Ok(new { message = "Comment created." });
        }

        [HttpGet("{taskId}/comments")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            return Ok(await _taskService.GetCommentsAsync(taskId));
        }
    }
}
