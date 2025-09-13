using System.ComponentModel.DataAnnotations;

namespace HrAPI.Models;

public class Feedback
{
    public int Id { get; set; }
    
    [Required]
    public int FromEmployeeId { get; set; }
    
    [Required]
    public int ToEmployeeId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? PolishedContent { get; set; }
    
    public FeedbackType Type { get; set; } = FeedbackType.General;
    
    public int Rating { get; set; } = 5; // 1-10 scale
    
    public bool IsAnonymous { get; set; } = false;
    
    public bool IsPolished { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Employee FromEmployee { get; set; } = null!;
    public virtual Employee ToEmployee { get; set; } = null!;
}

public enum FeedbackType
{
    General = 0,
    Performance = 1,
    Collaboration = 2,
    Communication = 3,
    Leadership = 4,
    Technical = 5
}
