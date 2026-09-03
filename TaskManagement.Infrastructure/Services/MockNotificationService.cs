using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class MockNotificationService : INotificationService
    {
        private readonly ILogger<MockNotificationService> _logger;

        public MockNotificationService(ILogger<MockNotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyTaskAssignedAsync(string recipientEmail, string taskTitle, string assignedByName)
        {
            _logger.LogInformation("[NOTIFICATION DISPATCHED] To: {Email} | Event: Task Assigned | Task: '{Title}' | By: {Assigner}",
                recipientEmail, taskTitle, assignedByName);
            return Task.CompletedTask;
        }

        public Task NotifyTaskStatusChangedAsync(string recipientEmail, string taskTitle, string newStatus)
        {
            _logger.LogInformation("[NOTIFICATION DISPATCHED] To: {Email} | Event: Status Changed | Task: '{Title}' | New Status: {Status}",
                recipientEmail, taskTitle, newStatus);
            return Task.CompletedTask;
        }
    }
}
