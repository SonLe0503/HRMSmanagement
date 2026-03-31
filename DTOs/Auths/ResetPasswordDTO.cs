namespace HRManagement.DTOs.Auths
{
    public class ResetPasswordDTO
    {
        public string EmailOrUsername { get; set; } = null!;
        public string Otp { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
