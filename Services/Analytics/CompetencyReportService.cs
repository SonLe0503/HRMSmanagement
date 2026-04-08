using System.Text;
using System.Text.Json;
using HRManagement.DTOs.CompetencyReport;
using HRManagement.Models;
using HRManagement.Services.CurrentUsers;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services.Analytics
{
    public class CompetencyReportService : ICompetencyReportService
    {
        private readonly HrmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CompetencyReportService(HrmsDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<CompetencyReportResponseDTO> GenerateReportAsync(CompetencyReportFilterDTO filter)
        {
            await ValidateFilterAsync(filter);
            await CheckPermissionAsync(filter);

            var cycleName = filter.CycleId.HasValue
                ? await _context.EvaluationCycles
                    .Where(x => x.CycleId == filter.CycleId.Value)
                    .Select(x => x.CycleName)
                    .FirstOrDefaultAsync()
                : null;

            var query = BuildBaseQuery(filter);
            var rawData = await query.ToListAsync();

            var response = new CompetencyReportResponseDTO
            {
                Scope = filter.Scope,
                CycleName = cycleName,
                HasEnoughData = rawData.Count >= 3
            };

            if (!rawData.Any())
            {
                response.HasEnoughData = false;
                response.DisclaimerMessage = "No competency data found for the selected criteria.";
                await LogAuditAsync("SELECT", filter.CycleId ?? 0, filter);
                return response;
            }

            if (rawData.Count < 5)
            {
                response.DisclaimerMessage = "Insufficient data for analysis. Partial report is displayed.";
            }

            response.CompetencyProfiles = BuildCompetencyProfiles(rawData);
            response.Trends = await BuildTrendAnalysisAsync(filter);

            response.Strengths = response.CompetencyProfiles
                .OrderByDescending(x => x.AverageManagerRating)
                .Take(5)
                .ToList();

            response.DevelopmentGaps = response.CompetencyProfiles
                .OrderBy(x => x.AverageManagerRating)
                .Take(5)
                .ToList();

            if (filter.Scope.Equals("Team", StringComparison.OrdinalIgnoreCase))
            {
                response.EmployeeComparisons = BuildEmployeeComparisons(rawData);
            }

            if (filter.Scope.Equals("Organization", StringComparison.OrdinalIgnoreCase))
            {
                response.DepartmentComparisons = BuildDepartmentComparisons(rawData);
                response.HighLowPerformers = BuildHighLowPerformers(rawData);
            }

            await LogAuditAsync("SELECT", filter.CycleId ?? 0, filter);

            return response;
        }

        public async Task<CompetencyDrilldownResponseDTO> GetDrilldownAsync(CompetencyDrilldownRequestDTO request)
        {
            var query = _context.EvaluationRatings
                .Include(x => x.Criteria)
                .Include(x => x.Evaluation)
                    .ThenInclude(e => e.Employee)
                        .ThenInclude(emp => emp.Department)
                .Include(x => x.Evaluation)
                    .ThenInclude(e => e.Cycle)
                .Where(x =>
                    x.CriteriaId == request.CriteriaId &&
                    x.Evaluation.Cycle.Status == "Completed" &&
                    (x.Evaluation.Status == "Completed" || x.Evaluation.Status == "Acknowledged"))
                .AsQueryable();

            if (request.CycleId.HasValue)
                query = query.Where(x => x.Evaluation.CycleId == request.CycleId.Value);

            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.Evaluation.EmployeeId == request.EmployeeId.Value);

            if (request.DepartmentId.HasValue)
                query = query.Where(x => x.Evaluation.Employee.DepartmentId == request.DepartmentId.Value);

            var data = await query.ToListAsync();
            var first = data.FirstOrDefault();

            var result = new CompetencyDrilldownResponseDTO
            {
                CriteriaId = request.CriteriaId,
                CriteriaName = first?.Criteria?.CriteriaName ?? string.Empty,
                CriteriaCategory = first?.Criteria?.CriteriaCategory,
                Details = data.Select(x => new CompetencyDrilldownItemDTO
                {
                    EmployeeId = x.Evaluation.EmployeeId,
                    EmployeeCode = x.Evaluation.Employee.EmployeeCode,
                    EmployeeName = x.Evaluation.Employee.FullName ?? $"{x.Evaluation.Employee.FirstName} {x.Evaluation.Employee.LastName}",
                    DepartmentName = x.Evaluation.Employee.Department?.DepartmentName,
                    SelfRating = x.SelfRating,
                    ManagerRating = x.ManagerRating,
                    SelfComments = x.SelfComments,
                    ManagerComments = x.ManagerComments
                }).ToList()
            };

            return result;
        }

        public async Task<(byte[] FileContent, string FileName, string ContentType)> ExportReportAsync(ExportCompetencyReportRequestDTO request)
        {
            var report = await GenerateReportAsync(request.Filter);

            var format = request.Format.Trim().ToLower();

            if (format == "csv")
            {
                var csv = BuildCsv(report);
                return (
                    Encoding.UTF8.GetBytes(csv),
                    $"competency-report-{DateTime.Now:yyyyMMddHHmmss}.csv",
                    "text/csv"
                );
            }

            if (format == "excel")
            {
                var csv = BuildCsv(report);
                return (
                    Encoding.UTF8.GetBytes(csv),
                    $"competency-report-{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                );
            }

            if (format == "pdf")
            {
                var text = BuildPlainTextReport(report);
                return (
                    Encoding.UTF8.GetBytes(text),
                    $"competency-report-{DateTime.Now:yyyyMMddHHmmss}.pdf",
                    "application/pdf"
                );
            }

            throw new ArgumentException("Unsupported export format. Allowed values: csv, excel, pdf.");
        }

        private IQueryable<EvaluationRating> BuildBaseQuery(CompetencyReportFilterDTO filter)
        {
            var query = _context.EvaluationRatings
                .Include(x => x.Criteria)
                .Include(x => x.Evaluation)
                    .ThenInclude(e => e.Employee)
                        .ThenInclude(emp => emp.Department)
                .Include(x => x.Evaluation)
                    .ThenInclude(e => e.Cycle)
                .Where(x =>
                    x.Evaluation.Cycle.Status == "Completed" &&
                    (x.Evaluation.Status == "Completed" || x.Evaluation.Status == "Acknowledged"))
                .AsQueryable();

            if (filter.CycleId.HasValue)
                query = query.Where(x => x.Evaluation.CycleId == filter.CycleId.Value);

            if (filter.CriteriaIds != null && filter.CriteriaIds.Any())
                query = query.Where(x => filter.CriteriaIds.Contains(x.CriteriaId));

            if (!string.IsNullOrWhiteSpace(filter.CriteriaCategory))
                query = query.Where(x => x.Criteria.CriteriaCategory == filter.CriteriaCategory);

            if (filter.EmployeeId.HasValue)
                query = query.Where(x => x.Evaluation.EmployeeId == filter.EmployeeId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(x => x.Evaluation.Employee.DepartmentId == filter.DepartmentId.Value);

            if (filter.Scope.Equals("Team", StringComparison.OrdinalIgnoreCase) && _currentUserService.EmployeeId.HasValue)
            {
                var managerEmployeeId = _currentUserService.EmployeeId.Value;
                query = query.Where(x => x.Evaluation.Employee.ManagerId == managerEmployeeId);
            }

            return query;
        }

        private List<CompetencyReportItemDTO> BuildCompetencyProfiles(List<EvaluationRating> rawData)
        {
            return rawData
                .GroupBy(x => new
                {
                    x.CriteriaId,
                    x.Criteria.CriteriaName,
                    x.Criteria.CriteriaCategory
                })
                .Select(g =>
                {
                    var managerAvg = g.Where(x => x.ManagerRating.HasValue)
                        .Select(x => x.ManagerRating!.Value)
                        .DefaultIfEmpty(0)
                        .Average();

                    var hasSelf = g.Any(x => x.SelfRating.HasValue);
                    decimal? selfAvg = hasSelf
                        ? g.Where(x => x.SelfRating.HasValue)
                            .Select(x => x.SelfRating!.Value)
                            .Average()
                        : null;

                    return new CompetencyReportItemDTO
                    {
                        CriteriaId = g.Key.CriteriaId,
                        CriteriaName = g.Key.CriteriaName,
                        CriteriaCategory = g.Key.CriteriaCategory,
                        AverageManagerRating = Math.Round(managerAvg, 2),
                        AverageSelfRating = selfAvg.HasValue ? Math.Round(selfAvg.Value, 2) : null,
                        Gap = Math.Round((selfAvg ?? 0) - managerAvg, 2)
                    };
                })
                .OrderBy(x => x.CriteriaCategory)
                .ThenBy(x => x.CriteriaName)
                .ToList();
        }

        private async Task<List<CompetencyTrendDTO>> BuildTrendAnalysisAsync(CompetencyReportFilterDTO filter)
        {
            var query = _context.EvaluationRatings
                .Include(x => x.Criteria)
                .Include(x => x.Evaluation)
                    .ThenInclude(e => e.Cycle)
                .Include(x => x.Evaluation)
                    .ThenInclude(e => e.Employee)
                .Where(x =>
                    x.Evaluation.Cycle.Status == "Completed" &&
                    (x.Evaluation.Status == "Completed" || x.Evaluation.Status == "Acknowledged"))
                .AsQueryable();

            if (filter.CriteriaIds != null && filter.CriteriaIds.Any())
                query = query.Where(x => filter.CriteriaIds.Contains(x.CriteriaId));

            if (!string.IsNullOrWhiteSpace(filter.CriteriaCategory))
                query = query.Where(x => x.Criteria.CriteriaCategory == filter.CriteriaCategory);

            if (filter.EmployeeId.HasValue)
                query = query.Where(x => x.Evaluation.EmployeeId == filter.EmployeeId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(x => x.Evaluation.Employee.DepartmentId == filter.DepartmentId.Value);

            if (filter.Scope.Equals("Team", StringComparison.OrdinalIgnoreCase) && _currentUserService.EmployeeId.HasValue)
            {
                var managerEmployeeId = _currentUserService.EmployeeId.Value;
                query = query.Where(x => x.Evaluation.Employee.ManagerId == managerEmployeeId);
            }

            var data = await query.ToListAsync();

            return data
                .GroupBy(x => new { x.CriteriaId, x.Criteria.CriteriaName })
                .Select(g => new CompetencyTrendDTO
                {
                    CriteriaId = g.Key.CriteriaId,
                    CriteriaName = g.Key.CriteriaName,
                    Points = g.GroupBy(x => new { x.Evaluation.CycleId, x.Evaluation.Cycle.CycleName })
                        .Select(p =>
                        {
                            var managerAvg = p.Where(x => x.ManagerRating.HasValue)
                                .Select(x => x.ManagerRating!.Value)
                                .DefaultIfEmpty(0)
                                .Average();

                            var hasSelf = p.Any(x => x.SelfRating.HasValue);
                            decimal? selfAvg = hasSelf
                                ? p.Where(x => x.SelfRating.HasValue)
                                    .Select(x => x.SelfRating!.Value)
                                    .Average()
                                : null;

                            return new CompetencyTrendPointDTO
                            {
                                CycleId = p.Key.CycleId,
                                CycleName = p.Key.CycleName,
                                AverageManagerRating = Math.Round(managerAvg, 2),
                                AverageSelfRating = selfAvg.HasValue ? Math.Round(selfAvg.Value, 2) : null
                            };
                        })
                        .OrderBy(x => x.CycleId)
                        .ToList()
                })
                .ToList();
        }

        private List<EmployeeComparisonDTO> BuildEmployeeComparisons(List<EvaluationRating> rawData)
        {
            var teamAverage = Math.Round(
                rawData.Where(x => x.ManagerRating.HasValue)
                    .Select(x => x.ManagerRating!.Value)
                    .DefaultIfEmpty(0)
                    .Average(), 2);

            return rawData
                .GroupBy(x => new
                {
                    x.Evaluation.EmployeeId,
                    x.Evaluation.Employee.EmployeeCode,
                    x.Evaluation.Employee.FullName
                })
                .Select(g => new EmployeeComparisonDTO
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeCode = g.Key.EmployeeCode,
                    EmployeeName = g.Key.FullName ?? string.Empty,
                    EmployeeAverageRating = Math.Round(
                        g.Where(x => x.ManagerRating.HasValue)
                         .Select(x => x.ManagerRating!.Value)
                         .DefaultIfEmpty(0)
                         .Average(), 2),
                    TeamAverageRating = teamAverage
                })
                .OrderByDescending(x => x.EmployeeAverageRating)
                .ToList();
        }

        private List<DepartmentComparisonDTO> BuildDepartmentComparisons(List<EvaluationRating> rawData)
        {
            return rawData
                .Where(x => x.Evaluation.Employee.DepartmentId.HasValue)
                .GroupBy(x => new
                {
                    DepartmentId = x.Evaluation.Employee.DepartmentId!.Value,
                    DepartmentName = x.Evaluation.Employee.Department != null
                        ? x.Evaluation.Employee.Department.DepartmentName
                        : "Unknown"
                })
                .Select(g => new DepartmentComparisonDTO
                {
                    DepartmentId = g.Key.DepartmentId,
                    DepartmentName = g.Key.DepartmentName,
                    AverageRating = Math.Round(
                        g.Where(x => x.ManagerRating.HasValue)
                         .Select(x => x.ManagerRating!.Value)
                         .DefaultIfEmpty(0)
                         .Average(), 2)
                })
                .OrderByDescending(x => x.AverageRating)
                .ToList();
        }

        private List<HighLowPerformerDTO> BuildHighLowPerformers(List<EvaluationRating> rawData)
        {
            var grouped = rawData
                .GroupBy(x => new
                {
                    x.Evaluation.EmployeeId,
                    x.Evaluation.Employee.EmployeeCode,
                    x.Evaluation.Employee.FullName
                })
                .Select(g => new
                {
                    g.Key.EmployeeId,
                    g.Key.EmployeeCode,
                    g.Key.FullName,
                    AverageRating = Math.Round(
                        g.Where(x => x.ManagerRating.HasValue)
                         .Select(x => x.ManagerRating!.Value)
                         .DefaultIfEmpty(0)
                         .Average(), 2)
                })
                .OrderByDescending(x => x.AverageRating)
                .ToList();

            var high = grouped.Take(5).Select(x => new HighLowPerformerDTO
            {
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.EmployeeCode,
                EmployeeName = x.FullName ?? string.Empty,
                AverageRating = x.AverageRating,
                Group = "High"
            });

            var low = grouped.OrderBy(x => x.AverageRating).Take(5).Select(x => new HighLowPerformerDTO
            {
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.EmployeeCode,
                EmployeeName = x.FullName ?? string.Empty,
                AverageRating = x.AverageRating,
                Group = "Low"
            });

            return high.Concat(low).ToList();
        }

        private async System.Threading.Tasks.Task ValidateFilterAsync(CompetencyReportFilterDTO filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Scope))
                throw new ArgumentException("Scope is required.");

            var allowedScopes = new[] { "Individual", "Team", "Organization" };
            if (!allowedScopes.Contains(filter.Scope, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Scope must be Individual, Team, or Organization.");

            if (filter.Scope.Equals("Individual", StringComparison.OrdinalIgnoreCase) && !filter.EmployeeId.HasValue)
                throw new ArgumentException("EmployeeId is required for Individual scope.");

            if (filter.CycleId.HasValue)
            {
                var cycle = await _context.EvaluationCycles.FirstOrDefaultAsync(x => x.CycleId == filter.CycleId.Value);
                if (cycle == null)
                    throw new ArgumentException("Evaluation cycle not found.");

                if (cycle.Status != "Completed")
                    throw new ArgumentException("Only completed evaluation cycles can be used.");
            }
        }

        private async System.Threading.Tasks.Task CheckPermissionAsync(CompetencyReportFilterDTO filter)
        {
            var role = _currentUserService.RoleName;
            var currentEmployeeId = _currentUserService.EmployeeId;

            if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                if (!currentEmployeeId.HasValue)
                    throw new UnauthorizedAccessException("Current user is not linked to an employee.");

                if (!filter.Scope.Equals("Individual", StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Employee can only view individual report.");

                if (filter.EmployeeId != currentEmployeeId.Value)
                    throw new UnauthorizedAccessException("Employee can only view their own report.");
            }
            else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (filter.Scope.Equals("Organization", StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Manager cannot view organization report.");

                if (filter.Scope.Equals("Individual", StringComparison.OrdinalIgnoreCase) && filter.EmployeeId.HasValue)
                {
                    var isSubordinate = await _context.Employees.AnyAsync(x =>
                        x.EmployeeId == filter.EmployeeId.Value &&
                        currentEmployeeId.HasValue &&
                        x.ManagerId == currentEmployeeId.Value);

                    if (!isSubordinate && filter.EmployeeId != currentEmployeeId)
                        throw new UnauthorizedAccessException("You do not have permission to view this employee report.");
                }
            }
            else if (string.Equals(role, "HR Staff", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            else
            {
                throw new UnauthorizedAccessException("Access denied.");
            }
        }

        private async System.Threading.Tasks.Task LogAuditAsync(string action, int recordId, object payload)
        {
            var log = new AuditLog
            {
                TableName = "CompetencyReports",
                Action = action,
                RecordId = recordId,
                UserId = _currentUserService.UserId > 0 ? _currentUserService.UserId : null,
                NewValues = JsonSerializer.Serialize(payload),
                ActionDate = DateTime.Now,
                Ipaddress = null
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private string BuildCsv(CompetencyReportResponseDTO report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Scope,Cycle");
            sb.AppendLine($"{EscapeCsv(report.Scope)},{EscapeCsv(report.CycleName)}");
            sb.AppendLine();

            sb.AppendLine("Competency Profiles");
            sb.AppendLine("CriteriaId,CriteriaName,CriteriaCategory,AverageManagerRating,AverageSelfRating,Gap");

            foreach (var item in report.CompetencyProfiles)
            {
                sb.AppendLine($"{item.CriteriaId},{EscapeCsv(item.CriteriaName)},{EscapeCsv(item.CriteriaCategory)},{item.AverageManagerRating},{item.AverageSelfRating},{item.Gap}");
            }

            if (report.EmployeeComparisons.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Employee Comparisons");
                sb.AppendLine("EmployeeId,EmployeeCode,EmployeeName,EmployeeAverageRating,TeamAverageRating");

                foreach (var item in report.EmployeeComparisons)
                {
                    sb.AppendLine($"{item.EmployeeId},{EscapeCsv(item.EmployeeCode)},{EscapeCsv(item.EmployeeName)},{item.EmployeeAverageRating},{item.TeamAverageRating}");
                }
            }

            if (report.DepartmentComparisons.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Department Comparisons");
                sb.AppendLine("DepartmentId,DepartmentName,AverageRating");

                foreach (var item in report.DepartmentComparisons)
                {
                    sb.AppendLine($"{item.DepartmentId},{EscapeCsv(item.DepartmentName)},{item.AverageRating}");
                }
            }

            return sb.ToString();
        }

        private string BuildPlainTextReport(CompetencyReportResponseDTO report)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Scope: {report.Scope}");
            sb.AppendLine($"Cycle: {report.CycleName}");
            sb.AppendLine();

            sb.AppendLine("Competency Profiles:");
            foreach (var item in report.CompetencyProfiles)
            {
                sb.AppendLine($"- {item.CriteriaName} | Category: {item.CriteriaCategory} | Manager Avg: {item.AverageManagerRating} | Self Avg: {item.AverageSelfRating}");
            }

            return sb.ToString();
        }

        private string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";

            return value;
        }
    }
}
