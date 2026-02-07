using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionCode { get; set; } = null!;

    public string PositionName { get; set; } = null!;

    public string? Description { get; set; }

    public int Level { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Hrprocedure> Hrprocedures { get; set; } = new List<Hrprocedure>();
}
