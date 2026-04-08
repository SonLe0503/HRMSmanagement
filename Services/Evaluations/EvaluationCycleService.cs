using DocumentFormat.OpenXml.Spreadsheet;
using HRManagement.DataAcess;
using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.DataAcess.Interfaces;
using HRManagement.Services.CurrentUsers;

namespace HRManagement.Services.Evaluations
{
    public class EvaluationCycleService : IEvaluationCycleService
    {
        private readonly IEvaluationCycleRepository _cycleRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeRepository _employeeRepository;

        // Valid cycle types
        private static readonly string[] VALID_CYCLE_TYPES = { "Annual", "Semi-annual", "Quarterly", "Probation" };

        // Valid statuses
        private const string STATUS_DRAFT = "Draft";
        private const string STATUS_ACTIVE = "Active";
        private const string STATUS_CLOSED = "Cancelled";

        public EvaluationCycleService(
            IEvaluationCycleRepository cycleRepository,
            ICurrentUserService currentUserService,
            IEmployeeRepository employeeRepository)
        {
            _cycleRepository = cycleRepository;
            _currentUserService = currentUserService;
            _employeeRepository = employeeRepository;
        }

        public async Task<EvaluationCycleResponseDto> CreateCycleAsync(CreateEvaluationCycleDto createDto)
        {
            // Cycle type validation
            if (!VALID_CYCLE_TYPES.Contains(createDto.CycleType))
            {
                throw new ArgumentException(
                    $"Invalid cycle type. Must be one of: {string.Join(", ", VALID_CYCLE_TYPES)}");
            }

            // Evaluation period validation
            if (createDto.EvaluationPeriodStart >= createDto.EvaluationPeriodEnd)
            {
                throw new ArgumentException(
                    "Evaluation period start date must be before end date.");
            }

            // Self-evaluation period validation
            if (createDto.SelfEvaluationStart >= createDto.SelfEvaluationEnd)
            {
                throw new ArgumentException(
                    "Self-evaluation start date must be before end date.");
            }

            // Manager evaluation period validation
            if (createDto.ManagerEvaluationStart >= createDto.ManagerEvaluationEnd)
            {
                throw new ArgumentException(
                    "Manager evaluation start date must be before end date.");
            }


            if (createDto.ManagerEvaluationStart < createDto.SelfEvaluationEnd)
            {
                throw new ArgumentException(
                    "Manager evaluation should start after or when self-evaluation ends.");
            }

            // Review meeting validation (optional)
            if (createDto.ReviewMeetingStart.HasValue && createDto.ReviewMeetingEnd.HasValue)
            {
                if (createDto.ReviewMeetingStart >= createDto.ReviewMeetingEnd)
                {
                    throw new ArgumentException(
                        "Review meeting start date must be before end date.");
                }

                if (createDto.ReviewMeetingStart < createDto.ManagerEvaluationEnd)
                {
                    throw new ArgumentException(
                        "Review meetings should start after manager evaluation ends.");
                }
            }

            // Step 8: Check for overlapping evaluation cycles
            var hasOverlap = await _cycleRepository.HasOverlappingCycleAsync(
                createDto.EvaluationPeriodStart,
                createDto.EvaluationPeriodEnd);

            if (hasOverlap)
            {
                throw new InvalidOperationException(
                    "An active or draft evaluation cycle already exists for the selected period and departments. " +
                    "Please adjust the cycle scope or close the previous cycle before creating a new one.");
            }

            int currentUserId = _currentUserService.GetCurrentUserId();

            // Step 9: Create the evaluation cycle (as Draft first)
            var cycle = new EvaluationCycle
            {
                CycleName = createDto.CycleName,
                CycleType = createDto.CycleType,
                EvaluationPeriodStart = createDto.EvaluationPeriodStart,
                EvaluationPeriodEnd = createDto.EvaluationPeriodEnd,
                SelfEvaluationStart = createDto.SelfEvaluationStart,
                SelfEvaluationEnd = createDto.SelfEvaluationEnd,
                ManagerEvaluationStart = createDto.ManagerEvaluationStart,
                ManagerEvaluationEnd = createDto.ManagerEvaluationEnd,
                ReviewMeetingStart = createDto.ReviewMeetingStart,
                ReviewMeetingEnd = createDto.ReviewMeetingEnd,
                Status = STATUS_DRAFT, // Created as Draft first
                CreatedDate = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            await _cycleRepository.AddAsync(cycle);

            return await MapToResponseDto(cycle);
        }

        public async Task<EvaluationCycleSummaryDto> GetCycleSummaryAsync(int cycleId)
        {
            // Step 11: Display cycle summary
            var cycle = await _cycleRepository.GetByIdWithDetailsAsync(cycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            // Count assigned evaluators (from evaluations)
            int assignedEvaluators = cycle.Evaluations
                .Select(e => e.PrimaryEvaluatorId)
                .Where(id => id.HasValue)
                .Distinct()
                .Count();

            return new EvaluationCycleSummaryDto
            {
                CycleId = cycle.CycleId,
                CycleName = cycle.CycleName,
                CycleType = cycle.CycleType,
                EvaluationPeriodStart = cycle.EvaluationPeriodStart,
                EvaluationPeriodEnd = cycle.EvaluationPeriodEnd,
                AssignedEvaluatorsCount = assignedEvaluators,
                Status = cycle.Status,
                Timeline = new TimelineOverviewDto
                {
                    SelfEvaluationStart = cycle.SelfEvaluationStart,
                    SelfEvaluationEnd = cycle.SelfEvaluationEnd,
                    ManagerEvaluationStart = cycle.ManagerEvaluationStart,
                    ManagerEvaluationEnd = cycle.ManagerEvaluationEnd,
                    ReviewMeetingStart = cycle.ReviewMeetingStart,
                    ReviewMeetingEnd = cycle.ReviewMeetingEnd
                }
            };
        }

        public async Task<EvaluationCycleResponseDto> ActivateCycleAsync(int cycleId)
        {
            // Step 12: HR Staff confirms cycle activation
            var cycle = await _cycleRepository.GetByIdAsync(cycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            if (cycle.Status == STATUS_ACTIVE)
            {
                throw new InvalidOperationException("Cycle is already active.");
            }

            if (cycle.Status == STATUS_CLOSED)
            {
                throw new InvalidOperationException("Cannot activate a closed cycle.");
            }

            // Step 13: System activates the cycle and sets status to "Active"
            cycle.Status = STATUS_ACTIVE;

            await _cycleRepository.UpdateAsync(cycle);

            // Step 14: System sends notifications (handled by notification service)
            // TODO: Trigger notification service here

            // Step 15: Display success message
            return await MapToResponseDto(cycle);
        }

        public async Task<IEnumerable<EvaluationCycleListDto>> GetAllCyclesAsync()
        {
            var cycles = await _cycleRepository.GetAllAsync();
            return cycles.Select(MapToListDto).ToList();
        }

        public async Task<IEnumerable<EvaluationCycleListDto>> GetActiveCyclesAsync()
        {
            var cycles = await _cycleRepository.GetActiveAsync();
            return cycles.Select(MapToListDto).ToList();
        }

        public async Task<EvaluationCycleResponseDto?> GetCycleByIdAsync(int cycleId)
        {
            var cycle = await _cycleRepository.GetByIdWithDetailsAsync(cycleId);
            if (cycle == null)
                return null;

            return await MapToResponseDto(cycle);
        }

        public async Task<EvaluationCycleResponseDto> UpdateCycleAsync(int cycleId, UpdateEvaluationCycleDto updateDto)
        {
            var cycle = await _cycleRepository.GetByIdAsync(cycleId);
            if (cycle == null)
            {
                throw new KeyNotFoundException("Evaluation cycle not found.");
            }

            if (cycle.Status == STATUS_CLOSED)
            {
                throw new InvalidOperationException("Cannot update a closed cycle.");
            }

            // Validate updated data
            ValidateCycleData(updateDto);

            // Check for overlapping cycles (excluding current cycle)
            var hasOverlap = await _cycleRepository.HasOverlappingCycleAsync(
                updateDto.EvaluationPeriodStart,
                updateDto.EvaluationPeriodEnd,
                cycleId);

            if (hasOverlap)
            {
                throw new InvalidOperationException(
                    "The updated cycle overlaps with an existing active or draft cycle. " +
                    "Please adjust the dates or departments.");
            }

            cycle.CycleName = updateDto.CycleName;
            cycle.CycleType = updateDto.CycleType;
            cycle.EvaluationPeriodStart = updateDto.EvaluationPeriodStart;
            cycle.EvaluationPeriodEnd = updateDto.EvaluationPeriodEnd;
            cycle.SelfEvaluationStart = updateDto.SelfEvaluationStart;
            cycle.SelfEvaluationEnd = updateDto.SelfEvaluationEnd;
            cycle.ManagerEvaluationStart = updateDto.ManagerEvaluationStart;
            cycle.ManagerEvaluationEnd = updateDto.ManagerEvaluationEnd;
            cycle.ReviewMeetingStart = updateDto.ReviewMeetingStart;
            cycle.ReviewMeetingEnd = updateDto.ReviewMeetingEnd;

            await _cycleRepository.UpdateAsync(cycle);

            return await MapToResponseDto(cycle);
        }

        public async Task<bool> CloseCycleAsync(int cycleId, CloseCycleDto closeDto)
        {
            var cycle = await _cycleRepository.GetByIdAsync(cycleId);
            if (cycle == null)
                return false;

            if (cycle.Status == STATUS_CLOSED)
                return false;

            cycle.Status = STATUS_CLOSED;

            await _cycleRepository.UpdateAsync(cycle);

            return true;
        }

        #region Validation

        private void ValidateCycleData(CreateEvaluationCycleDto dto)
        {
            // Required fields validation
            if (string.IsNullOrWhiteSpace(dto.CycleName))
            {
                throw new ArgumentException("Cycle name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.CycleType))
            {
                throw new ArgumentException("Cycle type is required.");
            }

            // Cycle type validation
            if (!VALID_CYCLE_TYPES.Contains(dto.CycleType))
            {
                throw new ArgumentException(
                    $"Invalid cycle type. Must be one of: {string.Join(", ", VALID_CYCLE_TYPES)}");
            }

            // Timeline validation (BR-35)
            ValidateTimeline(
                dto.EvaluationPeriodStart,
                dto.EvaluationPeriodEnd,
                dto.SelfEvaluationStart,
                dto.SelfEvaluationEnd,
                dto.ManagerEvaluationStart,
                dto.ManagerEvaluationEnd,
                dto.ReviewMeetingStart,
                dto.ReviewMeetingEnd);
        }

        private void ValidateCycleData(UpdateEvaluationCycleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CycleName))
            {
                throw new ArgumentException("Cycle name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.CycleType))
            {
                throw new ArgumentException("Cycle type is required.");
            }

            if (!VALID_CYCLE_TYPES.Contains(dto.CycleType))
            {
                throw new ArgumentException(
                    $"Invalid cycle type. Must be one of: {string.Join(", ", VALID_CYCLE_TYPES)}");
            }

            ValidateTimeline(
                dto.EvaluationPeriodStart,
                dto.EvaluationPeriodEnd,
                dto.SelfEvaluationStart,
                dto.SelfEvaluationEnd,
                dto.ManagerEvaluationStart,
                dto.ManagerEvaluationEnd,
                dto.ReviewMeetingStart,
                dto.ReviewMeetingEnd);
        }

        private void ValidateTimeline(
            DateOnly periodStart,
            DateOnly periodEnd,
            DateOnly selfStart,
            DateOnly selfEnd,
            DateOnly managerStart,
            DateOnly managerEnd,
            DateOnly? reviewStart,
            DateOnly? reviewEnd)
        {
            // Evaluation period validation
            if (periodStart >= periodEnd)
            {
                throw new ArgumentException(
                    "Evaluation period start date must be before end date.");
            }

            // Self-evaluation period validation
            if (selfStart >= selfEnd)
            {
                throw new ArgumentException(
                    "Self-evaluation start date must be before end date.");
            }

            // Manager evaluation period validation
            if (managerStart >= managerEnd)
            {
                throw new ArgumentException(
                    "Manager evaluation start date must be before end date.");
            }

            // Timeline sequence validation
            if (selfStart < periodEnd)
            {
                // Self-evaluation should start after evaluation period ends (but can overlap)
                // This is a warning, not an error
            }

            if (managerStart < selfEnd)
            {
                throw new ArgumentException(
                    "Manager evaluation should start after or when self-evaluation ends.");
            }

            // Review meeting validation (optional)
            if (reviewStart.HasValue && reviewEnd.HasValue)
            {
                if (reviewStart >= reviewEnd)
                {
                    throw new ArgumentException(
                        "Review meeting start date must be before end date.");
                }

                if (reviewStart < managerEnd)
                {
                    throw new ArgumentException(
                        "Review meetings should start after manager evaluation ends.");
                }
            }
        }

        #endregion

        #region Mapping

        private async Task<EvaluationCycleResponseDto> MapToResponseDto(EvaluationCycle cycle)
        {
            if (cycle.Evaluations == null)
            {
                var detailedCycle = await _cycleRepository.GetByIdWithDetailsAsync(cycle.CycleId);
                if (detailedCycle != null)
                {
                    cycle = detailedCycle;
                }
            }

            var employee = cycle.CreatedBy.HasValue ? await _employeeRepository.GetEmployeeByIdAsync(cycle.CreatedBy.Value) : null;

            return new EvaluationCycleResponseDto
            {
                CycleId = cycle.CycleId,
                CycleName = cycle.CycleName ?? string.Empty,
                CycleType = cycle.CycleType ?? string.Empty,
                EvaluationPeriodStart = cycle.EvaluationPeriodStart,
                EvaluationPeriodEnd = cycle.EvaluationPeriodEnd,
                SelfEvaluationStart = cycle.SelfEvaluationStart,
                SelfEvaluationEnd = cycle.SelfEvaluationEnd,
                ManagerEvaluationStart = cycle.ManagerEvaluationStart,
                ManagerEvaluationEnd = cycle.ManagerEvaluationEnd,
                ReviewMeetingStart = cycle.ReviewMeetingStart,
                ReviewMeetingEnd = cycle.ReviewMeetingEnd,
                Status = cycle.Status ?? STATUS_DRAFT,
                CreatedDate = cycle.CreatedDate,
                CreatedBy = cycle.CreatedBy,
                CreatedByName = employee?.FullName ?? "System"
            };
        }

        private EvaluationCycleListDto MapToListDto(EvaluationCycle cycle)
        {
            return new EvaluationCycleListDto
            {
                CycleId = cycle.CycleId,
                CycleName = cycle.CycleName ?? string.Empty,
                CycleType = cycle.CycleType ?? string.Empty,
                EvaluationPeriodStart = cycle.EvaluationPeriodStart,
                EvaluationPeriodEnd = cycle.EvaluationPeriodEnd,
                Status = cycle.Status ?? STATUS_DRAFT,
                EmployeeCount = cycle.Evaluations?.Count ?? 0
            };
        }

        #endregion
    }
}

