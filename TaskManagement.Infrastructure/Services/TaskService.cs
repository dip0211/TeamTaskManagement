using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public TaskService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, string role, TaskItemStatus? status, TaskPriority? priority, DateTime? deadline)
        {
            var query = _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Comments)
                .AsQueryable();

            // RBAC: Standard users only view tasks assigned to them
            if (role == nameof(UserRole.User))
            {
                query = query.Where(t => t.AssignedToUserId == currentUserId);
            }

            if (status.HasValue) query = query.Where(t => t.Status == status.Value);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
            if (deadline.HasValue) query = query.Where(t => t.DueDate <= deadline.Value);

            return await query.Select(t => new TaskResponseDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.DueDate,
                t.CreatedAt,
                t.AssignedToUser != null ? new UserSummaryDto(t.AssignedToUser.Id, t.AssignedToUser.FullName, t.AssignedToUser.Email, t.AssignedToUser.Role.ToString()) : null,
                new UserSummaryDto(t.CreatedByUser.Id, t.CreatedByUser.FullName, t.CreatedByUser.Email, t.CreatedByUser.Role.ToString()),
                t.Comments.Count
            )).ToListAsync();
        }

        public async Task<int> CreateTaskAsync(int creatorUserId, string creatorName, string creatorRole, TaskCreateDto dto)
        {
            if (dto.AssignedToUserId.HasValue)
            {
                var assignee = await _context.Users.FindAsync(dto.AssignedToUserId.Value)
                    ?? throw new KeyNotFoundException("Assignee not found.");

                if (creatorRole == nameof(UserRole.Manager))
                {
                    var manager = await _context.Users.FindAsync(creatorUserId);
                    if (manager?.TeamId == null ||
                        assignee.TeamId != manager.TeamId ||
                        assignee.Role != UserRole.User)
                    {
                        throw new UnauthorizedAccessException("Managers can only assign tasks to team users within their own team.");
                    }
                }
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                CreatedByUserId = creatorUserId,
                AssignedToUserId = dto.AssignedToUserId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            if (task.AssignedToUserId.HasValue)
            {
                var assignee = await _context.Users.FindAsync(task.AssignedToUserId.Value);
                if (assignee != null)
                {
                    await _notificationService.NotifyTaskAssignedAsync(assignee.Email, task.Title, creatorName);
                }
            }

            return task.Id;
        }

        public async Task UpdateTaskStatusAsync(int taskId, int currentUserId, string role, TaskItemStatus newStatus)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new KeyNotFoundException("Task not found.");

            if (role == nameof(UserRole.User) && task.AssignedToUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Forbidden: You can only update tasks assigned to you.");
            }

            task.Status = newStatus;
            await _context.SaveChangesAsync();

            if (task.AssignedToUser != null)
            {
                await _notificationService.NotifyTaskStatusChangedAsync(task.AssignedToUser.Email, task.Title, newStatus.ToString());
            }
        }

        public async Task AddCommentAsync(int taskId, int currentUserId, string content)
        {
            var exists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
            if (!exists) throw new KeyNotFoundException("Task not found.");

            var comment = new Comment
            {
                TaskItemId = taskId,
                UserId = currentUserId,
                Content = content
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CommentResponseDto>> GetCommentsAsync(int taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskItemId == taskId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponseDto(
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    new UserSummaryDto(c.User.Id, c.User.FullName, c.User.Email, c.User.Role.ToString())
                ))
                .ToListAsync();
        }
    }
}
