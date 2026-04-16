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
                .ForMember(d => d.TotalEmployees, opt => opt.MapFrom(s => s.PayrollRecords.Count))
                .ForMember(d => d.TotalNetPay,    opt => opt.MapFrom(s => s.PayrollRecords.Sum(r => r.NetPay)))
                .ForMember(d => d.TotalGrossPay,  opt => opt.MapFrom(s => s.PayrollRecords.Sum(r => r.GrossPay)));

            CreateMap<PayrollRecord, PayrollRecordDto>()
                .ForMember(d => d.EmployeeCode,   opt => opt.MapFrom(s => s.Employee.EmployeeCode))
                .ForMember(d => d.EmployeeName,   opt => opt.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Employee.Department.DepartmentName))
                .ForMember(d => d.PositionName,   opt => opt.MapFrom(s => s.Employee.Position.PositionName))
                .ForMember(d => d.Allowances,     opt => opt.MapFrom(s => s.PayrollAllowances))
                .ForMember(d => d.Deductions,     opt => opt.MapFrom(s => s.PayrollDeductions));

            CreateMap<PayrollAllowance, PayrollAllowanceDto>();
            CreateMap<PayrollDeduction, PayrollDeductionDto>();

            CreateMap<Payslip, PayslipDto>()
                .ForMember(d => d.EmployeeCode,   opt => opt.MapFrom(s => s.Employee.EmployeeCode))
                .ForMember(d => d.EmployeeName,   opt => opt.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Employee.Department.DepartmentName))
                .ForMember(d => d.PositionName,   opt => opt.MapFrom(s => s.Employee.Position.PositionName))
                .ForMember(d => d.Month,          opt => opt.MapFrom(s => s.Period.Month))
                .ForMember(d => d.Year,           opt => opt.MapFrom(s => s.Period.Year))
                .ForMember(d => d.GrossPay,       opt => opt.MapFrom(s => s.PayrollRecord.GrossPay))
                .ForMember(d => d.TotalDeductions,opt => opt.MapFrom(s => s.PayrollRecord.TotalDeductions))
                .ForMember(d => d.NetPay,         opt => opt.MapFrom(s => s.PayrollRecord.NetPay));
        }
    }
}
