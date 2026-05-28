using AutoMapper;
using HRManagement.DTOs.Payroll;
using HRManagement.Models;
using System.Linq;

namespace HRManagement.Mappers
{
    public class PayrollProfile : Profile
    {
        public PayrollProfile()
        {
            CreateMap<PayrollPeriod, PayrollPeriodDto>()
                .ForMember(d => d.AttendanceCutoffDate, opt => opt.MapFrom(s => s.AttendanceCutoffDate))
                .ForMember(d => d.ReviewWindowDays,     opt => opt.MapFrom(s => s.ReviewWindowDays))
                .ForMember(d => d.TotalEmployees, opt => opt.MapFrom(s => s.PayrollRecords.Count))
                // Tính từ components (không dùng GrossPay/NetPay nullable/stale trong DB)
                .ForMember(d => d.TotalGrossPay,  opt => opt.MapFrom(s => s.PayrollRecords.Sum(r =>
                    (r.WorkingDays > 0
                        ? Math.Round(r.BaseSalary / r.WorkingDays * r.ActualWorkingDays, 0)
                        : 0m)
                    + r.TotalAllowances + r.OvertimePay + r.BonusAmount)))
                .ForMember(d => d.TotalInsurance, opt => opt.MapFrom(s => s.PayrollRecords.Sum(r => r.InsuranceAmount)))
                .ForMember(d => d.TotalTax,       opt => opt.MapFrom(s => s.PayrollRecords.Sum(r => r.TaxAmount)))
                .ForMember(d => d.TotalNetPay,    opt => opt.MapFrom(s => s.PayrollRecords.Sum(r =>
                    (r.WorkingDays > 0
                        ? Math.Round(r.BaseSalary / r.WorkingDays * r.ActualWorkingDays, 0)
                        : 0m)
                    + r.TotalAllowances + r.OvertimePay + r.BonusAmount
                    - r.InsuranceAmount - r.TaxAmount)));

            CreateMap<PayrollRecord, PayrollRecordDto>()
                .ForMember(d => d.EmployeeCode,    opt => opt.MapFrom(s => s.Employee.EmployeeCode))
                .ForMember(d => d.EmployeeName,    opt => opt.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.DepartmentName,  opt => opt.MapFrom(s => s.Employee.Department.DepartmentName))
                .ForMember(d => d.PositionName,    opt => opt.MapFrom(s => s.Employee.Position.PositionName))
                // Lương theo ngày công = BaseSalary / WorkingDays × ActualWorkingDays
                .ForMember(d => d.SalariedAmount,  opt => opt.MapFrom(s =>
                    s.WorkingDays > 0
                        ? Math.Round(s.BaseSalary / s.WorkingDays * s.ActualWorkingDays, 0)
                        : 0m))
                // GrossPay và NetPay là nullable trong model
                .ForMember(d => d.GrossPay,        opt => opt.MapFrom(s => s.GrossPay ?? 0m))
                .ForMember(d => d.NetPay,          opt => opt.MapFrom(s => s.NetPay ?? 0m))
                .ForMember(d => d.Allowances,      opt => opt.MapFrom(s => s.PayrollAllowances))
                .ForMember(d => d.Deductions,      opt => opt.MapFrom(s => s.PayrollDeductions));

            CreateMap<PayrollAllowance, PayrollAllowanceDto>();
            CreateMap<PayrollDeduction, PayrollDeductionDto>();

            CreateMap<PayrollFeedback, PayrollFeedbackDto>()
                .ForMember(d => d.IsAgreed, opt => opt.MapFrom(s => s.IsAgreed))
                .ForMember(d => d.Content,  opt => opt.MapFrom(s => s.Content));


            CreateMap<Payslip, PayslipDto>()
                .ForMember(d => d.EmployeeCode,   opt => opt.MapFrom(s => s.Employee.EmployeeCode))
                .ForMember(d => d.EmployeeName,   opt => opt.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Employee.Department.DepartmentName))
                .ForMember(d => d.PositionName,   opt => opt.MapFrom(s => s.Employee.Position.PositionName))
                .ForMember(d => d.Month,          opt => opt.MapFrom(s => s.Period.Month))
                .ForMember(d => d.Year,           opt => opt.MapFrom(s => s.Period.Year))
                // Tính Gross từ các thành phần (tránh dùng GrossPay nullable/stale trong DB)
                .ForMember(d => d.GrossPay, opt => opt.MapFrom(s =>
                    (s.PayrollRecord.WorkingDays > 0
                        ? Math.Round(s.PayrollRecord.BaseSalary / s.PayrollRecord.WorkingDays * s.PayrollRecord.ActualWorkingDays, 0)
                        : 0m)
                    + s.PayrollRecord.TotalAllowances
                    + s.PayrollRecord.OvertimePay
                    + s.PayrollRecord.BonusAmount))
                // Tổng khấu trừ = BH + Thuế + Khấu trừ thủ công
                .ForMember(d => d.TotalDeductions, opt => opt.MapFrom(s =>
                    s.PayrollRecord.InsuranceAmount
                    + s.PayrollRecord.TaxAmount
                    + s.PayrollRecord.PayrollDeductions
                        .Where(d => d.DeductionType == "Manual")
                        .Sum(d => d.Amount)))
                // NetPay = Gross - Tổng khấu trừ
                .ForMember(d => d.NetPay, opt => opt.MapFrom(s =>
                    (s.PayrollRecord.WorkingDays > 0
                        ? Math.Round(s.PayrollRecord.BaseSalary / s.PayrollRecord.WorkingDays * s.PayrollRecord.ActualWorkingDays, 0)
                        : 0m)
                    + s.PayrollRecord.TotalAllowances
                    + s.PayrollRecord.OvertimePay
                    + s.PayrollRecord.BonusAmount
                    - s.PayrollRecord.InsuranceAmount
                    - s.PayrollRecord.TaxAmount
                    - s.PayrollRecord.PayrollDeductions
                        .Where(d => d.DeductionType == "Manual")
                        .Sum(d => d.Amount)));
        }
    }
}
