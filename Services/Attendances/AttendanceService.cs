using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services.FaceVerifications;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services.Attendances
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IFaceVerificationService _faceVerificationService;
        private readonly HrmsDbContext _context;

        public AttendanceService(IAttendanceRepository attendanceRepository, IFaceVerificationService faceVerificationService, HrmsDbContext context)
        {
            _attendanceRepository = attendanceRepository;
            _faceVerificationService = faceVerificationService;
            _context = context;
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

            // Fetch approved OT to expand time windows
            var approvedOts = await _context.OvertimeRequests
                .Where(o => o.EmployeeId == employeeId && o.OvertimeDate == today && o.Status == "Approved")
                .ToListAsync();

            var earliestCheckIn = shiftStart.AddMinutes(-earlyCheckInMinutes);
            var latestCheckIn = shiftStart.AddMinutes(latestCheckInMinutes);

            // Expansion: If there's an OT starting BEFORE the shift, expand the earliestCheckIn
            if (approvedOts.Any())
            {
                var minOtStart = approvedOts.Min(o => today.ToDateTime(o.StartTime));
                if (minOtStart < shiftStart)
                {
                    // Earliest allowed is now based on OT start minus grace period
                    earliestCheckIn = minOtStart.AddMinutes(-earlyCheckInMinutes);
                }
            }

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
                bool validLoc = await IsValidLocation(dto.Latitude, dto.Longitude);
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
                    ExplanationStatus = validLoc ? null : "Required",
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
                bool validLoc = await IsValidLocation(dto.Latitude, dto.Longitude);
                attendance.ShiftId = shift.ShiftId;
                attendance.CheckInTime = now;
                attendance.LateMinutes = lateMinutes;
                attendance.Status = lateMinutes > 0 ? "Late" : "Present";
                if (!validLoc) attendance.ExplanationStatus = "Required";
                attendance.Source = "Web";
                attendance.Location = dto.Location;
                attendance.Remarks = dto.Remarks;
                attendance.ModifiedDate = now;
                attendance.ModifiedBy = employeeId;
                attendance.CheckInVerificationMethod = "Face";
                attendance.CheckInVerified = true;

                await _attendanceRepository.UpdateAttendanceAsync(attendance);
            }

            try
            {
                await _attendanceRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Concurrency handling: If two check-ins happen at same time, 
                // one might fail due to UNIQUE KEY constraint.
                // We fetch the existing one and use it.
                var existing = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(employeeId, today);
                if (existing != null)
                {
                    return MapAttendance(existing);
                }
                throw; // Rethrow if not a duplicate key issue
            }

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

                // Fetch approved OT to expand time windows
                var approvedOts = await _context.OvertimeRequests
                    .Where(o => o.EmployeeId == employeeId && o.OvertimeDate == attendanceDate && o.Status == "Approved")
                    .ToListAsync();

                // Cho phép checkout sớm tối đa 2 tiếng trước giờ kết thúc ca (Giữ nguyên kỷ luật ca chính)
                var earliestCheckOut = shiftEnd.AddMinutes(-120);

                // Cho phép checkout muộn tối đa X phút sau giờ kết thúc ca
                var latestCheckOutMinutes = shift.LatestCheckOutMinutes ?? 240;
                var latestCheckOut = shiftEnd.AddMinutes(latestCheckOutMinutes);

                // Expansion: Nếu có OT kết thúc SAU ca, mở rộng mốc checkout muộn
                if (approvedOts.Any())
                {
                    var maxOtEnd = approvedOts.Max(o => {
                        var end = attendanceDate.ToDateTime(o.EndTime);
                        if (o.EndTime <= o.StartTime) end = end.AddDays(1);
                        return end;
                    });

                    if (maxOtEnd > shiftEnd)
                    {
                        latestCheckOut = maxOtEnd.AddMinutes(latestCheckOutMinutes);
                    }
                }

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

            bool validLocOut = await IsValidLocation(dto.Latitude, dto.Longitude);
            if (!validLocOut) attendance.ExplanationStatus = "Required";

            var workingHours = (decimal)(now - attendance.CheckInTime.Value).TotalHours;
            attendance.WorkingHours = Math.Round(Math.Min(workingHours, 9999m), 2);

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
                    attendance.OvertimeHours = overtime > 0 ? Math.Round(Math.Min(overtime, 9999m), 2) : 0;
                }
            }

            // FIX STATUS
            if (!attendance.CheckInTime.HasValue || !attendance.CheckOutTime.HasValue)
            {
                attendance.Status = "Incomplete";
                attendance.ExplanationStatus = "Required";
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

            // Entity đã được track bởi EF Core từ GetOpenAttendanceRecordAsync,
            // không cần gọi Update() vì sẽ mark cả navigation properties → lỗi.
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
            return await SearchAttendanceWithLeavesAsync(fromDate, toDate, employeeId, null);
        }

        public async Task<List<AttendanceResponseDto>> GetAttendanceByDateAsync(DateOnly date)
        {
            // Chuyển về gọi hàm search để dùng chung logic map nghỉ phép
            return await SearchAttendanceWithLeavesAsync(date, date, null, null);
        }

        public async Task<List<AttendanceResponseDto>> SearchAttendanceAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status)
        {
            return await SearchAttendanceWithLeavesAsync(fromDate, toDate, employeeId, status);
        }

        private async Task<List<AttendanceResponseDto>> SearchAttendanceWithLeavesAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status)
        {
            // Default range nếu thiếu
            var finalFrom = fromDate ?? DateOnly.FromDateTime(DateTime.Now).AddDays(-30);
            var finalTo = toDate ?? DateOnly.FromDateTime(DateTime.Now);

            var records = await _attendanceRepository.SearchAttendanceAsync(fromDate, toDate, employeeId, status);
            var result = records.Select(MapAttendance).ToList();

            // Fetch approved Overtime Requests
            var approvedOts = await _context.OvertimeRequests
                .Where(or => or.Status == "Approved" && 
                             or.OvertimeDate >= finalFrom && 
                             or.OvertimeDate <= finalTo &&
                             (employeeId == null || or.EmployeeId == employeeId))
                .ToListAsync();

            // Nếu lọc theo status cụ thể mà không phải nghỉ phép thì không cần map thêm 
            // Tuy nhiên OT thì luôn cần map vào kết quả attendance hiện tại
            
            // Xây dựng query cho LeaveRequests
            var leaves = await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Employee)
                .Where(lr => lr.EmployeeId == (employeeId ?? lr.EmployeeId) &&
                             lr.Status == "Approved" &&
                             lr.StartDate <= finalTo &&
                             lr.EndDate >= finalFrom)
                .ToListAsync();

            // Lấy phân ca để hỗ trợ mapping
            var assignments = await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .Where(sa => sa.EmployeeId == (employeeId ?? sa.EmployeeId) &&
                             sa.Status == "Active" &&
                             sa.AssignmentDate >= finalFrom &&
                             sa.AssignmentDate <= finalTo)
                .ToListAsync();

            var attendanceMap = result
                .GroupBy(r => (EmployeeId: r.EmployeeId, Date: r.AttendanceDate))
                .ToDictionary(g => g.Key, g => g.First());

            // 1. Map Leave Requests (existing logic)
            if (leaves.Any())
            {
                foreach (var leave in leaves)
                {
                    var start = leave.StartDate < finalFrom ? finalFrom : leave.StartDate;
                    var end = leave.EndDate > finalTo ? finalTo : leave.EndDate;

                    for (var date = start; date <= end; date = date.AddDays(1))
                    {
                        var key = (EmployeeId: leave.EmployeeId, Date: date);
                        
                        if (!attendanceMap.ContainsKey(key) || 
                            attendanceMap[key].Status == "Absent" || 
                            attendanceMap[key].Status == "Vắng mặt")
                        {
                            var assignment = assignments.FirstOrDefault(a => a.EmployeeId == leave.EmployeeId && a.AssignmentDate == date);
                            
                            var leaveRecord = new AttendanceResponseDto
                            {
                                AttendanceId = 0,
                                EmployeeId = leave.EmployeeId,
                                EmployeeName = leave.Employee?.FullName ?? (attendanceMap.ContainsKey(key) ? attendanceMap[key].EmployeeName : "Unknown"),
                                AttendanceDate = date,
                                ShiftId = assignment?.ShiftId,
                                ShiftName = assignment?.Shift?.ShiftName ?? "No Shift",
                                CheckInTime = null,
                                CheckOutTime = null,
                                WorkingHours = leave.LeaveType.IsPaid ? (assignment?.Shift?.WorkingHours ?? 8) : 0,
                                OvertimeHours = 0,
                                LateMinutes = 0,
                                EarlyLeaveMinutes = 0,
                                Status = leave.LeaveType.IsPaid ? "PaidLeave" : "UnpaidLeave",
                                Source = "LeaveRequest",
                                Remarks = leave.LeaveType.LeaveTypeName + (assignment == null ? " (Mất phân ca)" : "") + ": " + leave.Reason
                            };

                            if (attendanceMap.ContainsKey(key))
                            {
                                var oldIndex = result.IndexOf(attendanceMap[key]);
                                if (oldIndex != -1) result[oldIndex] = leaveRecord;
                            }
                            else
                            {
                                result.Add(leaveRecord);
                            }
                            attendanceMap[key] = leaveRecord;
                        }
                    }
                }
            }

            // 2. Map Overtime Sync (Phase 3 logic)
            foreach (var attendance in result)
            {
                var dayOts = approvedOts.Where(o => o.EmployeeId == attendance.EmployeeId && o.OvertimeDate == attendance.AttendanceDate).ToList();
                if (!dayOts.Any() && !attendance.CheckInTime.HasValue) continue;

                var assignment = assignments.FirstOrDefault(a => a.EmployeeId == attendance.EmployeeId && a.AssignmentDate == attendance.AttendanceDate);
                
                decimal approvedHours = dayOts.Sum(o => o.TotalHours);
                decimal actualOtHours = 0;
                decimal payrollOtHours = 0;

                if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                {
                    var checkIn = attendance.CheckInTime.Value;
                    var checkOut = attendance.CheckOutTime.Value;

                    if (assignment?.Shift != null)
                    {
                        var shift = assignment.Shift;
                        var baseDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue);
                        var shiftStart = baseDate.Add(shift.StartTime.ToTimeSpan());
                        var shiftEnd = baseDate.Add(shift.EndTime.ToTimeSpan());
                        if (shift.IsOvernight == true) shiftEnd = shiftEnd.AddDays(1);

                        // Actual OT is time worked outside shift range
                        // Case 1: Before Shift
                        if (checkIn < shiftStart)
                        {
                            var earlyEnd = checkOut < shiftStart ? checkOut : shiftStart;
                            actualOtHours += (decimal)(earlyEnd - checkIn).TotalHours;
                        }
                        // Case 2: After Shift
                        if (checkOut > shiftEnd)
                        {
                            var lateStart = checkIn > shiftEnd ? checkIn : shiftEnd;
                            actualOtHours += (decimal)(checkOut - lateStart).TotalHours;
                        }

                        // Payroll OT = Intersection of (Actual OT Range) AND (Approved OT Range)
                        foreach (var ot in dayOts)
                        {
                            var otStart = baseDate.Add(ot.StartTime.ToTimeSpan());
                            var otEnd = baseDate.Add(ot.EndTime.ToTimeSpan());
                            if (otEnd <= otStart) otEnd = otEnd.AddDays(1);

                            // Intersection with worked range outside shift
                            // Before shift intersection
                            var bStart = MaxDateTime(checkIn, otStart);
                            var bEnd = MinDateTime(shiftStart, MinDateTime(checkOut, otEnd));
                            if (bEnd > bStart) payrollOtHours += (decimal)(bEnd - bStart).TotalHours;

                            // After shift intersection
                            var aStart = MaxDateTime(shiftEnd, MaxDateTime(checkIn, otStart));
                            var aEnd = MinDateTime(checkOut, otEnd);
                            if (aEnd > aStart) payrollOtHours += (decimal)(aEnd - aStart).TotalHours;
                        }
                    }
                    else // Day Off
                    {
                        actualOtHours = (decimal)(checkOut - checkIn).TotalHours;
                        
                        foreach (var ot in dayOts)
                        {
                            var baseDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue);
                            var otStart = baseDate.Add(ot.StartTime.ToTimeSpan());
                            var otEnd = baseDate.Add(ot.EndTime.ToTimeSpan());
                            if (otEnd <= otStart) otEnd = otEnd.AddDays(1);

                            var overlapStart = MaxDateTime(checkIn, otStart);
                            var overlapEnd = MinDateTime(checkOut, otEnd);
                            if (overlapEnd > overlapStart) payrollOtHours += (decimal)(overlapEnd - overlapStart).TotalHours;
                        }
                    }
                }

                attendance.ActualOvertimeHours = Math.Round(actualOtHours, 2);
                attendance.ApprovedOvertimeHours = Math.Round(approvedHours, 2);
                attendance.PayrollOvertimeHours = Math.Round(payrollOtHours, 2);
                attendance.OvertimeHours = attendance.PayrollOvertimeHours; // Standard view uses Payroll OT
            }

            // 3. Generate virtual "Absent" records for shift days with no check-in and no leave
            if (status == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                foreach (var assignment in assignments)
                {
                    if (assignment.AssignmentDate >= today) continue; // skip today and future

                    var key = (EmployeeId: assignment.EmployeeId, Date: assignment.AssignmentDate);
                    if (!attendanceMap.ContainsKey(key))
                    {
                        var empName = await _context.Employees
                            .Where(e => e.EmployeeId == assignment.EmployeeId)
                            .Select(e => e.FullName)
                            .FirstOrDefaultAsync() ?? "Unknown";

                        var absentRecord = new AttendanceResponseDto
                        {
                            AttendanceId = 0,
                            EmployeeId = assignment.EmployeeId,
                            EmployeeName = empName,
                            AttendanceDate = assignment.AssignmentDate,
                            ShiftId = assignment.ShiftId,
                            ShiftName = assignment.Shift?.ShiftName ?? "No Shift",
                            CheckInTime = null,
                            CheckOutTime = null,
                            WorkingHours = 0,
                            OvertimeHours = 0,
                            LateMinutes = 0,
                            EarlyLeaveMinutes = 0,
                            Status = "Absent",
                            Source = "System",
                            Remarks = null
                        };

                        result.Add(absentRecord);
                        attendanceMap[key] = absentRecord;
                    }
                }
            }

            return result.OrderByDescending(r => r.AttendanceDate).ThenBy(r => r.EmployeeName).ToList();
        }

        private static DateTime MaxDateTime(DateTime a, DateTime b) => a > b ? a : b;
        private static DateTime MinDateTime(DateTime a, DateTime b) => a < b ? a : b;

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
            attendance.Source = dto.Source ?? "Manual";
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
                OvertimeHours = 0, // This will be dynamically synced in history view
                LateMinutes = 0,
                EarlyLeaveMinutes = 0,
                Status = dto.Status,
                Source = dto.Source ?? "Manual",
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

        

        public async Task<AttendanceResponseDto> SubmitExplanationAsync(int employeeId, int attendanceId, string message)
        {
            var attendance = await _attendanceRepository.GetAttendanceByIdAsync(attendanceId)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công.");

            if (attendance.EmployeeId != employeeId)
                throw new InvalidOperationException("Bạn không có quyền giải trình cho bản ghi này.");
                
            if (attendance.IsLocked == true)
                throw new InvalidOperationException("Bản ghi chấm công đã bị khóa.");

            attendance.ExplanationMessage = message;
            attendance.ExplanationStatus = "Pending";
            attendance.ModifiedDate = DateTime.Now;
            attendance.ModifiedBy = employeeId;

            await _attendanceRepository.UpdateAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();

            return MapAttendance(attendance);
        }

        public async Task<AttendanceResponseDto> SubmitAbsentExplanationAsync(int employeeId, DateOnly date, string message)
        {
            // Verify if there was a shift assignment
            var assignment = await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .FirstOrDefaultAsync(sa => sa.EmployeeId == employeeId && sa.AssignmentDate == date && sa.Status == "Active");

            if (assignment == null)
            {
                throw new InvalidOperationException("Bạn không có ca làm việc vào ngày này để giải trình.");
            }

            // Check if there is already an attendance record
            var attendance = await _attendanceRepository.GetAttendanceByEmployeeAndDateAsync(employeeId, date);
            if (attendance != null)
            {
                if (attendance.IsLocked == true)
                    throw new InvalidOperationException("Bản ghi chấm công đã bị khóa.");

                attendance.ExplanationMessage = message;
                attendance.ExplanationStatus = "Pending";
                attendance.ModifiedDate = DateTime.Now;
                attendance.ModifiedBy = employeeId;
                await _attendanceRepository.UpdateAttendanceAsync(attendance);
            }
            else
            {
                attendance = new AttendanceRecord
                {
                    EmployeeId = employeeId,
                    AttendanceDate = date,
                    ShiftId = assignment.ShiftId,
                    CheckInTime = null,
                    CheckOutTime = null,
                    WorkingHours = 0,
                    OvertimeHours = 0,
                    LateMinutes = 0,
                    EarlyLeaveMinutes = 0,
                    Status = "Absent",
                    Source = "System",
                    IsManualAdjusted = false,
                    IsLocked = false,
                    ExplanationMessage = message,
                    ExplanationStatus = "Pending",
                    CreatedDate = DateTime.Now
                };
                await _attendanceRepository.AddAttendanceAsync(attendance);
            }

            await _attendanceRepository.SaveChangesAsync();
            return MapAttendance(attendance);
        }

        public async Task<AttendanceResponseDto> ApproveExplanationAsync(int managerId, int attendanceId, ApproveExplanationDto dto)
        {
            var attendance = await _attendanceRepository.GetAttendanceByIdAsync(attendanceId)
                ?? throw new InvalidOperationException("Không tìm thấy bản ghi chấm công.");

            if (dto.IsApproved)
            {
                attendance.ExplanationStatus = "Approved";

                // If manager provided manual times to fix a missing check-in/out
                if (dto.ManualCheckInTime.HasValue)
                {
                    attendance.CheckInTime = attendance.AttendanceDate.ToDateTime(TimeOnly.FromTimeSpan(dto.ManualCheckInTime.Value));
                    attendance.IsManualAdjusted = true;
                }
                if (dto.ManualCheckOutTime.HasValue)
                {
                    attendance.CheckOutTime = attendance.AttendanceDate.ToDateTime(TimeOnly.FromTimeSpan(dto.ManualCheckOutTime.Value));
                    attendance.IsManualAdjusted = true;
                }

                if (dto.ManualCheckInTime.HasValue || dto.ManualCheckOutTime.HasValue)
                {
                    var lateMinutes = 0;
                    var earlyLeaveMinutes = 0;

                    if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                    {
                        var hours = (decimal)(attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
                        attendance.WorkingHours = Math.Round(hours, 2);

                        var shift = await _context.Shifts.FindAsync(attendance.ShiftId);
                        if (shift != null)
                        {
                            var shiftStart = attendance.AttendanceDate.ToDateTime(shift.StartTime);
                            var shiftEnd = attendance.AttendanceDate.ToDateTime(shift.EndTime);
                            if (shift.IsOvernight ?? false) shiftEnd = shiftEnd.AddDays(1);

                            if (attendance.CheckInTime.Value > shiftStart)
                            {
                                lateMinutes = (int)Math.Floor((attendance.CheckInTime.Value - shiftStart).TotalMinutes);
                            }
                            if (attendance.CheckOutTime.Value < shiftEnd)
                            {
                                earlyLeaveMinutes = (int)Math.Floor((shiftEnd - attendance.CheckOutTime.Value).TotalMinutes);
                            }
                        }

                        attendance.LateMinutes = lateMinutes;
                        attendance.EarlyLeaveMinutes = earlyLeaveMinutes;

                        if (lateMinutes > 0 && earlyLeaveMinutes > 0)
                            attendance.Status = "LateEarlyLeave";
                        else if (lateMinutes > 0)
                            attendance.Status = "Late";
                        else if (earlyLeaveMinutes > 0)
                            attendance.Status = "EarlyLeave";
                        else
                            attendance.Status = "Present";
                    }
                    else
                    {
                        attendance.WorkingHours = null;
                        if (!attendance.CheckInTime.HasValue && !attendance.CheckOutTime.HasValue)
                            attendance.Status = "Absent";
                        else
                            attendance.Status = "Incomplete";
                    }
                }
            }
            else
            {
                attendance.ExplanationStatus = "Rejected";
            }

            attendance.ExplanationResponse = dto.Response;
            attendance.ApprovedBy = managerId;
            attendance.ApprovedDate = DateTime.Now;
            attendance.ModifiedDate = DateTime.Now;
            attendance.ModifiedBy = managerId;

            await _attendanceRepository.UpdateAttendanceAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();

            return MapAttendance(attendance);
        }

        private static AttendanceResponseDto MapAttendance(AttendanceRecord a)
        {
            // NẾU LUỒNG GIẢI TRÌNH CHƯA HOÀN THẤT (Cần giải trình, Đang chờ duyệt, Bị từ chối) -> Không trả về WorkingHours
            bool isExplanationUnresolved = a.ExplanationStatus == "Required" || a.ExplanationStatus == "Pending" || a.ExplanationStatus == "Rejected";
            
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
                WorkingHours = isExplanationUnresolved ? 0 : a.WorkingHours,
                OvertimeHours = isExplanationUnresolved ? 0 : a.OvertimeHours,
                LateMinutes = a.LateMinutes,
                EarlyLeaveMinutes = a.EarlyLeaveMinutes,
                Status = a.Status,
                Source = a.Source,
                IsManualAdjusted = a.IsManualAdjusted,
                IsLocked = a.IsLocked,
                Location = a.Location,
                Remarks = a.Remarks,
                ExplanationMessage = a.ExplanationMessage,
                ExplanationStatus = a.ExplanationStatus,
                ExplanationResponse = a.ExplanationResponse
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

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // metres
            var p1 = lat1 * Math.PI / 180;
            var p2 = lat2 * Math.PI / 180;
            var dp = (lat2 - lat1) * Math.PI / 180;
            var dl = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dp / 2) * Math.Sin(dp / 2) +
                    Math.Cos(p1) * Math.Cos(p2) *
                    Math.Sin(dl / 2) * Math.Sin(dl / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; 
        }

        private async Task<bool> IsValidLocation(double? lat, double? lon)
        {
            if (!lat.HasValue || !lon.HasValue) return false;

            var companyLatStr = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "CompanyLat");
            var companyLngStr = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "CompanyLng");
            var companyRadiusStr = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "CompanyRadius");

            if (companyLatStr == null || companyLngStr == null) return true; // Default to true if not configured

            if (double.TryParse(companyLatStr.SettingValue, System.Globalization.CultureInfo.InvariantCulture, out double cLat) && 
                double.TryParse(companyLngStr.SettingValue, System.Globalization.CultureInfo.InvariantCulture, out double cLng))
            {
                double radius = 500;
                if (companyRadiusStr != null && double.TryParse(companyRadiusStr.SettingValue, out double r))
                    radius = r;

                var dist = CalculateDistance(cLat, cLng, lat.Value, lon.Value);
                return dist <= radius;
            }

            return true;
        }

    }
}
