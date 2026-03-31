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
        private readonly FaceEmbeddingService _faceEmbeddingService;

        private const decimal MATCH_THRESHOLD = 0.363m;

        public FaceVerificationService(
            IAttendanceRepository attendanceRepository,
            IFileStorageService fileStorageService,
            FaceEmbeddingService faceEmbeddingService)
        {
            _attendanceRepository = attendanceRepository;
            _fileStorageService = fileStorageService;
            _faceEmbeddingService = faceEmbeddingService;
        }

        public async Task<string> RegisterFaceAsync(int employeeId, string referenceImageBase64)
        {
            if (string.IsNullOrWhiteSpace(referenceImageBase64))
                throw new Exception("Reference image is required.");

            var imagePath = await _fileStorageService.SaveBase64ImageAsync(
                referenceImageBase64,
                "face-profiles",
                $"emp_{employeeId}"
            );

            var embedding = _faceEmbeddingService.ExtractEmbedding(imagePath);
            var embeddingBytes = FloatArrayToBytes(embedding);

            var existing = await _attendanceRepository.GetActiveFaceProfileByEmployeeIdAsync(employeeId);

            if (existing == null)
            {
                var profile = new FaceProfile
                {
                    EmployeeId = employeeId,
                    ReferenceImagePath = imagePath,
                    FaceEmbedding = embeddingBytes,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    CreatedBy = employeeId
                };

                await _attendanceRepository.AddFaceProfileAsync(profile);
            }
            else
            {
                existing.ReferenceImagePath = imagePath;
                existing.FaceEmbedding = embeddingBytes;
                existing.Status = "Active";
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
            if (string.IsNullOrWhiteSpace(faceImageBase64))
                throw new Exception("Face image is required.");

            var faceProfile = await _attendanceRepository.GetActiveFaceProfileByEmployeeIdAsync(employeeId);

            if (faceProfile == null || faceProfile.FaceEmbedding == null || faceProfile.FaceEmbedding.Length == 0)
            {
                var noProfileResult = new FaceVerificationResultDto
                {
                    IsMatch = false,
                    ConfidenceScore = 0,
                    ThresholdUsed = MATCH_THRESHOLD,
                    LivenessPassed = null,
                    FailureReason = "NoFaceProfile"
                };

                await SaveVerificationLogAsync(
                    employeeId,
                    verificationType,
                    faceImageBase64,
                    noProfileResult,
                    deviceInfo,
                    ipAddress,
                    location);

                return noProfileResult;
            }

            var capturedImagePath = await _fileStorageService.SaveBase64ImageAsync(
                faceImageBase64,
                "face-captures",
                $"{verificationType.ToLower()}_{employeeId}"
            );

            try
            {
                var capturedEmbedding = _faceEmbeddingService.ExtractEmbedding(capturedImagePath);
                var referenceEmbedding = BytesToFloatArray(faceProfile.FaceEmbedding);

                var similarity = _faceEmbeddingService.CosineSimilarity(referenceEmbedding, capturedEmbedding);
                var isMatch = similarity >= (float)MATCH_THRESHOLD;

                var result = new FaceVerificationResultDto
                {
                    IsMatch = isMatch,
                    ConfidenceScore = (decimal)similarity,
                    ThresholdUsed = MATCH_THRESHOLD,
                    LivenessPassed = null,
                    FailureReason = isMatch ? null : "FaceNotMatched",
                    CapturedImagePath = capturedImagePath
                };

                await SaveVerificationLogAsync(
                    employeeId,
                    verificationType,
                    null,
                    result,
                    deviceInfo,
                    ipAddress,
                    location,
                    capturedImagePath);

                return result;
            }
            catch (Exception ex)
            {
                var failedResult = new FaceVerificationResultDto
                {
                    IsMatch = false,
                    ConfidenceScore = 0m,
                    ThresholdUsed = MATCH_THRESHOLD,
                    LivenessPassed = null,
                    FailureReason = ex.Message,
                    CapturedImagePath = capturedImagePath
                };

                await SaveVerificationLogAsync(
                    employeeId,
                    verificationType,
                    null,
                    failedResult,
                    deviceInfo,
                    ipAddress,
                    location,
                    capturedImagePath);

                return failedResult;
            }
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
                capturedImagePath = await _fileStorageService.SaveBase64ImageAsync(
                    faceImageBase64,
                    "face-captures",
                    $"{verificationType.ToLower()}_{employeeId}"
                );
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

        private byte[] FloatArrayToBytes(float[] values)
        {
            var bytes = new byte[values.Length * sizeof(float)];
            Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private float[] BytesToFloatArray(byte[] bytes)
        {
            var values = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
            return values;
        }
    }
}