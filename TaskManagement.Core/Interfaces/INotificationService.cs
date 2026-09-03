using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskAssignedAsync(string recipientEmail, string taskTitle, string assignedByName);
        Task NotifyTaskStatusChangedAsync(string recipientEmail, string taskTitle, string newStatus);
    }
}
