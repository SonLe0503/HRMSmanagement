using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class AuditLog
{
    public int AuditId { get; set; }

    public string TableName { get; set; } = null!;

    public string Action { get; set; } = null!;

    public int RecordId { get; set; }

    public int? UserId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime ActionDate { get; set; }

    public string? Ipaddress { get; set; }

    public virtual User? User { get; set; }
}
