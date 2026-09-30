namespace HRManagement.DTOs
{
    /// <summary>Node of the current user's own menu (what the sidebar renders).</summary>
    public class MyMenuNodeDTO
    {
        public int MenuId { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? IconName { get; set; }
        public List<MyMenuNodeDTO> Children { get; set; } = new();
    }

    /// <summary>Node of the full menu tree, for the "Quản lý menu" admin screen.</summary>
    public class MenuAdminNodeDTO
    {
        public int MenuId { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? IconName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public bool? IsGranted { get; set; }
        public List<MenuAdminNodeDTO> Children { get; set; } = new();
    }

    public class MenuWriteDTO
    {
        public int? ParentId { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? IconName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int>? RoleIds { get; set; }
    }

    public class UpdateRoleMenusDTO
    {
        public List<int> MenuIds { get; set; } = new();
    }
}
