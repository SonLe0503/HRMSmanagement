using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services.CurrentUsers;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services.HRProceduces
{
    public class HRProcedureService : IHRProcedureService
    {
        private readonly IHRProcedureRepository _hrProcedureRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly HrmsDbContext _context;
        private readonly Approvals.ITopLevelResolver _topLevelResolver;

        public HRProcedureService(
            IHRProcedureRepository hrProcedureRepository,
            IEmployeeRepository employeeRepository,
            ICurrentUserService currentUserService,
            HrmsDbContext context,
            Approvals.ITopLevelResolver topLevelResolver)
        {
            _hrProcedureRepository = hrProcedureRepository;
            _employeeRepository = employeeRepository;
            _currentUserService = currentUserService;
            _context = context;
            _topLevelResolver = topLevelResolver;
        }

        // ─────────────────────────────────────────
        // APPROVE  (Phase 2: tách Approve khỏi Apply)
        // ─────────────────────────────────────────
        public async Task<HRProcedureResponseDto> ApproveProcedureAsync(int procedureId, ApproveHRProcedureDto approveDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId)
                    ?? throw new KeyNotFoundException("HR procedure not found.");

                if (procedure.Status != "Pending")
                    throw new InvalidOperationException("Cannot approve: only Pending procedures can be approved.");

                // ── Mới: Thắt chặt quyền phê duyệt ──────────────────
                var isAdmin = _currentUserService.RoleName == "ADMIN";
                bool isAuthorized = isAdmin;

                if (!isAuthorized)
                {
                    try
                    {
                        var currentEmpId = await _currentUserService.GetCurrentEmployeeIdAsync();
                        isAuthorized = await _topLevelResolver.IsTopLevelEmployeeAsync(currentEmpId);
                    }
                    catch { /* Không phải nhân viên hoặc không tìm thấy profile */ }
                }

                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException("Only ADMIN or TOP-LEVEL employees can approve HR procedures.");
                }
                // ──────────────────────────────────────────────────
                
                var finalApproverId = isAdmin ? (int?)null : await _currentUserService.GetCurrentEmployeeIdAsync();
                procedure.Status = "Approved";
                procedure.ApprovedDate = DateTime.UtcNow;
                procedure.ApprovedBy = finalApproverId;
                procedure.ReviewedDate = DateTime.UtcNow;
                procedure.ReviewedBy = finalApproverId;

                // Phase 2: chỉ apply ngay nếu EffectiveDate <= hôm nay
                if (ShouldApplyNow(procedure))
                {
                    await ApplyProcedureToEmployeeAsync(procedure, finalApproverId);
                    procedure.AppliedDate = DateTime.UtcNow;
                    procedure.AppliedBy = finalApproverId;
                }

                await _hrProcedureRepository.UpdateAsync(procedure);
                await transaction.CommitAsync();

                return await BuildResponseDtoAsync(procedure);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ─────────────────────────────────────────
        // APPLY riêng (Phase 2: dùng cho scheduled job hoặc manual trigger)
        // ─────────────────────────────────────────
        public async Task<HRProcedureResponseDto> ApplyApprovedProcedureAsync(int procedureId, int? appliedBy = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId)
                    ?? throw new KeyNotFoundException("HR procedure not found.");

                if (procedure.Status != "Approved")
                    throw new InvalidOperationException("Cannot apply: procedure must be in Approved status.");

                if (procedure.AppliedDate != null)
                    throw new InvalidOperationException("Procedure has already been applied.");

                // Use provided ID or resolve from current user (if any)
                int? finalAppliedBy = appliedBy;
                if (!finalAppliedBy.HasValue)
                {
                    try { finalAppliedBy = await _currentUserService.GetCurrentEmployeeIdAsync(); } catch { /* Ignore if no HTTP context */ }
                }

                await ApplyProcedureToEmployeeAsync(procedure, finalAppliedBy);
                procedure.AppliedDate = DateTime.UtcNow;
                procedure.AppliedBy = finalAppliedBy;

                await _hrProcedureRepository.UpdateAsync(procedure);
                await transaction.CommitAsync();

                return await BuildResponseDtoAsync(procedure);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ─────────────────────────────────────────
        // SUBMIT  (Phase 1: validation mở rộng)
        // ─────────────────────────────────────────
        public async Task<HRProcedureResponseDto> SubmitProcedureAsync(CreateHRProcedureDto createDto)
        {
            var validTypes = new[] { "Appointment", "Transfer", "Promotion", "Resignation", "Termination" };
            if (!validTypes.Contains(createDto.ProcedureType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid procedure type. Allowed: Appointment, Transfer, Promotion, Resignation, Termination.");

            if (!await _hrProcedureRepository.EmployeeExistsAsync(createDto.EmployeeId))
                throw new KeyNotFoundException("Employee not found.");

            if (createDto.EffectiveDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Effective date cannot be in the past.");

            if (await _hrProcedureRepository.HasActiveProcedureAsync(createDto.EmployeeId, createDto.ProcedureType))
                throw new InvalidOperationException("Employee already has an active procedure of this type.");

            // ── Phase 1: Transfer – cần ít nhất 1 thay đổi ──────────────────
            var type = createDto.ProcedureType;
            if (type.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
            {
                bool hasAnyChange = createDto.NewDepartmentId.HasValue
                    || createDto.NewPositionId.HasValue
                    || createDto.NewManagerId.HasValue
                    || createDto.NewSalary.HasValue;

                if (!hasAnyChange)
                    throw new ArgumentException("Transfer requires at least one of: NewDepartmentId, NewPositionId, NewManagerId, NewSalary.");

                // Validate no-op (so sánh với dữ liệu hiện tại)
                await ValidateNotNoOpAsync(createDto);
            }

            // ── Phase 1: Promotion – bắt buộc NewPositionId ──────────────────
            if (type.Equals("Promotion", StringComparison.OrdinalIgnoreCase) && !createDto.NewPositionId.HasValue)
                throw new ArgumentException("Promotion requires NewPositionId.");

            // ── Validate FK tồn tại ──────────────────────────────────────────
            if (createDto.NewDepartmentId.HasValue && !await _hrProcedureRepository.DepartmentExistsAsync(createDto.NewDepartmentId.Value))
                throw new KeyNotFoundException("NewDepartment not found.");

            if (createDto.NewPositionId.HasValue && !await _hrProcedureRepository.PositionExistsAsync(createDto.NewPositionId.Value))
                throw new KeyNotFoundException("NewPosition not found.");

            // ── Phase 1: Validate NewManager ────────────────────────────────
            if (createDto.NewManagerId.HasValue)
                await ValidateNewManagerAsync(createDto.EmployeeId, createDto.NewManagerId.Value);

            var procedure = new Hrprocedure
            {
                ProcedureNumber  = GenerateProcedureNumber(),
                EmployeeId       = createDto.EmployeeId,
                ProcedureType    = createDto.ProcedureType,
                EffectiveDate    = createDto.EffectiveDate,
                NewDepartmentId  = createDto.NewDepartmentId,
                NewPositionId    = createDto.NewPositionId,
                NewManagerId     = createDto.NewManagerId,
                NewSalary        = createDto.NewSalary,
                Reason           = createDto.Reason,
                Status           = "Pending",
                SubmittedDate    = DateTime.UtcNow,
                SubmittedBy      = await _currentUserService.GetCurrentEmployeeIdAsync()
            };

            await _hrProcedureRepository.AddAsync(procedure);

            return await BuildResponseDtoAsync(procedure);
        }

        // ─────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────
        public async Task<HRProcedureResponseDto> UpdateProcedureAsync(int procedureId, UpdateHRProcedureDto updateDto)
        {
            var procedure = await _hrProcedureRepository.GetByIdAsync(procedureId)
                ?? throw new KeyNotFoundException("HR procedure not found.");

            if (procedure.Status != "Pending")
                throw new InvalidOperationException("Cannot update: only Pending procedures can be updated.");

            var type = updateDto.ProcedureType;

            if (type.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
            {
                bool hasAnyChange = updateDto.NewDepartmentId.HasValue
                    || updateDto.NewPositionId.HasValue
                    || updateDto.NewManagerId.HasValue
                    || updateDto.NewSalary.HasValue;
                if (!hasAnyChange)
                    throw new ArgumentException("Transfer requires at least one of: NewDepartmentId, NewPositionId, NewManagerId, NewSalary.");
            }

            if (type.Equals("Promotion", StringComparison.OrdinalIgnoreCase) && !updateDto.NewPositionId.HasValue)
                throw new ArgumentException("Promotion requires NewPositionId.");

            if (updateDto.NewManagerId.HasValue)
                await ValidateNewManagerAsync(procedure.EmployeeId, updateDto.NewManagerId.Value);

            procedure.ProcedureType   = updateDto.ProcedureType;
            procedure.EffectiveDate   = updateDto.EffectiveDate;
            procedure.NewDepartmentId = updateDto.NewDepartmentId;
            procedure.NewPositionId   = updateDto.NewPositionId;
            procedure.NewManagerId    = updateDto.NewManagerId;
            procedure.NewSalary       = updateDto.NewSalary;
            procedure.Reason          = updateDto.Reason;

            await _hrProcedureRepository.UpdateAsync(procedure);

            return await BuildResponseDtoAsync(procedure);
        }

        // ─────────────────────────────────────────
        // REJECT
        // ─────────────────────────────────────────
        public async Task<HRProcedureResponseDto> RejectProcedureAsync(int procedureId, RejectHRProcedureDto rejectDto)
        {
            var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId)
                ?? throw new KeyNotFoundException("HR procedure not found.");

            if (procedure.Status != "Pending")
                throw new InvalidOperationException("Cannot reject: only Pending procedures can be rejected.");

            if (string.IsNullOrWhiteSpace(rejectDto.RejectionReason))
                throw new ArgumentException("Rejection reason is required.");

            // ── Mới: Thắt chặt quyền từ chối ─────────────────────
            var isAdmin = _currentUserService.RoleName == "ADMIN";
            bool isAuthorized = isAdmin;

            if (!isAuthorized)
            {
                try
                {
                    var currentEmpId = await _currentUserService.GetCurrentEmployeeIdAsync();
                    isAuthorized = await _topLevelResolver.IsTopLevelEmployeeAsync(currentEmpId);
                }
                catch { /* Không phải nhân viên hoặc không tìm thấy profile */ }
            }

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("Only ADMIN or TOP-LEVEL employees can reject HR procedures.");
            }
            // ──────────────────────────────────────────────────
            
            var finalReviewerId = isAdmin ? (int?)null : await _currentUserService.GetCurrentEmployeeIdAsync();
            procedure.Status          = "Rejected";
            procedure.RejectionReason = rejectDto.RejectionReason;
            procedure.ReviewedDate    = DateTime.UtcNow;
            procedure.ReviewedBy      = finalReviewerId;

            await _hrProcedureRepository.UpdateAsync(procedure);

            return await BuildResponseDtoAsync(procedure);
        }

        // ─────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────
        public async Task<bool> DeleteProcedureAsync(int procedureId)
        {
            var procedure = await _hrProcedureRepository.GetByIdAsync(procedureId);
            if (procedure == null || procedure.Status != "Pending")
                return false;

            return await _hrProcedureRepository.DeleteAsync(procedureId);
        }

        // ─────────────────────────────────────────
        // READ
        // ─────────────────────────────────────────
        public async Task<HRProcedureResponseDto?> GetProcedureByIdAsync(int procedureId)
        {
            var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId);
            if (procedure == null) return null;
            return await BuildResponseDtoAsync(procedure);
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetAllProceduresAsync()
        {
            var procedures = await _hrProcedureRepository.GetAllAsync();
            return await MapToListDtosAsync(procedures);
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetPendingProceduresAsync()
        {
            // ── Mới: Chỉ ADMIN và TOP-LEVEL mới thấy danh sách chờ duyệt ─────
            var isAdmin = _currentUserService.RoleName == "ADMIN";
            bool isAuthorized = isAdmin;

            if (!isAuthorized)
            {
                try
                {
                    var currentEmpId = await _currentUserService.GetCurrentEmployeeIdAsync();
                    isAuthorized = await _topLevelResolver.IsTopLevelEmployeeAsync(currentEmpId);
                }
                catch { /* Không phải nhân viên */ }
            }

            if (!isAuthorized)
            {
                return Enumerable.Empty<HRProcedureListDto>();
            }
            // ────────────────────────────────────────────────────────────────

            var procedures = await _hrProcedureRepository.GetPendingProceduresAsync();
            return await MapToListDtosAsync(procedures);
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetProceduresByEmployeeAsync(int employeeId)
        {
            if (!await _hrProcedureRepository.EmployeeExistsAsync(employeeId))
                throw new KeyNotFoundException("Employee not found.");

            var procedures = await _hrProcedureRepository.GetByEmployeeIdAsync(employeeId);
            return await MapToListDtosAsync(procedures);
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetProceduresByStatusAsync(string status)
        {
            var procedures = await _hrProcedureRepository.GetByStatusAsync(status);
            return await MapToListDtosAsync(procedures);
        }

        // ═════════════════════════════════════════
        // PRIVATE HELPERS
        // ═════════════════════════════════════════

        /// <summary>Phase 2: nên apply ngay nếu EffectiveDate &lt;= hôm nay</summary>
        private static bool ShouldApplyNow(Hrprocedure procedure)
            => procedure.EffectiveDate <= DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>Phase 2: ghi thay đổi vào employee profile</summary>
        private async Task ApplyProcedureToEmployeeAsync(Hrprocedure procedure, int? appliedBy = null)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(procedure.EmployeeId);
            if (employee == null) return;

            switch (procedure.ProcedureType.ToLower())
            {
                case "appointment":
                    if (procedure.NewDepartmentId.HasValue) employee.DepartmentId = procedure.NewDepartmentId.Value;
                    if (procedure.NewPositionId.HasValue)   employee.PositionId   = procedure.NewPositionId.Value;
                    if (procedure.NewManagerId.HasValue)    employee.ManagerId    = procedure.NewManagerId.Value;
                    break;

                case "transfer":
                    if (procedure.NewDepartmentId.HasValue) employee.DepartmentId = procedure.NewDepartmentId.Value;
                    if (procedure.NewPositionId.HasValue)   employee.PositionId   = procedure.NewPositionId.Value;
                    if (procedure.NewManagerId.HasValue)    employee.ManagerId    = procedure.NewManagerId.Value;
                    if (procedure.NewSalary.HasValue)       employee.BaseSalary   = procedure.NewSalary.Value;
                    break;

                case "promotion":
                    if (procedure.NewPositionId.HasValue) employee.PositionId = procedure.NewPositionId.Value;
                    if (procedure.NewSalary.HasValue)     employee.BaseSalary  = procedure.NewSalary.Value;
                    if (procedure.NewManagerId.HasValue)  employee.ManagerId   = procedure.NewManagerId.Value;
                    break;

                case "resignation":
                    employee.EmploymentStatus = "Resignation";
                    employee.ResignationDate  = procedure.EffectiveDate;
                    break;

                case "termination":
                    employee.EmploymentStatus = "Termination";
                    employee.ResignationDate  = procedure.EffectiveDate;
                    break;
            }

            employee.ModifiedDate = DateTime.UtcNow;
            employee.ModifiedBy   = appliedBy;
            await _employeeRepository.UpdateEmployeeAsync(employee);
        }

        /// <summary>Phase 1: phát hiện no-op – tất cả giá trị mới giống hiện tại</summary>
        private async Task ValidateNotNoOpAsync(CreateHRProcedureDto dto)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(dto.EmployeeId);
            if (employee == null) return;

            bool deptSame     = !dto.NewDepartmentId.HasValue || dto.NewDepartmentId == employee.DepartmentId;
            bool positionSame = !dto.NewPositionId.HasValue   || dto.NewPositionId   == employee.PositionId;
            bool managerSame  = !dto.NewManagerId.HasValue    || dto.NewManagerId    == employee.ManagerId;
            bool salarySame   = !dto.NewSalary.HasValue       || dto.NewSalary       == employee.BaseSalary;

            if (deptSame && positionSame && managerSame && salarySame)
                throw new ArgumentException("Procedure is a no-op: all new values are identical to the current employee profile.");
        }

        /// <summary>Phase 1: validate NewManager hợp lệ</summary>
        private async Task ValidateNewManagerAsync(int employeeId, int newManagerId)
        {
            if (newManagerId == employeeId)
                throw new ArgumentException("NewManager cannot be the employee themselves.");

            var manager = await _employeeRepository.GetEmployeeByIdAsync(newManagerId);
            if (manager == null)
                throw new KeyNotFoundException("NewManager not found.");

            if (manager.EmploymentStatus != "Active")
                throw new ArgumentException("NewManager must be an active employee.");
        }

        /// <summary>Build full response DTO, lookup names lazy</summary>
        private async Task<HRProcedureResponseDto> BuildResponseDtoAsync(Hrprocedure p)
        {
            var submittedBy  = await _employeeRepository.GetEmployeeByIdAsync(p.SubmittedBy);
            var reviewedBy   = p.ReviewedBy.HasValue  ? await _employeeRepository.GetEmployeeByIdAsync(p.ReviewedBy.Value)  : null;
            var approvedBy   = p.ApprovedBy.HasValue  ? await _employeeRepository.GetEmployeeByIdAsync(p.ApprovedBy.Value)  : null;
            var appliedBy    = p.AppliedBy.HasValue   ? await _employeeRepository.GetEmployeeByIdAsync(p.AppliedBy.Value)   : null;
            var newManager   = p.NewManagerId.HasValue ? await _employeeRepository.GetEmployeeByIdAsync(p.NewManagerId.Value) : null;

            return new HRProcedureResponseDto
            {
                ProcedureId      = p.ProcedureId,
                ProcedureNumber  = p.ProcedureNumber,
                EmployeeId       = p.EmployeeId,
                EmployeeFullName = p.Employee?.FullName ?? "Unknown",
                EmployeeCode     = p.Employee?.EmployeeCode ?? "",
                ProcedureType    = p.ProcedureType,
                EffectiveDate    = p.EffectiveDate,
                NewDepartmentId  = p.NewDepartmentId,
                NewDepartmentName = p.NewDepartment?.DepartmentName,
                NewPositionId    = p.NewPositionId,
                NewPositionName  = p.NewPosition?.PositionName,
                NewManagerId     = p.NewManagerId,
                NewManagerName   = newManager?.FullName,
                NewSalary        = p.NewSalary,
                Reason           = p.Reason,
                Status           = p.Status,
                RejectionReason  = p.RejectionReason,
                SubmittedDate    = p.SubmittedDate,
                SubmittedBy      = p.SubmittedBy,
                SubmittedByName  = submittedBy?.FullName  ?? "System",
                ReviewedDate     = p.ReviewedDate,
                ReviewedBy       = p.ReviewedBy,
                ReviewedByName   = reviewedBy?.FullName   ?? null,
                ApprovedDate     = p.ApprovedDate,
                ApprovedBy       = p.ApprovedBy,
                ApprovedByName   = approvedBy?.FullName   ?? null,
                AppliedDate      = p.AppliedDate,
                AppliedBy        = p.AppliedBy,
                AppliedByName    = appliedBy?.FullName    ?? null,
            };
        }

        private async Task<IEnumerable<HRProcedureListDto>> MapToListDtosAsync(IEnumerable<Hrprocedure> procedures)
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return procedures.Select(p =>
            {
                var submittedByEmployee = employees.FirstOrDefault(e => e.EmployeeId == p.SubmittedBy);
                return new HRProcedureListDto
                {
                    ProcedureId      = p.ProcedureId,
                    ProcedureNumber  = p.ProcedureNumber,
                    EmployeeFullName = p.Employee?.FullName ?? "Unknown",
                    EmployeeCode     = p.Employee?.EmployeeCode ?? "",
                    ProcedureType    = p.ProcedureType,
                    EffectiveDate    = p.EffectiveDate,
                    Status           = p.Status,
                    SubmittedDate    = p.SubmittedDate,
                    SubmittedByName  = submittedByEmployee?.FullName ?? "System"
                };
            }).ToList();
        }

        private static string GenerateProcedureNumber()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"PR-{datePrefix}-{random}";
        }
    }
}
