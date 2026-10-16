using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class UpdateProfileDto
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;
}