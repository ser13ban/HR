using System.ComponentModel.DataAnnotations;

namespace HrAPI.Models;

public class AbsenceRequest
{
    public int Id { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public AbsenceType Type { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public AbsenceStatus Status { get; set; } = AbsenceStatus.Pending;
    
    public int? ApprovedById { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedBy { get; set; }
    
    // Computed properties
    public int DurationInDays => (EndDate - StartDate).Days + 1;
}

public enum AbsenceType
{
    Vacation = 0,
    SickLeave = 1,
    PersonalLeave = 2,
    Other = 3
}

public enum AbsenceStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}
