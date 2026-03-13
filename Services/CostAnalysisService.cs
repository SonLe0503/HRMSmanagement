using System.Text.Json;
using HRManagement.DTOs.CostAnalysis;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class CostAnalysisService : ICostAnalysisService
    {
        private readonly HrmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public CostAnalysisService(
            HrmsDbContext context,
            ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        public async Task<CostAnalysisResponseDTO> GenerateCostAnalysisAsync(CostAnalysisRequestDTO request)
        {
            int userId = _currentUserService.GetUserId();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = request.StartDate ?? today.AddMonths(-12);
            var endDate = request.EndDate ?? today;

            var employeeQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            if (request.BreakdownLevel.Equals("department", StringComparison.OrdinalIgnoreCase) && request.DepartmentId.HasValue)
            {
                employeeQuery = employeeQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);
            }

            var employees = await employeeQuery.ToListAsync();

            if (employees.Count < 1)
            {
                return new CostAnalysisResponseDTO
                {
                    Success = false,
                    Message = "MSG-88: Incomplete or missing cost data."
                };
            }

            var employeeIds = employees.Select(e => e.EmployeeId).ToList();

            var payrollRecords = await _context.PayrollRecords
                .Include(p => p.Employee)
                .Include(p => p.Period)
                .Where(p => employeeIds.Contains(p.EmployeeId))
                .ToListAsync();

            var overtimeRequests = await _context.OvertimeRequests
                .Where(o => employeeIds.Contains(o.EmployeeId)
                            && o.OvertimeDate >= startDate
                            && o.OvertimeDate <= endDate
                            && o.Status == "Approved")
                .ToListAsync();

            var employeeContracts = await _context.EmployeeContracts
                .Where(c => employeeIds.Contains(c.EmployeeId))
                .ToListAsync();

            var resignedEmployees = employees
                .Where(e => e.ResignationDate.HasValue
                            && e.ResignationDate.Value >= startDate
                            && e.ResignationDate.Value <= endDate)
                .ToList();

            decimal totalPayrollCost = payrollRecords.Sum(p => p.NetPay ?? 0);
            decimal totalBaseSalary = payrollRecords.Sum(p => p.BaseSalary);
            decimal totalVariablePay = payrollRecords.Sum(p => p.BonusAmount + p.OvertimePay + p.TotalAllowances);
            decimal totalInsurance = payrollRecords.Sum(p => p.InsuranceAmount);
            decimal totalOvertime = payrollRecords.Sum(p => p.OvertimePay);
            decimal totalBenefits = payrollRecords.Sum(p => p.InsuranceAmount + p.TotalAllowances);
            decimal totalWorkforceCost = payrollRecords.Sum(p => p.BaseSalary + p.TotalAllowances + p.OvertimePay + p.BonusAmount);
            decimal costPerEmployee = employees.Count == 0 ? 0 : Math.Round(totalWorkforceCost / employees.Count, 2);

            await _auditService.TrackAsync(userId, "SELECT", "Viewed cost analysis");

            return new CostAnalysisResponseDTO
            {
                Success = true,
                Message = "Cost analysis generated successfully.",
                TotalCostAnalytics = BuildTotalCostAnalytics(employees, payrollRecords, totalWorkforceCost, costPerEmployee),
                SalaryCompensationAnalytics = BuildSalaryAnalytics(payrollRecords, totalBaseSalary, totalVariablePay, totalOvertime),
                BenefitsCostAnalytics = BuildBenefitsAnalytics(totalInsurance, totalBenefits),
                RecruitmentCostAnalytics = BuildRecruitmentAnalytics(),
                TrainingDevelopmentCostAnalytics = BuildTrainingAnalytics(employees.Count),
                AttritionCostAnalytics = BuildAttritionAnalytics(resignedEmployees, employees),
                CostEfficiencyMetrics = BuildEfficiencyMetrics(totalWorkforceCost, employees)
            };
        }

        public async Task<CostScenarioResponseDTO> CreateScenarioAsync(CostScenarioDTO request)
        {
            int userId = _currentUserService.GetUserId();

            var employees = await _context.Employees
                .Where(e => e.EmploymentStatus == "Active")
                .ToListAsync();

            var payrollRecords = await _context.PayrollRecords.ToListAsync();

            decimal currentCost = payrollRecords.Sum(p => p.BaseSalary + p.TotalAllowances + p.OvertimePay + p.BonusAmount);

            int currentHeadcount = employees.Count;
            decimal avgBaseSalary = currentHeadcount == 0 ? 0 : payrollRecords.Sum(p => p.BaseSalary) / currentHeadcount;
            decimal avgBenefits = currentHeadcount == 0 ? 0 : payrollRecords.Sum(p => p.InsuranceAmount + p.TotalAllowances) / currentHeadcount;

            int projectedHeadcount = currentHeadcount + request.HeadcountChange;
            if (projectedHeadcount < 0) projectedHeadcount = 0;

            decimal projectedSalaryCost = projectedHeadcount * avgBaseSalary * (1 + request.SalaryAdjustmentPercent / 100);
            decimal projectedBenefitsCost = projectedHeadcount * avgBenefits * (1 + request.BenefitsAdjustmentPercent / 100);
            decimal projectedOtherCost = (currentCost - payrollRecords.Sum(p => p.BaseSalary) - payrollRecords.Sum(p => p.InsuranceAmount + p.TotalAllowances))
                                         * (1 + request.OtherCostAdjustmentPercent / 100);

            decimal scenarioCost = projectedSalaryCost + projectedBenefitsCost + projectedOtherCost;

            await _auditService.TrackAsync(userId, "SELECT", "Created cost scenario");

            return new CostScenarioResponseDTO
            {
                Success = true,
                Message = "Cost scenario calculated successfully.",
                CurrentCost = Math.Round(currentCost, 2),
                ScenarioCost = Math.Round(scenarioCost, 2),
                CostDifference = Math.Round(scenarioCost - currentCost, 2),
                Breakdown = new List<object>
                {
                    new { Category = "Salary Cost", Current = payrollRecords.Sum(p => p.BaseSalary), Scenario = Math.Round(projectedSalaryCost, 2) },
                    new { Category = "Benefits Cost", Current = payrollRecords.Sum(p => p.InsuranceAmount + p.TotalAllowances), Scenario = Math.Round(projectedBenefitsCost, 2) },
                    new { Category = "Other Cost", Current = currentCost - payrollRecords.Sum(p => p.BaseSalary) - payrollRecords.Sum(p => p.InsuranceAmount + p.TotalAllowances), Scenario = Math.Round(projectedOtherCost, 2) }
                }
            };
        }

        public async Task<string> SetCostAlertAsync(CostAlertDTO request)
        {
            int userId = _currentUserService.GetUserId();

            var key = $"COST_ALERT_{userId}_{Guid.NewGuid():N}";
            var value = JsonSerializer.Serialize(request);

            var setting = new SystemSetting
            {
                SettingKey = key,
                SettingValue = value,
                SettingCategory = "General",
                Description = "Cost analysis alert configuration",
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = userId
            };

            _context.SystemSettings.Add(setting);
            await _context.SaveChangesAsync();

            await _auditService.TrackAsync(userId, "INSERT", "Configured cost alert");

            return "MSG-89: Cost alert created successfully.";
        }

        private TotalCostAnalyticsDTO BuildTotalCostAnalytics(
            List<Employee> employees,
            List<PayrollRecord> payrollRecords,
            decimal totalWorkforceCost,
            decimal costPerEmployee)
        {
            var byDepartment = payrollRecords
                .GroupBy(p => p.Employee.Department?.DepartmentName ?? "Unknown")
                .Select(g => new
                {
                    Department = g.Key,
                    Cost = g.Sum(x => x.BaseSalary + x.TotalAllowances + x.OvertimePay + x.BonusAmount)
                })
                .Cast<object>()
                .ToList();

            return new TotalCostAnalyticsDTO
            {
                TotalWorkforceCost = Math.Round(totalWorkforceCost, 2),
                CostPerEmployee = Math.Round(costPerEmployee, 2),
                CostByDepartment = byDepartment,
                CostTrend = new List<object>
                {
                    new { Period = "Current", Cost = Math.Round(totalWorkforceCost, 2) }
                },
                BudgetVsActual = new List<object>
                {
                    new { Budget = Math.Round(totalWorkforceCost * 1.05m, 2), Actual = Math.Round(totalWorkforceCost, 2) }
                },
                ForecastCost = Math.Round(totalWorkforceCost * 1.03m, 2)
            };
        }

        private SalaryCompensationAnalyticsDTO BuildSalaryAnalytics(
            List<PayrollRecord> payrollRecords,
            decimal totalBaseSalary,
            decimal totalVariablePay,
            decimal totalOvertime)
        {
            var payEquity = payrollRecords
                .GroupBy(p => p.Employee.PositionId)
                .Select(g => new
                {
                    PositionId = g.Key,
                    AvgBaseSalary = g.Average(x => x.BaseSalary)
                })
                .Cast<object>()
                .ToList();

            decimal compensationRatio = totalBaseSalary == 0 ? 0 : Math.Round(totalVariablePay / totalBaseSalary, 2);

            return new SalaryCompensationAnalyticsDTO
            {
                BaseSalaryCosts = Math.Round(totalBaseSalary, 2),
                VariablePayCosts = Math.Round(totalVariablePay, 2),
                OvertimeCosts = Math.Round(totalOvertime, 2),
                SalaryIncreaseAnalysis = new List<object>(),
                CompensationRatio = compensationRatio,
                PayEquityAnalysis = payEquity
            };
        }

        private BenefitsCostAnalyticsDTO BuildBenefitsAnalytics(decimal totalInsurance, decimal totalBenefits)
        {
            decimal otherBenefits = Math.Max(0, totalBenefits - totalInsurance);

            return new BenefitsCostAnalyticsDTO
            {
                HealthInsuranceCosts = Math.Round(totalInsurance, 2),
                RetirementContributions = 0,
                OtherBenefitsCosts = Math.Round(otherBenefits, 2),
                CostPerBenefitType = new List<object>
                {
                    new { BenefitType = "Insurance", Cost = Math.Round(totalInsurance, 2) },
                    new { BenefitType = "Other Benefits", Cost = Math.Round(otherBenefits, 2) }
                },
                BenefitsUtilizationRate = 0,
                BenefitsRoiAnalysis = 0
            };
        }

        private RecruitmentCostAnalyticsDTO BuildRecruitmentAnalytics()
        {
            return new RecruitmentCostAnalyticsDTO
            {
                TotalRecruitmentCost = 0,
                CostPerHireBySource = new List<object>(),
                CostPerHireByPosition = new List<object>(),
                RecruitmentCostTrend = new List<object>(),
                RecruitmentRoi = 0
            };
        }

        private TrainingDevelopmentCostAnalyticsDTO BuildTrainingAnalytics(int employeeCount)
        {
            return new TrainingDevelopmentCostAnalyticsDTO
            {
                TrainingCostsPerEmployee = 0,
                TotalTrainingCost = 0,
                TrainingCostsByProgram = new List<object>(),
                TrainingRoi = 0,
                ExternalVsInternalTrainingCostRatio = 0
            };
        }

        private AttritionCostAnalyticsDTO BuildAttritionAnalytics(List<Employee> resignedEmployees, List<Employee> employees)
        {
            decimal avgReplacementCost = employees.Count == 0
                ? 0
                : employees.Where(e => e.BaseSalary.HasValue).Select(e => e.BaseSalary!.Value).DefaultIfEmpty(0).Average() * 0.5m;

            decimal turnoverCost = resignedEmployees.Count * avgReplacementCost;
            decimal lostProductivity = resignedEmployees.Count * avgReplacementCost * 0.3m;
            decimal knowledgeLoss = resignedEmployees.Count * avgReplacementCost * 0.2m;

            var replacementByPosition = resignedEmployees
                .GroupBy(e => e.Position?.PositionName ?? "Unknown")
                .Select(g => new
                {
                    Position = g.Key,
                    ReplacementCost = Math.Round(g.Count() * avgReplacementCost, 2)
                })
                .Cast<object>()
                .ToList();

            return new AttritionCostAnalyticsDTO
            {
                CostOfTurnover = Math.Round(turnoverCost, 2),
                ReplacementCostByPosition = replacementByPosition,
                LostProductivityCost = Math.Round(lostProductivity, 2),
                KnowledgeLossImpact = Math.Round(knowledgeLoss, 2)
            };
        }

        private CostEfficiencyMetricsDTO BuildEfficiencyMetrics(decimal totalWorkforceCost, List<Employee> employees)
        {
            int activeEmployees = employees.Count(e => e.EmploymentStatus == "Active");
            decimal revenuePerEmployee = 0;
            decimal profitPerEmployee = 0;
            decimal hrCostAsPercentOfRevenue = 0;
            decimal spanOfControl = employees.Count == 0 ? 0 : Math.Round((decimal)employees.Count / Math.Max(1, employees.Count(e => e.ManagerId.HasValue)), 2);
            decimal productivityCostRatio = activeEmployees == 0 ? 0 : Math.Round(totalWorkforceCost / activeEmployees, 2);

            return new CostEfficiencyMetricsDTO
            {
                RevenuePerEmployee = revenuePerEmployee,
                ProfitPerEmployee = profitPerEmployee,
                HrCostAsPercentOfRevenue = hrCostAsPercentOfRevenue,
                SpanOfControlEfficiency = spanOfControl,
                ProductivityCostRatio = productivityCostRatio
            };
        }
    }
}