using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class EventDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [PositiveDistance]
    public decimal Distance { get; set; }

    [Required]
    public string EventType { get; set; } = string.Empty;
}