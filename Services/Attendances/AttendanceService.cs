using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services.FaceVerifications;

namespace HRManagement.Services.Attendances
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IFaceVerificationService _faceVerificationService;
        public AttendanceService(IAttendanceRepository attendanceRepository, IFaceVerificationService faceVerificationService)
        {
            _attendanceRepository = attendanceRepository;
            _faceVerificationService = faceVerificationService;
        }
        public async Task<AttendanceResponseDto> CheckInAsync(int employeeId, CheckInRequestDto dto)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            var assignment = await _attendanceRepository.GetActiveShiftAssignmentAsync(employeeId, today);
            if (assignment == null)
                throw new InvalidOperationException("Bạn chưa được phân ca làm việc hôm nay.");

            var shift = assignment.Shift;
            if (shift == null)
                throw new InvalidOperationException("Không tìm thấy ca làm việc.");

            var attendance = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(employeeId, today);

            if (attendance != null && (attendance.IsLocked ?? false))
                throw new InvalidOperationException("Bản ghi chấm công đã bị khóa.");

            if (attendance != null && attendance.CheckInTime.HasValue)
                throw new InvalidOperationException("Bạn đã check-in hôm nay rồi.");

            var shiftStart = today.ToDateTime(shift.StartTime);
            var earlyCheckInMinutes = shift.EarlyCheckInMinutes ?? 30;
            var latestCheckInMinutes = shift.LatestCheckInMinutes ?? 120;
            var lateGraceMinutes = shift.LateGraceMinutes ?? 5;

            var earliestCheckIn = shiftStart.AddMinutes(-earlyCheckInMinutes);
            var latestCheckIn = shiftStart.AddMinutes(latestCheckInMinutes);

            if (now < earliestCheckIn)
                throw new InvalidOperationException($"Chưa đến thời gian check-in. Bạn chỉ được check-in từ {earliestCheckIn:HH:mm}.");

            if (now > latestCheckIn)
                throw new InvalidOperationException("Đã quá thời gian check-in cho phép.");

            if (string.IsNullOrWhiteSpace(dto.FaceImageBase64))
                throw new ArgumentException("Vui lòng chụp ảnh khuôn mặt để check-in.");

            // STEP 1: Verify face trước
            var faceResult = await _faceVerificationService.VerifyAsync(
                employeeId,
                dto.FaceImageBase64,
                "CheckIn",
                dto.DeviceInfo,
                dto.IpAddress,
                dto.Location
            );

            if (!faceResult.IsMatch)
                throw new InvalidOperationException($"Xác minh khuôn mặt thất bại. {faceResult.FailureReason}");

            int lateMinutes = 0;
            var lateThreshold = shiftStart.AddMinutes(lateGraceMinutes);

            if (now > lateThreshold)
            {
                lateMinutes = (int)Math.Floor((now - shiftStart).TotalMinutes);
            }

            var log = new AttendanceLog
            {
                EmployeeId = employeeId,
                ShiftId = shift.ShiftId,
                LogTime = now,
                LogType = "CheckIn",
                Source = "Web",
                DeviceInfo = dto.DeviceInfo,
                IpAddress = dto.IpAddress,
                Location = dto.Location,
                Remarks = dto.Remarks,
                IsValid = true,
                VerificationMethod = "Face",
                VerificationStatus = "Verified",
                CreatedDate = now,
                CreatedBy = employeeId
            };

            await _attendanceRepository.AddAttendanceLogAsync(log);

            if (attendance == null)
            {
                attendance = new AttendanceRecord
                {
                    EmployeeId = employeeId,
                    AttendanceDate = today,
                    ShiftId = shift.ShiftId,
                    CheckInTime = now,
                    CheckOutTime = null,
                    WorkingHours = null,
                    OvertimeHours = 0,
                    LateMinutes = lateMinutes,
                    EarlyLeaveMinutes = 0,
                    Status = lateMinutes > 0 ? "Late" : "Present",
                    Source = "Web",
                    IsManualAdjusted = false,
                    IsLocked = false,
                    ApprovedBy = null,
                    ApprovedDate = null,
                    Location = dto.Location,
                    Remarks = dto.Remarks,
                    CreatedDate = now,
                    ModifiedDate = null,
                    ModifiedBy = null,
                    CheckInVerificationMethod = "Face",
                    CheckInVerified = true

                };

                await _attendanceRepository.AddAttendanceAsync(attendance);
            }
            else
            {
                attendance.ShiftId = shift.ShiftId;
                attendance.CheckInTime = now;
                attendance.LateMinutes = lateMinutes;
                attendance.Status = lateMinutes > 0 ? "Late" : "Present";
                attendance.Source = "Web";
                attendance.Location = dto.Location;
                attendance.Remarks = dto.Remarks;
                attendance.ModifiedDate = now;
                attendance.ModifiedBy = employeeId;
                attendance.CheckInVerificationMethod = "Face";
                attendance.CheckInVerified = true;

                await _attendanceRepository.UpdateAttendanceAsync(attendance);
            }

            await _attendanceRepository.SaveChangesAsync();

            var result = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(employeeId, today)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công sau khi check-in.");

            return MapAttendance(result);
        }

        public async Task<AttendanceResponseDto> CheckOutAsync(int employeeId, CheckOutRequestDto dto)
        {
            var now = DateTime.Now;

            // Ưu tiên lấy bản ghi đang mở (đã check-in nhưng chưa check-out)
            var attendance = await _attendanceRepository.GetOpenAttendanceRecordAsync(employeeId);

            if (attendance == null)
                throw new InvalidOperationException("Bạn chưa check-in hoặc đã check-out rồi.");

            if (attendance.IsLocked ?? false)
                throw new InvalidOperationException("Bản ghi chấm công đã bị khóa.");

            if (!attendance.CheckInTime.HasValue)
                throw new InvalidOperationException("Bạn chưa check-in hôm nay.");

            if (attendance.CheckOutTime.HasValue)
                throw new InvalidOperationException("Bạn đã check-out rồi.");

            if (string.IsNullOrWhiteSpace(dto.FaceImageBase64))
                throw new ArgumentException("Vui lòng chụp ảnh khuôn mặt để check-out.");

            Shift? shift = null;
            if (attendance.ShiftId.HasValue)
            {
                shift = await _attendanceRepository.GetShiftByIdAsync(attendance.ShiftId.Value);
            }

            // VALIDATE khung giờ check-out
            if (shift != null)
            {
                var attendanceDate = attendance.AttendanceDate;
                var shiftEnd = attendanceDate.ToDateTime(shift.EndTime);

                if (shift.IsOvernight ?? false)
                {
                    shiftEnd = shiftEnd.AddDays(1);
                }

                // Cho phép checkout sớm tối đa 2 tiếng trước giờ kết thúc ca
                var earliestCheckOut = shiftEnd.AddMinutes(-120);

                // Cho phép checkout muộn tối đa X phút sau giờ kết thúc ca
                var latestCheckOutMinutes = shift.LatestCheckOutMinutes ?? 240;
                var latestCheckOut = shiftEnd.AddMinutes(latestCheckOutMinutes);

                if (now < earliestCheckOut)
                    throw new InvalidOperationException($"Chưa đến thời gian check-out. Bạn chỉ được check-out từ {earliestCheckOut:HH:mm}.");

                if (now > latestCheckOut)
                    throw new InvalidOperationException("Đã quá thời gian check-out cho phép.");
            }

            // STEP 1: Verify face trước
            var faceResult = await _faceVerificationService.VerifyAsync(
                employeeId,
                dto.FaceImageBase64,
                "CheckOut",
                dto.DeviceInfo,
                dto.IpAddress,
                dto.Location
            );

            if (!faceResult.IsMatch)
                throw new InvalidOperationException($"Xác minh khuôn mặt thất bại. {faceResult.FailureReason}");

            var log = new AttendanceLog
            {
                EmployeeId = employeeId,
                ShiftId = attendance.ShiftId,
                LogTime = now,
                LogType = "CheckOut",
                Source = "Web",
                DeviceInfo = dto.DeviceInfo,
                IpAddress = dto.IpAddress,
                Location = dto.Location,
                Remarks = dto.Remarks,
                IsValid = true,
                VerificationMethod = "Face",
                VerificationStatus = "Verified",
                CreatedDate = now,
                CreatedBy = employeeId
            };

            await _attendanceRepository.AddAttendanceLogAsync(log);

            attendance.CheckOutTime = now;
            attendance.Location = dto.Location ?? attendance.Location;
            attendance.Remarks = dto.Remarks ?? attendance.Remarks;
            attendance.ModifiedDate = now;
            attendance.ModifiedBy = employeeId;
            attendance.CheckOutVerificationMethod = "Face";
            attendance.CheckOutVerified = true;

            var workingHours = (decimal)(now - attendance.CheckInTime.Value).TotalHours;
            attendance.WorkingHours = Math.Round(workingHours, 2);

            if (shift != null)
            {
                var attendanceDate = attendance.AttendanceDate;
                var shiftEnd = attendanceDate.ToDateTime(shift.EndTime);

                if (shift.IsOvernight ?? false)
                {
                    shiftEnd = shiftEnd.AddDays(1);
                }

                if (now < shiftEnd)
                {
                    attendance.EarlyLeaveMinutes = (int)Math.Floor((shiftEnd - now).TotalMinutes);
                    attendance.OvertimeHours = 0;
                }
                else
                {
                    attendance.EarlyLeaveMinutes = 0;
                    var overtime = (decimal)(now - shiftEnd).TotalHours;
                    attendance.OvertimeHours = overtime > 0 ? Math.Round(overtime, 2) : 0;
                }
            }

            // FIX STATUS
            if (!attendance.CheckInTime.HasValue || !attendance.CheckOutTime.HasValue)
            {
                attendance.Status = "Incomplete";
            }
            else if ((attendance.LateMinutes ?? 0) > 0 && (attendance.EarlyLeaveMinutes ?? 0) > 0)
            {
                attendance.Status = "LateEarlyLeave";
            }
            else if ((attendance.LateMinutes ?? 0) > 0)
            {
                attendance.Status = "Late";
            }
            else if ((attendance.EarlyLeaveMinutes ?? 0) > 0)
            {
                attendance.Status = "EarlyLeave";
            }
            else
            {
                attendance.Status = "Present";
            }

            attendance.Source = "Web-Face";

            await _attendanceRepository.UpdateAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();

            var result = await _attendanceRepository.GetAttendanceByIdAsync(attendance.AttendanceId)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công sau khi check-out.");

            return MapAttendance(result);
        }

        public async Task<AttendanceDetailResponseDto?> GetMyTodayAsync(int employeeId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var attendance = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(employeeId, today);

            if (attendance == null)
                return null;

            var logs = await _attendanceRepository.GetLogsByEmployeeAndDateAsync(employeeId, today);

            return new AttendanceDetailResponseDto
            {
                Attendance = MapAttendance(attendance),
                Logs = logs.Select(MapLog).ToList()
            };
        }

        public async Task<List<AttendanceResponseDto>> GetMyHistoryAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate)
        {
            var records = await _attendanceRepository.SearchAttendanceAsync(fromDate, toDate, employeeId, null);
            return records.Select(MapAttendance).ToList();
        }

        public async Task<List<AttendanceResponseDto>> GetAttendanceByDateAsync(DateOnly date)
        {
            var records = await _attendanceRepository.GetAttendanceByDateAsync(date);
            return records.Select(MapAttendance).ToList();
        }

        public async Task<List<AttendanceResponseDto>> SearchAttendanceAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status)
        {
            var records = await _attendanceRepository.SearchAttendanceAsync(fromDate, toDate, employeeId, status);
            return records.Select(MapAttendance).ToList();
        }

        public async Task<AttendanceDetailResponseDto?> GetAttendanceDetailAsync(int employeeId, DateOnly date)
        {
            var attendance = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(employeeId, date);
            if (attendance == null)
                return null;

            var logs = await _attendanceRepository.GetLogsByEmployeeAndDateAsync(employeeId, date);

            return new AttendanceDetailResponseDto
            {
                Attendance = MapAttendance(attendance),
                Logs = logs.Select(MapLog).ToList()
            };
        }

        public async Task<AttendanceResponseDto> ManualAdjustAttendanceAsync(int attendanceId, int approverId, ManualAdjustAttendanceDto dto)
        {
            var attendance = await _attendanceRepository.GetAttendanceByIdAsync(attendanceId)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công.");

            if (attendance.IsLocked ?? false)
                throw new InvalidOperationException("Bản ghi chấm công đã bị khóa.");

            attendance.CheckInTime = dto.CheckInTime;
            attendance.CheckOutTime = dto.CheckOutTime;
            attendance.Status = dto.Status;
            attendance.IsManualAdjusted = true;
            attendance.Source = "Manual";
            attendance.Remarks = dto.Remarks;
            attendance.ApprovedBy = approverId;
            attendance.ApprovedDate = DateTime.Now;
            attendance.ModifiedDate = DateTime.Now;
            attendance.ModifiedBy = approverId;

            if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
            {
                var hours = (decimal)(attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
                attendance.WorkingHours = Math.Round(hours, 2);
            }
            else
            {
                attendance.WorkingHours = null;
            }

            await _attendanceRepository.UpdateAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();

            return MapAttendance(attendance);
        }

        public async Task<AttendanceResponseDto> ManualCreateAttendanceAsync(int approverId, ManualCreateAttendanceDto dto)
        {
            var existing = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(dto.EmployeeId, dto.AttendanceDate);
            if (existing != null)
                throw new InvalidOperationException("Nhân viên đã có bản ghi chấm công trong ngày này.");

            decimal? workingHours = null;
            if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
            {
                workingHours = Math.Round((decimal)(dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours, 2);
            }

            var attendance = new AttendanceRecord
            {
                EmployeeId = dto.EmployeeId,
                AttendanceDate = dto.AttendanceDate,
                ShiftId = dto.ShiftId,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                WorkingHours = workingHours,
                OvertimeHours = 0,
                LateMinutes = 0,
                EarlyLeaveMinutes = 0,
                Status = dto.Status,
                Source = "Manual",
                IsManualAdjusted = true,
                IsLocked = false,
                ApprovedBy = approverId,
                ApprovedDate = DateTime.Now,
                Remarks = dto.Remarks,
                CreatedDate = DateTime.Now,
                ModifiedDate = null,
                ModifiedBy = null
            };

            await _attendanceRepository.AddAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();

            var created = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(dto.EmployeeId, dto.AttendanceDate)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi sau khi tạo.");

            return MapAttendance(created);
        }

        public async System.Threading.Tasks.Task LockAttendanceAsync(int attendanceId, int userId)
        {
            var attendance = await _attendanceRepository.GetAttendanceByIdAsync(attendanceId)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công.");

            attendance.IsLocked = true;
            attendance.ModifiedDate = DateTime.Now;
            attendance.ModifiedBy = userId;

            await _attendanceRepository.UpdateAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UnlockAttendanceAsync(int attendanceId, int userId)
        {
            var attendance = await _attendanceRepository.GetAttendanceByIdAsync(attendanceId)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công.");

            attendance.IsLocked = false;
            attendance.ModifiedDate = DateTime.Now;
            attendance.ModifiedBy = userId;

            await _attendanceRepository.UpdateAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();
        }

        public async Task<List<AttendanceLogResponseDto>> GetLogsAsync(int employeeId, DateOnly date)
        {
            var logs = await _attendanceRepository.GetLogsByEmployeeAndDateAsync(employeeId, date);
            return logs.Select(MapLog).ToList();
        }

        

        private static AttendanceResponseDto MapAttendance(AttendanceRecord a)
        {
            return new AttendanceResponseDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee?.FullName ?? string.Empty,
                AttendanceDate = a.AttendanceDate,
                ShiftId = a.ShiftId,
                ShiftName = a.Shift?.ShiftName,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                WorkingHours = a.WorkingHours,
                OvertimeHours = a.OvertimeHours,
                LateMinutes = a.LateMinutes,
                EarlyLeaveMinutes = a.EarlyLeaveMinutes,
                Status = a.Status,
                Source = a.Source,
                IsManualAdjusted = a.IsManualAdjusted,
                IsLocked = a.IsLocked,
                Location = a.Location,
                Remarks = a.Remarks
            };
        }

        private static AttendanceLogResponseDto MapLog(AttendanceLog l)
        {
            return new AttendanceLogResponseDto
            {
                LogId = l.LogId,
                EmployeeId = l.EmployeeId,
                ShiftId = l.ShiftId,
                LogTime = l.LogTime,
                LogType = l.LogType,
                Source = l.Source,
                DeviceInfo = l.DeviceInfo,
                IpAddress = l.IpAddress,
                Location = l.Location,
                Remarks = l.Remarks
            };
        }

    }
}
