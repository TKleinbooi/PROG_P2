namespace RaceDay.API.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }

    public int UserID { get; set; }
    public User? Participant { get; set; }

    public int EventID { get; set; }
    public Event? Event { get; set; }

    public int CategoryID { get; set; }
    public Category? Category { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
}