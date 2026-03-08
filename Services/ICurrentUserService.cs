using System;

public interface ICurrentUserService
{
    int UserId { get; }
    int? EmployeeId { get; }
    string RoleName { get; }
}
