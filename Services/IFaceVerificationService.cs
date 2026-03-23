using HRManagement.DTOs.Attendances;

namespace HRManagement.Services
{
    public interface IFaceVerificationService
    {
        Task<FaceVerificationResultDto> VerifyAsync(int employeeId, string faceImageBase64, string verificationType, string? deviceInfo, string? ipAddress, string? location);
        Task<string> RegisterFaceAsync(int employeeId, string referenceImageBase64);

    }
}
