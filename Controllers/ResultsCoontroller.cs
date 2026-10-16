using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDayAPI.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    private int CurrentUserID =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> RecordResult(ResultDto dto)
    {
        if (dto.FinishTime < TimeSpan.Zero)
            return BadRequest("Finish time cannot be negative.");

        var enrolment = await _context.Enrolments
            .Include(e => e.Event)
            .SingleOrDefaultAsync(e =>
                e.EnrolmentID == dto.EnrolmentID);

        if (enrolment == null)
            return NotFound("Enrolment not found.");

        if (enrolment.Event!.OrganiserID != CurrentUserID)
            return Forbid();

        if (await _context.Results.AnyAsync(
            r => r.EnrolmentID == dto.EnrolmentID))
        {
            return Conflict("A result already exists for this enrolment.");
        }

        var result = new Result
        {
            EnrolmentID = dto.EnrolmentID,
            FinishTime = dto.FinishTime,
            FinishingPosition = dto.Position
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var results = await _context.Results
            .AsNoTracking()
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Event)
            .Where(r => r.Enrolment!.UserID == CurrentUserID)
            .Select(r => new
            {
                r.ResultID,
                EventName = r.Enrolment!.Event!.Name,
                r.FinishTime,
                r.FinishingPosition
            })
            .ToListAsync();

        return Ok(results);
    }
}