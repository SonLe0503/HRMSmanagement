namespace HRManagement.Services.FileStorages
{
    public interface IFileStorageService
    {
        Task<string> SaveBase64ImageAsync(string base64Image, string folderName, string filePrefix);
    }
}
