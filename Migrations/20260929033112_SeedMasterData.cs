using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class SeedMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @now DATETIME = GETDATE();
DECLARE @today DATE = GETDATE();

-- Departments
INSERT INTO [Departments] ([DepartmentCode], [DepartmentName], [Description], [IsActive], [CreatedDate]) VALUES
    (N'DEPT-BOD', N'Ban Giam doc', N'Ban dieu hanh', 1, @now),
    (N'DEPT-HR', N'Phong Nhan su', N'Quan ly nhan su, tuyen dung, tinh luong', 1, @now),
    (N'DEPT-SALES', N'Phong Kinh doanh', N'Kinh doanh va cham soc khach hang', 1, @now);

-- Positions
INSERT INTO [Positions] ([PositionCode], [PositionName], [Description], [Level], [IsTopLevel], [IsActive], [CreatedDate]) VALUES
    (N'POS-DIRECTOR', N'Giam doc', N'Nguoi dung dau to chuc', 1, 1, 1, @now),
    (N'POS-HRMGR', N'Truong phong Nhan su', N'Quan ly phong Nhan su', 2, 0, 1, @now),
    (N'POS-SALESMGR', N'Truong phong Kinh doanh', N'Quan ly phong Kinh doanh', 2, 0, 1, @now),
    (N'POS-STAFF', N'Nhan vien', N'Nhan vien tac nghiep', 3, 0, 1, @now);

-- Shifts
INSERT INTO [Shifts] ([ShiftCode], [ShiftName], [StartTime], [EndTime], [WorkingHours], [ShiftType], [IsActive], [CreatedDate]) VALUES
    (N'SHIFT-HC', N'Ca hanh chinh', '08:00:00', '17:00:00', 8, N'Regular', 1, @now);

-- Leave types
INSERT INTO [LeaveTypes] ([LeaveTypeCode], [LeaveTypeName], [AnnualEntitlement], [IsPaid], [RequiresApproval], [IsCarryForward], [MaxCarryForwardDays], [IsActive], [CreatedDate]) VALUES
    (N'LT-ANNUAL', N'Nghi phep nam', 12, 1, 1, 1, 5, 1, @now),
    (N'LT-SICK', N'Nghi om', 30, 1, 1, 0, 0, 1, @now),
    (N'LT-UNPAID', N'Nghi khong luong', 0, 0, 1, 0, 0, 1, @now);

-- Payroll allowance policies
INSERT INTO [PayrollPolicies] ([PolicyName], [PolicyType], [Description], [BaseAmount], [ApplicableEmployeeGroup], [EffectiveStartDate], [IsActive], [CreatedDate]) VALUES
    (N'Phu cap an trua', N'Allowance', N'Phu cap an trua hang thang', 730000, N'All', @today, 1, @now),
    (N'Phu cap xang xe', N'Allowance', N'Phu cap di lai hang thang', 300000, N'All', @today, 1, @now);

-- Link seed employees to department / position / manager
UPDATE [Employees] SET
    [DepartmentId] = (SELECT DepartmentId FROM Departments WHERE DepartmentCode = N'DEPT-BOD'),
    [PositionId] = (SELECT PositionId FROM Positions WHERE PositionCode = N'POS-DIRECTOR')
WHERE [EmployeeCode] = N'EMP0001';

UPDATE [Employees] SET
    [DepartmentId] = (SELECT DepartmentId FROM Departments WHERE DepartmentCode = N'DEPT-HR'),
    [PositionId] = (SELECT PositionId FROM Positions WHERE PositionCode = N'POS-HRMGR'),
    [ManagerId] = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0001')
WHERE [EmployeeCode] = N'EMP0002';

UPDATE [Employees] SET
    [DepartmentId] = (SELECT DepartmentId FROM Departments WHERE DepartmentCode = N'DEPT-SALES'),
    [PositionId] = (SELECT PositionId FROM Positions WHERE PositionCode = N'POS-SALESMGR'),
    [ManagerId] = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0001')
WHERE [EmployeeCode] = N'EMP0003';

UPDATE [Employees] SET
    [DepartmentId] = (SELECT DepartmentId FROM Departments WHERE DepartmentCode = N'DEPT-SALES'),
    [PositionId] = (SELECT PositionId FROM Positions WHERE PositionCode = N'POS-STAFF'),
    [ManagerId] = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0003')
WHERE [EmployeeCode] = N'EMP0004';

-- Department managers
UPDATE [Departments] SET [ManagerId] = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0001') WHERE [DepartmentCode] = N'DEPT-BOD';
UPDATE [Departments] SET [ManagerId] = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0002') WHERE [DepartmentCode] = N'DEPT-HR';
UPDATE [Departments] SET [ManagerId] = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0003') WHERE [DepartmentCode] = N'DEPT-SALES';

