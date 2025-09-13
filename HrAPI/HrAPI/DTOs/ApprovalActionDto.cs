using System.ComponentModel.DataAnnotations;

namespace HrAPI.DTOs;

public class ApprovalActionDto
{
    [StringLength(500)]
    public string? ApproverNotes { get; set; }
}
