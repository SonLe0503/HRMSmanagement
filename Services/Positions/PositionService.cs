using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs;
using HRManagement.Models;
using System.Security.Claims;

namespace HRManagement.Services.Positions
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PositionService(IPositionRepository positionRepository, IHttpContextAccessor httpContextAccessor)
        {
            _positionRepository = positionRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<PositionResponseDto> CreatePositionAsync(CreatePositionDto createDto)
        {
            if (await _positionRepository.PositionCodeExistsAsync(createDto.PositionCode))
            {
                throw new InvalidOperationException($"Position code '{createDto.PositionCode}' already exists.");
            }

            var position = new Position
            {
                PositionCode = createDto.PositionCode,
                PositionName = createDto.PositionName,
                Description = createDto.Description,
                Level = createDto.Level,
                IsTopLevel = createDto.IsTopLevel,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId()
            };

            await _positionRepository.AddAsync(position);

            return new PositionResponseDto
            {
                PositionId = position.PositionId,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Description = position.Description,
                Level = position.Level,
                IsTopLevel = position.IsTopLevel,
                IsActive = position.IsActive,
                EmployeeCount = position.Employees?.Count ?? 0,
                CreatedDate = position.CreatedDate,
                CreatedBy = position.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = position.ModifiedDate,
                ModifiedBy = position.ModifiedBy,
                ModifiedByName = position.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<IEnumerable<PositionListDto>> GetAllPositionsAsync()
        {
            var positions = await _positionRepository.GetAllAsync();
            return positions.Select(position => new PositionListDto
            {
                PositionId = position.PositionId,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Level = position.Level,
                IsTopLevel = position.IsTopLevel,
                EmployeeCount = position.Employees?.Count ?? 0,
                IsActive = position.IsActive
            }).ToList();
        }

        public async Task<IEnumerable<PositionListDto>> GetActivePositionsAsync()
        {
            var positions = await _positionRepository.GetActiveAsync();
            return positions.Select(position => new PositionListDto
            {
                PositionId = position.PositionId,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Level = position.Level,
                IsTopLevel = position.IsTopLevel,
                EmployeeCount = position.Employees?.Count ?? 0,
                IsActive = position.IsActive
            }).ToList();
        }

        public async Task<PositionResponseDto?> GetPositionByIdAsync(int positionId)
        {
            var position = await _positionRepository.GetByIdWithDetailsAsync(positionId);
            if (position == null)
                return null;

            return new PositionResponseDto
            {
                PositionId = position.PositionId,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Description = position.Description,
                Level = position.Level,
                IsTopLevel = position.IsTopLevel,
                IsActive = position.IsActive,
                EmployeeCount = position.Employees?.Count ?? 0,
                CreatedDate = position.CreatedDate,
                CreatedBy = position.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = position.ModifiedDate,
                ModifiedBy = position.ModifiedBy,
                ModifiedByName = position.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<PositionResponseDto> UpdatePositionAsync(int positionId, UpdatePositionDto updateDto)
        {
            var position = await _positionRepository.GetByIdAsync(positionId);
            if (position == null)
            {
                throw new KeyNotFoundException("Position not found.");
            }

            if (await _positionRepository.PositionCodeExistsAsync(updateDto.PositionCode, positionId))
            {
                throw new InvalidOperationException($"Position code '{updateDto.PositionCode}' already exists.");
            }

            position.PositionCode = updateDto.PositionCode;
            position.PositionName = updateDto.PositionName;
            position.Description = updateDto.Description;
            position.Level = updateDto.Level;
            position.IsTopLevel = updateDto.IsTopLevel;
            position.ModifiedDate = DateTime.UtcNow;
            position.ModifiedBy = GetCurrentUserId();

            await _positionRepository.UpdateAsync(position);

            return new PositionResponseDto
            {
                PositionId = position.PositionId,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Description = position.Description,
                Level = position.Level,
                IsTopLevel = position.IsTopLevel,
                IsActive = position.IsActive,
                EmployeeCount = position.Employees?.Count ?? 0,
                CreatedDate = position.CreatedDate,
                CreatedBy = position.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = position.ModifiedDate,
                ModifiedBy = position.ModifiedBy,
                ModifiedByName = position.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<bool> DeactivatePositionAsync(int positionId)
        {
            var position = await _positionRepository.GetByIdAsync(positionId);
            if (position == null)
                return false;

            if (!position.IsActive)
                return false;

            if (await _positionRepository.HasEmployeesAsync(positionId))
            {
                throw new InvalidOperationException("Cannot deactivate position with active employees.");
            }

            position.IsActive = false;
            position.ModifiedDate = DateTime.UtcNow;
            position.ModifiedBy = GetCurrentUserId();

            await _positionRepository.UpdateAsync(position);

            return true;
        }

        public async Task<bool> ActivatePositionAsync(int positionId)
        {
            var position = await _positionRepository.GetByIdAsync(positionId);
            if (position == null)
                return false;

            if (position.IsActive)
                return false;

            position.IsActive = true;
            position.ModifiedDate = DateTime.UtcNow;
            position.ModifiedBy = GetCurrentUserId();

            await _positionRepository.UpdateAsync(position);

            return true;
        }
        private int GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claim, out int userId))
                return userId;

            return 0;
        }

    }
}
