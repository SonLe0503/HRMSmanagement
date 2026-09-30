namespace HRManagement.Models;

public partial class RoleMenu
{
    public int RoleId { get; set; }

    public int MenuId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Menu Menu { get; set; } = null!;
}
