using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class ResultDto
{
    [Range(1, int.MaxValue)]
    public int EnrolmentID { get; set; }

    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int Position { get; set; }
}