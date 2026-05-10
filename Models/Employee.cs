using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public int? ManagerId { get; set; }

    public DateOnly JoinDate { get; set; }

    public DateOnly? ResignationDate { get; set; }

    public string EmploymentStatus { get; set; } = null!;

    public string EmploymentType { get; set; } = null!;

    public decimal? BaseSalary { get; set; }

    public decimal? InsuranceSalary { get; set; }

    public int NumberOfDependents { get; set; } = 0;

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();

    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();

    public virtual ICollection<Evaluation> EvaluationEmployees { get; set; } = new List<Evaluation>();

    public virtual ICollection<Evaluation> EvaluationPrimaryEvaluators { get; set; } = new List<Evaluation>();

    public virtual ICollection<Evaluation> EvaluationSecondaryEvaluators { get; set; } = new List<Evaluation>();

    public virtual ICollection<Hrprocedure> Hrprocedures { get; set; } = new List<Hrprocedure>();

    public virtual ICollection<Employee> InverseManager { get; set; } = new List<Employee>();

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual Employee? Manager { get; set; }

    public virtual ICollection<OvertimeRequest> OvertimeRequests { get; set; } = new List<OvertimeRequest>();

    public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();

    public virtual ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();

    public virtual Position? Position { get; set; }

    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
