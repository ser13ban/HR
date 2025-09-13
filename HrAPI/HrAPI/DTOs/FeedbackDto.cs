using System.ComponentModel.DataAnnotations;
using HrAPI.Models;

namespace HrAPI.DTOs;

public class FeedbackListDto
{
    public int Id { get; set; }
    public string FromEmployeeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PolishedContent { get; set; }
    public FeedbackType Type { get; set; }
    public int Rating { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsPolished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateFeedbackDto
{
    [Required]
    public int ToEmployeeId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    public FeedbackType Type { get; set; } = FeedbackType.General;
    
    [Range(1, 10)]
    public int Rating { get; set; } = 5;
    
    public bool IsAnonymous { get; set; } = false;
}

public class FeedbackDetailDto
{
    public int Id { get; set; }
    public int FromEmployeeId { get; set; }
    public string FromEmployeeName { get; set; } = string.Empty;
    public int ToEmployeeId { get; set; }
    public string ToEmployeeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PolishedContent { get; set; }
    public FeedbackType Type { get; set; }
    public int Rating { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsPolished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