-- System settings
INSERT INTO [SystemSettings] ([SettingKey], [SettingValue], [SettingCategory], [Description], [ModifiedDate]) VALUES
    (N'OfficeLatitude', N'21.0285', N'Attendance', N'Vi do van phong', @now),
    (N'OfficeLongitude', N'105.8542', N'Attendance', N'Kinh do van phong', @now),
    (N'AttendanceAllowedRadius', N'200', N'Attendance', N'Ban kinh cham cong hop le (met)', @now),
    (N'CheckInMethod', N'Location', N'Attendance', N'Phuong thuc cham cong mac dinh', @now),
    (N'Approval.TopLevelFallbackUserId', (SELECT CAST(UserId AS NVARCHAR(20)) FROM Users WHERE Username = N'admin'), N'Approval', N'Nguoi duyet du phong cap cao nhat', @now),
    (N'Approval.DefaultFallbackUserId', (SELECT CAST(UserId AS NVARCHAR(20)) FROM Users WHERE Username = N'admin'), N'Approval', N'Nguoi duyet du phong mac dinh', @now),
    (N'Payroll.CutOffDay', N'25', N'Payroll', N'Ngay chot cong hang thang', @now),
    (N'Payroll.DefaultReviewWindowDays', N'5', N'Payroll', N'So ngay ra soat cham cong truoc khi tinh luong', @now),
    (N'Payroll.Calc.BhxhRate', N'0.08', N'Payroll', N'Ty le BHXH nguoi lao dong', @now),
    (N'Payroll.Calc.BhytRate', N'0.015', N'Payroll', N'Ty le BHYT nguoi lao dong', @now),
    (N'Payroll.Calc.BhtnRate', N'0.01', N'Payroll', N'Ty le BHTN nguoi lao dong', @now),
    (N'Payroll.Calc.InsuranceCap', N'36000000', N'Payroll', N'Muc luong toi da dong bao hiem', @now),
    (N'Payroll.Calc.InsuranceBaseMode', N'Gross', N'Payroll', N'Co so tinh bao hiem', @now),
    (N'Payroll.Calc.InsuranceFixedBase', N'0', N'Payroll', N'Muc luong co dinh dong bao hiem (neu co)', @now),
    (N'Payroll.Calc.PersonalDeduction', N'11000000', N'Payroll', N'Muc giam tru gia canh ban than', @now),
    (N'Payroll.Calc.DependentDeduction', N'4400000', N'Payroll', N'Muc giam tru gia canh nguoi phu thuoc', @now),
    (N'Payroll.Calc.OtWeekdayMultiplier', N'1.5', N'Payroll', N'He so tang ca ngay thuong', @now),
    (N'Payroll.Calc.OtWeekendMultiplier', N'2.0', N'Payroll', N'He so tang ca cuoi tuan', @now),
    (N'Payroll.Calc.OtHolidayMultiplier', N'3.0', N'Payroll', N'He so tang ca ngay le', @now),
    (N'Company.Name', N'HRMS Demo Company', N'Company', N'Ten cong ty', @now),
    (N'Company.Address', N'123 Duong ABC, Ha Noi', N'Company', N'Dia chi cong ty', @now),
    (N'Company.Phone', N'0123456789', N'Company', N'So dien thoai cong ty', @now),
    (N'Company.Email', N'contact@hrms.local', N'Company', N'Email cong ty', @now),
    (N'ResignationNoticeDays', N'30', N'Resignation', N'So ngay bao truoc khi nghi viec', @now);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [SystemSettings] WHERE [SettingKey] IN (
    N'OfficeLatitude', N'OfficeLongitude', N'AttendanceAllowedRadius', N'CheckInMethod',
    N'Approval.TopLevelFallbackUserId', N'Approval.DefaultFallbackUserId',
    N'Payroll.CutOffDay', N'Payroll.DefaultReviewWindowDays',
    N'Payroll.Calc.BhxhRate', N'Payroll.Calc.BhytRate', N'Payroll.Calc.BhtnRate', N'Payroll.Calc.InsuranceCap',
    N'Payroll.Calc.InsuranceBaseMode', N'Payroll.Calc.InsuranceFixedBase', N'Payroll.Calc.PersonalDeduction',
    N'Payroll.Calc.DependentDeduction', N'Payroll.Calc.OtWeekdayMultiplier', N'Payroll.Calc.OtWeekendMultiplier',
    N'Payroll.Calc.OtHolidayMultiplier', N'Company.Name', N'Company.Address', N'Company.Phone', N'Company.Email',
    N'ResignationNoticeDays'
);

UPDATE [Departments] SET [ManagerId] = NULL WHERE [DepartmentCode] IN (N'DEPT-BOD', N'DEPT-HR', N'DEPT-SALES');

UPDATE [Employees] SET [DepartmentId] = NULL, [PositionId] = NULL, [ManagerId] = NULL
WHERE [EmployeeCode] IN (N'EMP0001', N'EMP0002', N'EMP0003', N'EMP0004');

DELETE FROM [PayrollPolicies] WHERE [PolicyName] IN (N'Phu cap an trua', N'Phu cap xang xe');
DELETE FROM [LeaveTypes] WHERE [LeaveTypeCode] IN (N'LT-ANNUAL', N'LT-SICK', N'LT-UNPAID');
DELETE FROM [Shifts] WHERE [ShiftCode] = N'SHIFT-HC';
DELETE FROM [Positions] WHERE [PositionCode] IN (N'POS-DIRECTOR', N'POS-HRMGR', N'POS-SALESMGR', N'POS-STAFF');
DELETE FROM [Departments] WHERE [DepartmentCode] IN (N'DEPT-BOD', N'DEPT-HR', N'DEPT-SALES');
");
        }
    }
}
