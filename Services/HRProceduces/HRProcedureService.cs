using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services.CurrentUsers;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services.HRProceduces
{
    public class HRProcedureService : IHRProcedureService
    {
        private readonly IHRProcedureRepository _hrProcedureRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICurrentUserService _currentUserService;
        public HRProcedureService(IHRProcedureRepository hrProcedureRepository, IEmployeeRepository employeeRepository, IHttpContextAccessor contextAccessor, ICurrentUserService currentUserService)
        {
            _hrProcedureRepository = hrProcedureRepository;
            _employeeRepository = employeeRepository;
            _contextAccessor = contextAccessor;
            _currentUserService = currentUserService;
        }

        public async Task<HRProcedureResponseDto> ApproveProcedureAsync(int procedureId, ApproveHRProcedureDto approveDto)
        {
            var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId);

            if (procedure == null)
            {
                throw new KeyNotFoundException("HR procedure not found.");
            }

            if (procedure.Status != "Pending")
            {
                throw new InvalidOperationException("Cannot approve procedure. Only pending procedures can be approved.");
            }

            procedure.Status = "Approved";
            procedure.ApprovedDate = DateTime.UtcNow;
            procedure.ApprovedBy = await _currentUserService.GetCurrentEmployeeIdAsync();
            procedure.ReviewedDate = DateTime.UtcNow;
            procedure.ReviewedBy = await _currentUserService.GetCurrentEmployeeIdAsync();

            await _hrProcedureRepository.UpdateAsync(procedure);

            await UpdateEmployeeProfileAsync(procedure);
            var submittedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.SubmittedBy);
            var reviewedByEmployee =  await _employeeRepository.GetEmployeeByIdAsync(procedure.ReviewedBy.Value);
            var approvedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.ApprovedBy.Value);

            return new HRProcedureResponseDto
            {
                ProcedureId = procedure.ProcedureId,
                ProcedureNumber = procedure.ProcedureNumber,
                EmployeeId = procedure.EmployeeId,
                EmployeeFullName = procedure.Employee?.FullName ?? "Unknown",
                EmployeeCode = procedure.Employee?.EmployeeCode ?? "",
                ProcedureType = procedure.ProcedureType,
                EffectiveDate = procedure.EffectiveDate,
                NewDepartmentId = procedure.NewDepartmentId,
                NewDepartmentName = procedure.NewDepartment?.DepartmentName,
                NewPositionId = procedure.NewPositionId,
                NewPositionName = procedure.NewPosition?.PositionName,
                NewSalary = procedure.NewSalary,
                Reason = procedure.Reason,
                Status = procedure.Status,
                RejectionReason = procedure.RejectionReason,
                SubmittedDate = procedure.SubmittedDate,
                SubmittedBy = procedure.SubmittedBy,
                SubmittedByName = submittedByEmployee?.FullName ?? "System",
                ReviewedDate = procedure.ReviewedDate,
                ReviewedBy = procedure.ReviewedBy,
                ReviewedByName = reviewedByEmployee?.FullName ?? "System",
                ApprovedDate = procedure.ApprovedDate,
                ApprovedBy = procedure.ApprovedBy,
                ApprovedByName = approvedByEmployee?.FullName ?? "System"
            };
        }

        public async Task<bool> DeleteProcedureAsync(int procedureId)
        {
            var procedure = await _hrProcedureRepository.GetByIdAsync(procedureId);
            if (procedure == null)
                return false;

            if (procedure.Status != "Pending")
                return false;

            var deleted = await _hrProcedureRepository.DeleteAsync(procedureId);

            return deleted;
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetAllProceduresAsync()
        {
            var procedures = await _hrProcedureRepository.GetAllAsync();
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            return procedures.Select(p =>
            {
                var submittedByEmployee = employees
                    .FirstOrDefault(e => e.EmployeeId == p.SubmittedBy);

                return new HRProcedureListDto
                {
                    ProcedureId = p.ProcedureId,
                    ProcedureNumber = p.ProcedureNumber,
                    EmployeeFullName = p.Employee?.FullName ?? "Unknown",
                    EmployeeCode = p.Employee?.EmployeeCode ?? "",
                    ProcedureType = p.ProcedureType,
                    EffectiveDate = p.EffectiveDate,
                    Status = p.Status,
                    SubmittedDate = p.SubmittedDate,
                    SubmittedByName = submittedByEmployee?.FullName ?? "System"
                };
            }).ToList();
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetPendingProceduresAsync()
        {
            var procedures = await _hrProcedureRepository.GetPendingProceduresAsync();
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            return procedures.Select(p =>
            {
                var submittedByEmployee = employees
                    .FirstOrDefault(e => e.EmployeeId == p.SubmittedBy);

                return new HRProcedureListDto
                {
                    ProcedureId = p.ProcedureId,
                    ProcedureNumber = p.ProcedureNumber,
                    EmployeeFullName = p.Employee?.FullName ?? "Unknown",
                    EmployeeCode = p.Employee?.EmployeeCode ?? "",
                    ProcedureType = p.ProcedureType,
                    EffectiveDate = p.EffectiveDate,
                    Status = p.Status,
                    SubmittedDate = p.SubmittedDate,
                    SubmittedByName = submittedByEmployee?.FullName ?? "System"
                };
            }).ToList();
        }

        public async Task<HRProcedureResponseDto?> GetProcedureByIdAsync(int procedureId)
        {
            var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId);
            if (procedure == null)
                return null;

            var submittedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.SubmittedBy);

            var reviewedByEmployee = procedure.ReviewedBy.HasValue
                ? await _employeeRepository.GetEmployeeByIdAsync(procedure.ReviewedBy.Value): null;

            var approvedByEmployee = procedure.ApprovedBy.HasValue
                ? await _employeeRepository.GetEmployeeByIdAsync(procedure.ApprovedBy.Value) : null;

            return new HRProcedureResponseDto
            {
                ProcedureId = procedure.ProcedureId,
                ProcedureNumber = procedure.ProcedureNumber,
                EmployeeId = procedure.EmployeeId,
                EmployeeFullName = procedure.Employee?.FullName ?? "Unknown",
                EmployeeCode = procedure.Employee?.EmployeeCode ?? "",
                ProcedureType = procedure.ProcedureType,
                EffectiveDate = procedure.EffectiveDate,
                NewDepartmentId = procedure.NewDepartmentId,
                NewDepartmentName = procedure.NewDepartment?.DepartmentName,
                NewPositionId = procedure.NewPositionId,
                NewPositionName = procedure.NewPosition?.PositionName,
                NewSalary = procedure.NewSalary,
                Reason = procedure.Reason,
                Status = procedure.Status,
                RejectionReason = procedure.RejectionReason,

                SubmittedDate = procedure.SubmittedDate,
                SubmittedBy = procedure.SubmittedBy,
                SubmittedByName = submittedByEmployee?.FullName ?? "System",

                ReviewedDate = procedure.ReviewedDate,
                ReviewedBy = procedure.ReviewedBy,
                ReviewedByName = reviewedByEmployee?.FullName ?? "System",

                ApprovedDate = procedure.ApprovedDate,
                ApprovedBy = procedure.ApprovedBy,
                ApprovedByName = approvedByEmployee?.FullName ?? "System",
            };
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetProceduresByEmployeeAsync(int employeeId)
        {
            if (!await _hrProcedureRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found in the system.");
            }
            
            var procedure = await _hrProcedureRepository.GetByEmployeeIdAsync(employeeId);
            var employee = await _employeeRepository.GetAllEmployeesAsync();

            return procedure.Select(p =>
            {
                var submittedByEmployee = employee
                    .FirstOrDefault(e => e.EmployeeId == p.SubmittedBy);

                return new HRProcedureListDto
                {
                    ProcedureId = p.ProcedureId,
                    ProcedureNumber = p.ProcedureNumber,
                    EmployeeFullName = p.Employee?.FullName ?? "Unknown",
                    EmployeeCode = p.Employee?.EmployeeCode ?? "",
                    ProcedureType = p.ProcedureType,
                    EffectiveDate = p.EffectiveDate,
                    Status = p.Status,
                    SubmittedDate = p.SubmittedDate,
                    SubmittedByName = submittedByEmployee?.FullName ?? "System"
                };
            }).ToList();
        }

        public async Task<IEnumerable<HRProcedureListDto>> GetProceduresByStatusAsync(string status)
        {
            var procedure = await _hrProcedureRepository.GetByStatusAsync(status);
            var employee = await _employeeRepository.GetAllEmployeesAsync();
            return procedure.Select(p =>
            {
                var submittedByEmployee = employee
                    .FirstOrDefault(e => e.EmployeeId == p.SubmittedBy);

                return new HRProcedureListDto
                {
                    ProcedureId = p.ProcedureId,
                    ProcedureNumber = p.ProcedureNumber,
                    EmployeeFullName = p.Employee?.FullName ?? "Unknown",
                    EmployeeCode = p.Employee?.EmployeeCode ?? "",
                    ProcedureType = p.ProcedureType,
                    EffectiveDate = p.EffectiveDate,
                    Status = p.Status,
                    SubmittedDate = p.SubmittedDate,
                    SubmittedByName = submittedByEmployee?.FullName ?? "System"
                };
            }).ToList();
        }

        public async Task<HRProcedureResponseDto> RejectProcedureAsync(int procedureId, RejectHRProcedureDto rejectDto)
        {
            var procedure = await _hrProcedureRepository.GetByIdWithDetailsAsync(procedureId);

            if (procedure == null)
            {
                throw new KeyNotFoundException("HR procedure not found.");
            }

            if (procedure.Status != "Pending")
            {
                throw new InvalidOperationException("Cannot reject procedure. Only pending procedures can be rejected.");
            }

            if (string.IsNullOrWhiteSpace(rejectDto.RejectionReason))
            {
                throw new ArgumentException("Rejection reason is required when rejecting a procedure.");
            }

            procedure.Status = "Rejected";
            procedure.RejectionReason = rejectDto.RejectionReason;
            procedure.ReviewedDate = DateTime.UtcNow;
            procedure.ReviewedBy = await _currentUserService.GetCurrentEmployeeIdAsync();

            await _hrProcedureRepository.UpdateAsync(procedure);
            var submittedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.SubmittedBy);
            var reviewedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.ReviewedBy.Value);
            return new HRProcedureResponseDto
            {
                ProcedureId = procedure.ProcedureId,
                ProcedureNumber = procedure.ProcedureNumber,
                EmployeeId = procedure.EmployeeId,
                EmployeeFullName = procedure.Employee?.FullName ?? "Unknown",
                EmployeeCode = procedure.Employee?.EmployeeCode ?? "",
                ProcedureType = procedure.ProcedureType,
                EffectiveDate = procedure.EffectiveDate,
                NewDepartmentId = procedure.NewDepartmentId,
                NewDepartmentName = procedure.NewDepartment?.DepartmentName,
                NewPositionId = procedure.NewPositionId,
                NewPositionName = procedure.NewPosition?.PositionName,
                NewSalary = procedure.NewSalary,
                Reason = procedure.Reason,
                Status = procedure.Status,
                RejectionReason = procedure.RejectionReason,
                SubmittedDate = procedure.SubmittedDate,
                SubmittedBy = procedure.SubmittedBy,
                SubmittedByName = submittedByEmployee?.FullName ?? "System",
                ReviewedDate = procedure.ReviewedDate,
                ReviewedBy = procedure.ReviewedBy,
                ReviewedByName = reviewedByEmployee?.FullName ?? "System",
                ApprovedDate = null,
                ApprovedBy =  null,
                ApprovedByName =  null
            };
        }

        public async Task<HRProcedureResponseDto> SubmitProcedureAsync(CreateHRProcedureDto createDto)
        {

            var validTypes = new[] { "Appointment", "Transfer", "Promotion", "Resignation", "Termination" };
            if (!validTypes.Contains(createDto.ProcedureType, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid procedure type. Allowed types: Appointment, Transfer, Promotion, Resignation, Termination.");
            }

            if (!await _hrProcedureRepository.EmployeeExistsAsync(createDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found in the system.");
            }

            if (createDto.EffectiveDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Effective date cannot be in the past.");
            }

            if (await _hrProcedureRepository.HasActiveProcedureAsync(createDto.EmployeeId, createDto.ProcedureType))
            {
                throw new InvalidOperationException("Employee already has an active procedure of this type. Cannot submit duplicate request.");
            }

            if (createDto.ProcedureType.Equals("Transfer", StringComparison.OrdinalIgnoreCase) &&
                !createDto.NewDepartmentId.HasValue)
            {
                throw new ArgumentException("Transfer procedure requires a New Department to be specified.");
            }

            if (createDto.ProcedureType.Equals("Promotion", StringComparison.OrdinalIgnoreCase) &&
                !createDto.NewPositionId.HasValue)
            {
                throw new ArgumentException("Promotion procedure requires a New Position to be specified.");
            }

            if (createDto.NewDepartmentId.HasValue &&
                !await _hrProcedureRepository.DepartmentExistsAsync(createDto.NewDepartmentId.Value))
            {
                throw new KeyNotFoundException("Department not found.");
            }

            if (createDto.NewPositionId.HasValue &&
                !await _hrProcedureRepository.PositionExistsAsync(createDto.NewPositionId.Value))
            {
                throw new KeyNotFoundException("Position not found.");
            }
            var procedure = new Hrprocedure
            {
                ProcedureNumber = GenerateProcedureNumber(),
                EmployeeId = createDto.EmployeeId,
                ProcedureType = createDto.ProcedureType,
                EffectiveDate = createDto.EffectiveDate,
                NewDepartmentId = createDto.NewDepartmentId,
                NewPositionId = createDto.NewPositionId,
                NewSalary = createDto.NewSalary,
                Reason = createDto.Reason,
                Status = "Pending",
                SubmittedDate = DateTime.UtcNow,
                SubmittedBy = await _currentUserService.GetCurrentEmployeeIdAsync()
            };

            await _hrProcedureRepository.AddAsync(procedure);

            var submittedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.SubmittedBy);
            return new HRProcedureResponseDto
            {
                ProcedureId = procedure.ProcedureId,
                ProcedureNumber = procedure.ProcedureNumber,
                EmployeeId = procedure.EmployeeId,
                EmployeeFullName = procedure.Employee?.FullName ?? "Unknown",
                EmployeeCode = procedure.Employee?.EmployeeCode ?? "",
                ProcedureType = procedure.ProcedureType,
                EffectiveDate = procedure.EffectiveDate,
                NewDepartmentId = procedure.NewDepartmentId,
                NewDepartmentName = procedure.NewDepartment?.DepartmentName,
                NewPositionId = procedure.NewPositionId,
                NewPositionName = procedure.NewPosition?.PositionName,
                NewSalary = procedure.NewSalary,
                Reason = procedure.Reason,
                Status = procedure.Status,
                RejectionReason = procedure.RejectionReason,
                SubmittedDate = procedure.SubmittedDate,
                SubmittedBy = procedure.SubmittedBy,
                SubmittedByName = submittedByEmployee?.FullName ?? "System",
                ReviewedDate = null,
                ReviewedBy = null,
                ReviewedByName = null,
                ApprovedDate = null,
                ApprovedBy = null,
                ApprovedByName = null
            };

        }

        public async Task<HRProcedureResponseDto> UpdateProcedureAsync(int procedureId, UpdateHRProcedureDto updateDto)
        {
            var procedure = await _hrProcedureRepository.GetByIdAsync(procedureId);
            if (procedure == null)
            {
                throw new KeyNotFoundException("HR procedure not found.");
            }

            if (procedure.Status != "Pending")
            {
                throw new InvalidOperationException("Cannot update procedure. Only pending procedures can be updated.");
            }

            procedure.ProcedureType = updateDto.ProcedureType;
            procedure.EffectiveDate = updateDto.EffectiveDate;
            procedure.NewDepartmentId = updateDto.NewDepartmentId;
            procedure.NewPositionId = updateDto.NewPositionId;
            procedure.NewSalary = updateDto.NewSalary;
            procedure.Reason = updateDto.Reason;

            await _hrProcedureRepository.UpdateAsync(procedure);
            var submittedByEmployee = await _employeeRepository.GetEmployeeByIdAsync(procedure.SubmittedBy);
            return new HRProcedureResponseDto
            {
                ProcedureId = procedure.ProcedureId,
                ProcedureNumber = procedure.ProcedureNumber,
                EmployeeId = procedure.EmployeeId,
                EmployeeFullName = procedure.Employee?.FullName ?? "Unknown",
                EmployeeCode = procedure.Employee?.EmployeeCode ?? "",
                ProcedureType = procedure.ProcedureType,
                EffectiveDate = procedure.EffectiveDate,
                NewDepartmentId = procedure.NewDepartmentId,
                NewDepartmentName = procedure.NewDepartment?.DepartmentName,
                NewPositionId = procedure.NewPositionId,
                NewPositionName = procedure.NewPosition?.PositionName,
                NewSalary = procedure.NewSalary,
                Reason = procedure.Reason,
                Status = procedure.Status,
                RejectionReason = procedure.RejectionReason,
                SubmittedDate = procedure.SubmittedDate,
                SubmittedBy = procedure.SubmittedBy,
                SubmittedByName = submittedByEmployee?.FullName ?? "System",
                ReviewedDate = null,
                ReviewedBy = null,
                ReviewedByName = null,
                ApprovedDate = null,
                ApprovedBy = null,
                ApprovedByName = null
            };
        }

        private async Task UpdateEmployeeProfileAsync(Hrprocedure procedure)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(procedure.EmployeeId);
            if (employee == null)
                return;

            switch (procedure.ProcedureType.ToLower())
            {
                case "appointment":
                    if (procedure.NewPositionId.HasValue)
                        employee.PositionId = procedure.NewPositionId.Value;
                    if (procedure.NewDepartmentId.HasValue)
                        employee.DepartmentId = procedure.NewDepartmentId.Value;
                    break;

                case "transfer":
                    if (procedure.NewDepartmentId.HasValue)
                        employee.DepartmentId = procedure.NewDepartmentId.Value;
                    break;

                case "promotion":
                    if (procedure.NewPositionId.HasValue)
                        employee.PositionId = procedure.NewPositionId.Value;
                    if (procedure.NewSalary.HasValue)
                        employee.BaseSalary = procedure.NewSalary.Value;
                    break;

                case "resignation":
                    employee.EmploymentStatus = "Resignation";
                    employee.ResignationDate = procedure.EffectiveDate;
                    break;

                case "termination":
                    employee.EmploymentStatus = "Termination";
                    employee.ResignationDate = procedure.EffectiveDate;
                    break;
            }

            employee.ModifiedDate = DateTime.UtcNow;
            employee.ModifiedBy = await _currentUserService.GetCurrentEmployeeIdAsync();

            await _employeeRepository.UpdateEmployeeAsync(employee);
        }

        private string GenerateProcedureNumber()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"PR-{datePrefix}-{random}";
        }
    }
}
