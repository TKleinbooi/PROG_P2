using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDayAPI.DTOs;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    private int CurrentUserID =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(EnrolmentDto dto)
    {
        var eventItem = await _context.Events
            .SingleOrDefaultAsync(e => e.EventID == dto.EventID);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (eventItem.Date <= DateTime.UtcNow)
            return BadRequest("This event is no longer accepting enrolments.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.CategoryID == dto.CategoryID);

        if (!categoryExists)
            return BadRequest("Category not found.");

        var duplicate = await _context.Enrolments.AnyAsync(e =>
            e.UserID == CurrentUserID &&
            e.EventID == dto.EventID);

        if (duplicate)
            return Conflict("You are already enrolled in this event.");

        var enrolment = new Enrolment
        {
            UserID = CurrentUserID,
            EventID = dto.EventID,
            CategoryID = dto.CategoryID,
            EnrolmentDate = DateTime.UtcNow
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return Ok(enrolment);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.UserID == CurrentUserID)
            .Select(e => new
            {
                e.EnrolmentID,
                EventName = e.Event!.Name,
                e.EventID,
                Category = e.Category!.CategoryName,
                e.EnrolmentDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    [HttpGet("event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var eventItem = await _context.Events
            .SingleOrDefaultAsync(e => e.EventID == eventId);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (eventItem.OrganiserID != CurrentUserID)
            return Forbid();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Where(e => e.EventID == eventId)
            .Select(e => new
            {
                e.EnrolmentID,
                e.UserID,
                Participant = e.Participant!.FirstName + " " + e.Participant.LastName,
                e.Category!.CategoryName,
                e.EnrolmentDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }
}