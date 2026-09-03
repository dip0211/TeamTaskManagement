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

    public class TaskServiceTests
    {
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task Manager_AssigningTaskToOwnTeamMember_SucceedsAndNotifies()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockNotification = new Mock<INotificationService>();
            var taskService = new TaskService(context, mockNotification.Object);

            var team = new Team { Id = 1, Name = "Engineering" };
            var manager = new User { Id = 10, FullName = "Alice Manager", Email = "alice@test.com", Role = UserRole.Manager, TeamId = 1 };
            var member = new User { Id = 20, FullName = "Bob Dev", Email = "bob@test.com", Role = UserRole.User, TeamId = 1 };

            context.Teams.Add(team);
            context.Users.AddRange(manager, member);
            await context.SaveChangesAsync();

            var dto = new TaskCreateDto("Build Auth", "Implement JWT", TaskPriority.High, DateTime.UtcNow.AddDays(3), member.Id);

            // Act
            var taskId = await taskService.CreateTaskAsync(manager.Id, manager.FullName, nameof(UserRole.Manager), dto);

            // Assert
            taskId.Should().BeGreaterThan(0);
            mockNotification.Verify(
                n => n.NotifyTaskAssignedAsync("bob@test.com", "Build Auth", "Alice Manager"),
                Times.Once
            );
        }

        [Fact]
        public async Task Manager_AssigningTaskOutsideTeam_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockNotification = new Mock<INotificationService>();
            var taskService = new TaskService(context, mockNotification.Object);

            var teamA = new Team { Id = 1, Name = "Team A" };
            var teamB = new Team { Id = 2, Name = "Team B" };
            var manager = new User { Id = 10, FullName = "Alice Manager", Role = UserRole.Manager, TeamId = 1 };
            var outsideUser = new User { Id = 30, FullName = "Charlie Sales", Role = UserRole.User, TeamId = 2 };

            context.Teams.AddRange(teamA, teamB);
            context.Users.AddRange(manager, outsideUser);
            await context.SaveChangesAsync();

            var dto = new TaskCreateDto("Cross-team Task", "Should fail", TaskPriority.Low, null, outsideUser.Id);

            // Act
            Func<Task> act = async () => await taskService.CreateTaskAsync(manager.Id, manager.FullName, nameof(UserRole.Manager), dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Managers can only assign tasks to team users within their own team*");
        }

        [Fact]
        public async Task RegularUser_UpdatingAnotherUsersTaskStatus_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockNotification = new Mock<INotificationService>();
            var taskService = new TaskService(context, mockNotification.Object);

            var task = new TaskItem
            {
                Id = 100,
                Title = "Private Task",
                AssignedToUserId = 5,
                CreatedByUserId = 1,
                Status = TaskItemStatus.ToDo
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            int unauthorizedUserId = 999;

            // Act
            Func<Task> act = async () => await taskService.UpdateTaskStatusAsync(100, unauthorizedUserId, nameof(UserRole.User), TaskItemStatus.Done);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Forbidden*");
        }

        [Fact]
        public async Task UpdateTaskStatus_ValidAssignee_DispatchesStatusNotification()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var mockNotification = new Mock<INotificationService>();
            var taskService = new TaskService(context, mockNotification.Object);

            var assignee = new User { Id = 5, FullName = "Dev User", Email = "dev@test.com" };
            var task = new TaskItem
            {
                Id = 1,
                Title = "Deploy to Production",
                AssignedToUserId = assignee.Id,
                AssignedToUser = assignee,
                CreatedByUserId = 1,
                Status = TaskItemStatus.InProgress
            };
            context.Users.Add(assignee);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            // Act
            await taskService.UpdateTaskStatusAsync(task.Id, assignee.Id, nameof(UserRole.User), TaskItemStatus.Done);

            // Assert
            var updated = await context.Tasks.FindAsync(task.Id);
            updated!.Status.Should().Be(TaskItemStatus.Done);
            mockNotification.Verify(
                n => n.NotifyTaskStatusChangedAsync("dev@test.com", "Deploy to Production", "Done"),
                Times.Once
            );
        }
    }
}