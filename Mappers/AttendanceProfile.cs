using AutoMapper;
using HRManagement.Models;
using HRManagement.DTOs.Attendances;
using HRManagement.Services.Attendances;

namespace HRManagement.Mappers
{
    public class AttendanceProfile : Profile
    {
        public AttendanceProfile()
        {
            CreateMap<AttendanceLog, AttendanceLogResponseDto>();

            CreateMap<AttendanceRecord, AttendanceResponseDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => src.Shift != null ? src.Shift.ShiftName : null))
                .ForMember(dest => dest.ExplanationLeaveTypeName, opt => opt.MapFrom(src => src.ExplanationLeaveType != null ? src.ExplanationLeaveType.LeaveTypeName : null))
                .ForMember(dest => dest.WorkingHours, opt => opt.MapFrom(src =>
                    (src.ExplanationStatus == "Required" || src.ExplanationStatus == "Pending" || src.ExplanationStatus == "Rejected") ? 0 : src.WorkingHours))
                .ForMember(dest => dest.OvertimeHours, opt => opt.MapFrom(src =>
                    (src.ExplanationStatus == "Required" || src.ExplanationStatus == "Pending" || src.ExplanationStatus == "Rejected") ? 0 : src.OvertimeHours))
                .ForMember(dest => dest.ShiftStartTime, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftEndTime, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftIsOvernight, opt => opt.Ignore())
                .ForMember(dest => dest.AllowedCheckInFrom, opt => opt.Ignore())
                .ForMember(dest => dest.AllowedCheckInTo, opt => opt.Ignore())
                .ForMember(dest => dest.AllowedCheckOutFrom, opt => opt.Ignore())
                .ForMember(dest => dest.AllowedCheckOutTo, opt => opt.Ignore())
                .AfterMap((src, dest) => AttendanceHelper.ApplyShiftWindowMetadata(dest, src.Shift, src.AttendanceDate));
        }
    }
}
