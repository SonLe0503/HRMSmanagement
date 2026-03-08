using System;
using System.ComponentModel.DataAnnotations;

public class CreateLeaveRequestDto
{
    public string LeaveType { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = null!;
    public bool SubmitEvenIfInsufficient { get; set; } = false;
}
