using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services.Employees
{
    public interface IEmployeeDocumentService
    {
        Task<IEnumerable<EmployeeDocumentListDto>> GetEmployeeDocumentsAsync(int employeeId);
        Task<EmployeeDocumentResponseDto?> GetDocumentByIdAsync(int documentId);
        Task<EmployeeDocumentResponseDto> UploadDocumentAsync(UploadEmployeeDocumentDto uploadDto, IFormFile file);
        Task<EmployeeDocumentResponseDto?> UpdateDocumentAsync(int documentId, UpdateEmployeeDocumentDto updateDto);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<(byte[] fileContent, string fileName, string contentType)?> DownloadDocumentAsync(int documentId);
        Task<IEnumerable<EmployeeDocumentListDto>> GetDocumentsByCategoryAsync(int employeeId, string category);

    }
}
