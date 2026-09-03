using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, string role, TaskItemStatus? status, TaskPriority? priority, DateTime? deadline);
        Task<int> CreateTaskAsync(int creatorUserId, string creatorName, string creatorRole, TaskCreateDto dto);
        Task UpdateTaskStatusAsync(int taskId, int currentUserId, string role, TaskItemStatus newStatus);
        Task AddCommentAsync(int taskId, int currentUserId, string content);
        Task<List<CommentResponseDto>> GetCommentsAsync(int taskId);
    }
}
