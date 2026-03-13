using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IEmployeeDocumentRepository
    {
        Task<IEnumerable<EmployeeDocument>> GetDocumentsByEmployeeIdAsync(int employeeId);
        Task<EmployeeDocument?> GetDocumentByIdAsync(int documentId);
        Task<EmployeeDocument> AddDocumentAsync(EmployeeDocument document);
        Task<EmployeeDocument> UpdateDocumentAsync(EmployeeDocument document);
        Task<EmployeeDocument?> GetDocumentByIdWithDetailsAsync(int documentId);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<bool> ExistsAsync(int documentId);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task<IEnumerable<EmployeeDocument>> GetDocumentsByCategoryAsync(int employeeId, string category);

    }
}
