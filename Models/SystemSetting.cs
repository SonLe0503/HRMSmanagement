using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class SystemSetting
{
    public int SettingId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string SettingCategory { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }
}
