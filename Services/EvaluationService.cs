using HRManagement.DataAcess;
using HRManagement.DTOs;
using HRManagement.Models;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IEvaluationCycleRepository _cycleRepository;
        private readonly IEvaluationTemplateRepository _templateRepository;
        private readonly IEmployeeRepository _employeeRepository;

        private const string STATUS_NOT_STARTED = "Not Started";
        private const string STATUS_SELF_EVALUATION = "Self Evaluation";
        private const string STATUS_MANAGER_EVALUATION = "Manager Evaluation";
        private const string STATUS_UNDER_REVIEW = "Under Review";
        private const string STATUS_COMPLETED = "Completed";
        private const string STATUS_ACKNOWLEDGED = "Acknowledged";

        public EvaluationService(
            IEvaluationRepository evaluationRepository,
            IEvaluationCycleRepository cycleRepository,
            IEvaluationTemplateRepository templateRepository,
            IEmployeeRepository employeeRepository)
        {
            _evaluationRepository = evaluationRepository;
            _cycleRepository = cycleRepository;
            _templateRepository = templateRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<AssignmentResultDto> AssignEvaluatorsAsync(AssignEvaluatorsDto dto)
        {
            var cycle = await _cycleRepository.GetByIdAsync(dto.CycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            if (cycle.Status != "Active")
            {
                throw new InvalidOperationException("Cannot assign evaluators to a non-active cycle.");
            }

            var result = new AssignmentResultDto();

            foreach (var assignment in dto.Assignments)
            {
                try
                {
                    await ValidateAssignment(dto.CycleId, assignment);

                    if (await _evaluationRepository.EmployeeHasEvaluationInCycleAsync(dto.CycleId, assignment.EmployeeId))
                    {
                        result.Errors.Add(new AssignmentErrorDto
                        {
                            EmployeeId = assignment.EmployeeId,
                            EmployeeName = await GetEmployeeNameAsync(assignment.EmployeeId),
                            ErrorMessage = "Employee already has an evaluation in this cycle."
                        });
                        result.FailedCount++;
                        continue;
                    }

                    var evaluation = new Evaluation
                    {
                        CycleId = dto.CycleId,
                        EmployeeId = assignment.EmployeeId,
                        TemplateId = assignment.TemplateId,
                        PrimaryEvaluatorId = assignment.PrimaryEvaluatorId,
                        SecondaryEvaluatorId = assignment.SecondaryEvaluatorId,
                        Status = STATUS_NOT_STARTED
                    };

                    await _evaluationRepository.AddAsync(evaluation);

                    var createdEvaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluation.EvaluationId);
                    if (createdEvaluation != null)
                    {
                        result.SuccessfulAssignments.Add(await MapToResponseDto(createdEvaluation));
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new AssignmentErrorDto
                    {
                        EmployeeId = assignment.EmployeeId,
                        EmployeeName = await GetEmployeeNameAsync(assignment.EmployeeId),
                        ErrorMessage = ex.Message
                    });
                    result.FailedCount++;
                }
            }

            return result;
        }

        public async Task<AssignmentResultDto> AutoAssignEvaluatorsAsync(AutoAssignEvaluatorsDto dto)
        {
            var cycle = await _cycleRepository.GetByIdAsync(dto.CycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            if (cycle.Status != "Active")
            {
                throw new InvalidOperationException("Cannot assign evaluators to a non-active cycle.");
            }

            var template = await _templateRepository.GetByIdAsync(dto.TemplateId);
            if (template == null || !template.IsActive)
            {
                throw new KeyNotFoundException("Evaluation template not found or inactive.");
            }

            var employees = await GetEmployeesInScopeAsync(cycle.ApplicableDepartments, dto.DepartmentId);

            var result = new AssignmentResultDto();

            foreach (var employee in employees)
            {
                try
                {
                    if (await _evaluationRepository.EmployeeHasEvaluationInCycleAsync(dto.CycleId, employee.EmployeeId))
                    {
                        continue; 
                    }

                    if (!employee.ManagerId.HasValue)
                    {
                        result.Errors.Add(new AssignmentErrorDto
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.FullName,
                            ErrorMessage = "Employee has no direct manager assigned."
                        });
                        result.FailedCount++;
                        continue;
                    }

                    int? secondaryEvaluatorId = null;
                    if (dto.IncludeSecondaryEvaluator)
                    {
                        var manager = await _employeeRepository.GetEmployeeByIdAsync(employee.ManagerId.Value);
                        if (manager?.ManagerId.HasValue == true)
                        {
                            secondaryEvaluatorId = manager.ManagerId.Value;
                        }
                    }

                    var evaluation = new Evaluation
                    {
                        CycleId = dto.CycleId,
                        EmployeeId = employee.EmployeeId,
                        TemplateId = dto.TemplateId,
                        PrimaryEvaluatorId = employee.ManagerId.Value,
                        SecondaryEvaluatorId = secondaryEvaluatorId,
                        Status = STATUS_NOT_STARTED
                    };

                    await _evaluationRepository.AddAsync(evaluation);

                    var createdEvaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluation.EvaluationId);
                    if (createdEvaluation != null)
                    {
                        result.SuccessfulAssignments.Add(await MapToResponseDto(createdEvaluation));
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new AssignmentErrorDto
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = employee.FullName,
                        ErrorMessage = ex.Message
                    });
                    result.FailedCount++;
                }
            }

            return result;
        }

        public async Task<AssignmentResultDto> BulkAssignByDepartmentAsync(BulkAssignByDepartmentDto dto)
        {
            var cycle = await _cycleRepository.GetByIdAsync(dto.CycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            if (cycle.Status != "Active")
            {
                throw new InvalidOperationException("Cannot assign evaluators to a non-active cycle.");
            }

            var template = await _templateRepository.GetByIdAsync(dto.TemplateId);
            if (template == null || !template.IsActive)
            {
                throw new KeyNotFoundException("Evaluation template not found or inactive.");
            }

            if (!await _employeeRepository.EmployeeExistsAsync(dto.PrimaryEvaluatorId))
            {
                throw new KeyNotFoundException("Primary evaluator not found.");
            }

            if (dto.SecondaryEvaluatorId.HasValue && !await _employeeRepository.EmployeeExistsAsync(dto.SecondaryEvaluatorId.Value))
            {
                throw new KeyNotFoundException("Secondary evaluator not found.");
            }

            var employees = await _employeeRepository.GetAllEmployeesAsync();
            var departmentEmployees = employees.Where(e =>
                e.DepartmentId == dto.DepartmentId &&
                e.EmploymentStatus == "Active").ToList();

            var result = new AssignmentResultDto();

            foreach (var employee in departmentEmployees)
            {
                try
                {
                    if (await _evaluationRepository.EmployeeHasEvaluationInCycleAsync(dto.CycleId, employee.EmployeeId))
                    {
                        continue; 
                    }

                    var evaluation = new Evaluation
                    {
                        CycleId = dto.CycleId,
                        EmployeeId = employee.EmployeeId,
                        TemplateId = dto.TemplateId,
                        PrimaryEvaluatorId = dto.PrimaryEvaluatorId,
                        SecondaryEvaluatorId = dto.SecondaryEvaluatorId,
                        Status = STATUS_NOT_STARTED
                    };

                    await _evaluationRepository.AddAsync(evaluation);

                    var createdEvaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluation.EvaluationId);
                    if (createdEvaluation != null)
                    {
                        result.SuccessfulAssignments.Add(await MapToResponseDto(createdEvaluation));
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new AssignmentErrorDto
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = employee.FullName,
                        ErrorMessage = ex.Message
                    });
                    result.FailedCount++;
                }
            }

            return result;
        }

        public async Task<List<AssignmentPreviewDto>> GetAssignmentPreviewAsync(int cycleId)
        {
            // Step 8: System displays assignment preview
            var cycle = await _cycleRepository.GetByIdAsync(cycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            var employees = await GetEmployeesInScopeAsync(cycle.ApplicableDepartments, null);
            var previews = new List<AssignmentPreviewDto>();

            foreach (var employee in employees)
            {
                var preview = new AssignmentPreviewDto
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.FullName,
                    Department = employee.Department?.DepartmentName ?? "N/A",
                    HasDirectManager = employee.ManagerId.HasValue
                };

                if (employee.ManagerId.HasValue)
                {
                    var manager = await _employeeRepository.GetEmployeeByIdAsync(employee.ManagerId.Value);
                    preview.SuggestedPrimaryEvaluatorId = employee.ManagerId.Value;
                    preview.SuggestedPrimaryEvaluatorName = manager?.FullName;

                    // Get skip-level manager
                    if (manager?.ManagerId.HasValue == true)
                    {
                        var skipLevel = await _employeeRepository.GetEmployeeByIdAsync(manager.ManagerId.Value);
                        preview.SuggestedSecondaryEvaluatorId = manager.ManagerId.Value;
                        preview.SuggestedSecondaryEvaluatorName = skipLevel?.FullName;
                    }
                }
                else
                {
                    preview.Issue = "No direct manager assigned";
                }

                previews.Add(preview);
            }

            return previews;
        }
        public async Task<IEnumerable<EvaluationListDto>> GetEvaluationsByCycleAsync(int cycleId)
        {
            var evaluations = await _evaluationRepository.GetByCycleIdAsync(cycleId);
            return evaluations.Select(MapToListDto).ToList();
        }

        public async Task<IEnumerable<EvaluationListDto>> GetEvaluationsByEmployeeAsync(int employeeId)
        {
            var evaluations = await _evaluationRepository.GetByEmployeeIdAsync(employeeId);
            return evaluations.Select(MapToListDto).ToList();
        }

        public async Task<IEnumerable<EvaluationListDto>> GetEvaluationsByEvaluatorAsync(int evaluatorId)
        {
            var evaluations = await _evaluationRepository.GetByEvaluatorIdAsync(evaluatorId);
            return evaluations.Select(MapToListDto).ToList();
        }

        public async Task<EvaluationResponseDto?> GetEvaluationByIdAsync(int evaluationId)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluationId);
            if (evaluation == null)
                return null;

            return await MapToResponseDto(evaluation);
        }
        public async Task<EvaluationResponseDto> ChangeEvaluatorAsync(int evaluationId, ChangeEvaluatorDto dto)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(evaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }

            if (evaluation.Status == STATUS_COMPLETED || evaluation.Status == STATUS_ACKNOWLEDGED)
            {
                throw new InvalidOperationException("Cannot change evaluator for completed evaluations.");
            }

            if (dto.PrimaryEvaluatorId.HasValue)
            {
                if (!await _employeeRepository.EmployeeExistsAsync(dto.PrimaryEvaluatorId.Value))
                {
                    throw new KeyNotFoundException("Primary evaluator not found.");
                }
                evaluation.PrimaryEvaluatorId = dto.PrimaryEvaluatorId.Value;
            }

            if (dto.SecondaryEvaluatorId.HasValue)
            {
                if (!await _employeeRepository.EmployeeExistsAsync(dto.SecondaryEvaluatorId.Value))
                {
                    throw new KeyNotFoundException("Secondary evaluator not found.");
                }
                evaluation.SecondaryEvaluatorId = dto.SecondaryEvaluatorId;
            }

            await _evaluationRepository.UpdateAsync(evaluation);

            var updated = await _evaluationRepository.GetByIdWithDetailsAsync(evaluationId);
            return await MapToResponseDto(updated!);
        }

        private async Task ValidateAssignment(int cycleId, EvaluatorAssignmentDto assignment)
        {
            if (!await _employeeRepository.EmployeeExistsAsync(assignment.EmployeeId))
            {
                throw new KeyNotFoundException($"Employee with ID {assignment.EmployeeId} not found.");
            }
            var template = await _templateRepository.GetByIdAsync(assignment.TemplateId);
            if (template == null || !template.IsActive)
            {
                throw new KeyNotFoundException("Evaluation template not found or inactive.");
            }

            if (!await _employeeRepository.EmployeeExistsAsync(assignment.PrimaryEvaluatorId))
            {
                throw new KeyNotFoundException("Primary evaluator not found.");
            }

            if (assignment.SecondaryEvaluatorId.HasValue)
            {
                if (!await _employeeRepository.EmployeeExistsAsync(assignment.SecondaryEvaluatorId.Value))
                {
                    throw new KeyNotFoundException("Secondary evaluator not found.");
                }
            }

            if (assignment.EmployeeId == assignment.PrimaryEvaluatorId ||
                assignment.EmployeeId == assignment.SecondaryEvaluatorId)
            {
                throw new InvalidOperationException("Employee cannot evaluate themselves (circular evaluation detected).");
            }
        }

        private async Task<List<Employee>> GetEmployeesInScopeAsync(string? applicableDepartments, int? specificDepartmentId)
        {
            var allEmployees = await _employeeRepository.GetAllEmployeesAsync();
            var activeEmployees = allEmployees.Where(e => e.EmploymentStatus == "Active").ToList();

            if (specificDepartmentId.HasValue)
            {
                return activeEmployees.Where(e => e.DepartmentId == specificDepartmentId.Value).ToList();
            }

            if (string.IsNullOrEmpty(applicableDepartments) || applicableDepartments == "All")
            {
                return activeEmployees;
            }

            var departmentIds = applicableDepartments
                .Split(',')
                .Select(d => d.Trim())
                .Where(d => int.TryParse(d, out _))
                .Select(int.Parse)
                .ToList();

            return activeEmployees.Where(e => departmentIds.Contains(e.DepartmentId)).ToList();
        }

        private async Task<string> GetEmployeeNameAsync(int employeeId)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
            return employee?.FullName ?? "Unknown";
        }

        private async Task<EvaluationResponseDto> MapToResponseDto(Evaluation evaluation)
        {
            return new EvaluationResponseDto
            {
                EvaluationId = evaluation.EvaluationId,
                CycleId = evaluation.CycleId,
                CycleName = evaluation.Cycle?.CycleName ?? "N/A",
                EmployeeId = evaluation.EmployeeId,
                EmployeeCode = evaluation.Employee?.EmployeeCode ?? "N/A",
                EmployeeName = evaluation.Employee?.FullName ?? "N/A",
                EmployeeDepartment = evaluation.Employee?.Department?.DepartmentName ?? "N/A",
                EmployeePosition = evaluation.Employee?.Position?.PositionName ?? "N/A",
                TemplateId = evaluation.TemplateId,
                TemplateName = evaluation.Template?.TemplateName ?? "N/A",
                PrimaryEvaluatorId = evaluation.PrimaryEvaluatorId,
                PrimaryEvaluatorName = evaluation.PrimaryEvaluator?.FullName,
                SecondaryEvaluatorId = evaluation.SecondaryEvaluatorId,
                SecondaryEvaluatorName = evaluation.SecondaryEvaluator?.FullName,
                Status = evaluation.Status,
                OverallRating = evaluation.OverallRating,
                SubmittedDate = evaluation.SubmittedDate,
                AcknowledgedDate = evaluation.AcknowledgedDate
            };
        }

        private EvaluationListDto MapToListDto(Evaluation evaluation)
        {
            return new EvaluationListDto
            {
                EvaluationId = evaluation.EvaluationId,
                EmployeeId = evaluation.EmployeeId,
                EmployeeName = evaluation.Employee?.FullName ?? "N/A",
                EmployeeDepartment = evaluation.Employee?.Department?.DepartmentName ?? "N/A",
                PrimaryEvaluatorName = evaluation.PrimaryEvaluator?.FullName ?? "Not Assigned",
                Status = evaluation.Status
            };
        }


    }
}
