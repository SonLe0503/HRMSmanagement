using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess
{
    public class EmployeeDocumentRepository : IEmployeeDocumentRepository
    {
        private readonly HrmsDbContext _context;
        public EmployeeDocumentRepository(HrmsDbContext context)
        {
            _context = context;
        }
        public async Task<EmployeeDocument> AddDocumentAsync(EmployeeDocument document)
        {
            _context.EmployeeDocuments.Add(document);
            await _context.SaveChangesAsync();
            return document;
            
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await _context.EmployeeDocuments.FindAsync(documentId);
            if (document == null)
                return false;

            _context.EmployeeDocuments.Remove(document);
            return await _context.SaveChangesAsync() > 0;
           
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> ExistsAsync(int documentId)
        {
            return await _context.EmployeeDocuments.AnyAsync(d => d.DocumentId == documentId);
        }

        public async Task<EmployeeDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.EmployeeDocuments.Include(e => e.Employee).FirstOrDefaultAsync(d => d.DocumentId == documentId);
        }

        public async Task<EmployeeDocument?> GetDocumentByIdWithDetailsAsync(int documentId)
        {
            return await _context.EmployeeDocuments.Include(e => e.Employee).FirstOrDefaultAsync(d => d.DocumentId == documentId);
        }

        public async Task<IEnumerable<EmployeeDocument>> GetDocumentsByCategoryAsync(int employeeId, string category)
        {
            return await _context.EmployeeDocuments.Include(e=>e.Employee).Where(e=>e.EmployeeId == employeeId && e.DocumentCategory == category).ToListAsync();
        }

        public async Task<IEnumerable<EmployeeDocument>> GetDocumentsByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeDocuments.Include(e => e.Employee).Where(e => e.EmployeeId == employeeId).ToListAsync();
        }

        public async Task<EmployeeDocument> UpdateDocumentAsync(EmployeeDocument document)
        {
            _context.EmployeeDocuments.Update(document);
            await _context.SaveChangesAsync();
            return document;
        }
    }
}
