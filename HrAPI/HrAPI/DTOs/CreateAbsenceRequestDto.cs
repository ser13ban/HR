using HrAPI.Models;
using HrAPI.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HrAPI.DTOs;

public class CreateAbsenceRequestDto
{
    [Required]
    [JsonConverter(typeof(AbsenceTypeJsonConverter))]
    public AbsenceType Type { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
