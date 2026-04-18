using System;

namespace HRManagement.DTOs.Attendances
{
    public class SubmitAbsentExplanationDto
    {
        public DateOnly Date { get; set; }
        public string Message { get; set; } = null!;
    }
}
