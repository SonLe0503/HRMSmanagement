using HRManagement.Models;

namespace HRManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HrmsDbContext _context;

        public NotificationService(HrmsDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task CreateNotificationAsync(
    int recipientUserId,
    string type,
    string title,
    string message,
    string? relatedEntity = null,
    int? relatedEntityId = null)
        {
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                NotificationType = type,
                Title = title,
                Message = message,
                RelatedEntity = relatedEntity,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                SentDate = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
