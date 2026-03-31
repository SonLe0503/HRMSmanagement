using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services.FileStorages;
using HRManagement.DataAcess.Interfaces;

namespace HRManagement.Services.FaceVerifications
{
    public class FaceVerificationService : IFaceVerificationService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IFileStorageService _fileStorageService;

        public FaceVerificationService(IAttendanceRepository attendanceRepository, IFileStorageService fileStorageService)
        {
            _attendanceRepository = attendanceRepository;
            _fileStorageService = fileStorageService;
        }
        public async Task<string> RegisterFaceAsync(int employeeId, string referenceImageBase64)
        {
            var imagePath = await _fileStorageService.SaveBase64ImageAsync(referenceImageBase64, "face-profiles", $"emp_{employeeId}");

            var existing = await _attendanceRepository.GetActiveFaceProfileByEmployeeIdAsync(employeeId);

            if (existing == null)
            {
                var profile = new FaceProfile
                {
                    EmployeeId = employeeId,
                    ReferenceImagePath = imagePath,
                    FaceEmbedding = null,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    CreatedBy = employeeId
                };

                await _attendanceRepository.AddFaceProfileAsync(profile);
            }
            else
            {
                existing.ReferenceImagePath = imagePath;
                existing.ModifiedDate = DateTime.Now;
                existing.ModifiedBy = employeeId;

                await _attendanceRepository.UpdateFaceProfileAsync(existing);
            }

            await _attendanceRepository.SaveChangesAsync();
            return imagePath;
        }

        public async Task<FaceVerificationResultDto> VerifyAsync(
            int employeeId,
            string faceImageBase64,
            string verificationType,
            string? deviceInfo,
            string? ipAddress,
            string? location)
        {
            var faceProfile = await _attendanceRepository.GetActiveFaceProfileByEmployeeIdAsync(employeeId);

            if (faceProfile == null)
            {
                var noProfileResult = new FaceVerificationResultDto
                {
                    IsMatch = false,
                    ConfidenceScore = 0,
                    ThresholdUsed = 80,
                    FailureReason = "NoFaceProfile"
                };

                await SaveVerificationLogAsync(employeeId, verificationType, faceImageBase64, noProfileResult, deviceInfo, ipAddress, location);
                return noProfileResult;
            }

            var capturedImagePath = await _fileStorageService.SaveBase64ImageAsync(faceImageBase64, "face-captures", $"{verificationType.ToLower()}_{employeeId}");

            // MVP giả lập:
            // Tạm thời: nếu đã có face profile thì cho pass để test luồng.
            // Sau này thay bằng gọi Python service / AI thật.
            var result = new FaceVerificationResultDto
            {
                IsMatch = true,
                ConfidenceScore = 95,
                ThresholdUsed = 80,
                LivenessPassed = true,
                FailureReason = null,
                CapturedImagePath = capturedImagePath
            };

            await SaveVerificationLogAsync(employeeId, verificationType, null, result, deviceInfo, ipAddress, location, capturedImagePath);

            return result;
        }

        private async System.Threading.Tasks.Task SaveVerificationLogAsync(
            int employeeId,
            string verificationType,
            string? faceImageBase64,
            FaceVerificationResultDto result,
            string? deviceInfo,
            string? ipAddress,
            string? location,
            string? capturedImagePath = null)
        {
            if (capturedImagePath == null && !string.IsNullOrWhiteSpace(faceImageBase64))
            {
                capturedImagePath = await _fileStorageService.SaveBase64ImageAsync(faceImageBase64, "face-captures", $"{verificationType.ToLower()}_{employeeId}");
            }

            var log = new FaceVerificationLog
            {
                EmployeeId = employeeId,
                AttendanceLogId = null,
                VerificationType = verificationType,
                CapturedImagePath = capturedImagePath,
                ConfidenceScore = result.ConfidenceScore,
                ThresholdUsed = result.ThresholdUsed,
                IsMatch = result.IsMatch,
                LivenessPassed = result.LivenessPassed,
                FailureReason = result.FailureReason,
                VerifiedAt = DateTime.Now,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                Location = location,
                CreatedBy = employeeId
            };

            await _attendanceRepository.AddFaceVerificationLogAsync(log);
            await _attendanceRepository.SaveChangesAsync();
        }
    }
}
