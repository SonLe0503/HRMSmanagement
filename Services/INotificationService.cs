namespace HRManagement.Services
{
    public interface INotificationService
    {
        System.Threading.Tasks.Task CreateNotificationAsync(
        int recipientUserId,
        string type,
        string title,
        string message,
        string? relatedEntity = null,
        int? relatedEntityId = null);
    }
}
