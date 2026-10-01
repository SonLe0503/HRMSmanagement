namespace HRManagement.Authorization
{
    /// <summary>
    /// Role cấp hệ thống, không lưu trong bảng Roles (bảng đó thuộc từng công ty).
    /// Được gán vào JWT cho tài khoản không thuộc công ty nào (User.CompanyId = null).
    /// </summary>
    public static class PlatformRoles
    {
        public const string SuperAdmin = "SUPERADMIN";
    }
}
