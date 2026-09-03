using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.DTOs
{
    public record TeamCreateDto([Required, MaxLength(100)] string Name);
    public record AssignMemberDto([Required] int UserId);
    public record TeamResponseDto(int Id, string Name, DateTime CreatedAt, List<UserSummaryDto> Members);
    public record UserSummaryDto(int Id, string FullName, string Email, string Role);

    // Tasks
    public record TaskCreateDto(
        [Required, MaxLength(200)] string Title,
        string Description,
        TaskPriority Priority,
        DateTime? DueDate,
        int? AssignedToUserId
    );

    public record TaskUpdateStatusDto([Required] TaskItemStatus Status);

    public record TaskResponseDto(
        int Id,
        string Title,
        string Description,
        string Status,
        string Priority,
        DateTime? DueDate,
        DateTime CreatedAt,
        UserSummaryDto? AssignedTo,
        UserSummaryDto CreatedBy,
        int CommentsCount
    );

    // Comments
    public record CommentCreateDto([Required, MaxLength(1000)] string Content);
    public record CommentResponseDto(int Id, string Content, DateTime CreatedAt, UserSummaryDto Author);
}
