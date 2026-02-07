using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int RecipientUserId { get; set; }

    public string NotificationType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? RelatedEntity { get; set; }

    public int? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTime SentDate { get; set; }

    public DateTime? ReadDate { get; set; }

    public virtual User RecipientUser { get; set; } = null!;
}
