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
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    private int CurrentUserID =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static bool ValidEventType(string type) =>
        new[] { "Run", "Walk", "Cycle" }
            .Contains(type, StringComparer.OrdinalIgnoreCase);

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetEvent(int id)
    {
        var item = await _context.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.EventID == id);

        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(EventDto dto)
    {
        if (!ValidEventType(dto.EventType))
            return BadRequest("Event type must be Run, Walk or Cycle.");

        if (dto.EventDate <= DateTime.UtcNow)
            return BadRequest("Event date must be in the future.");

        var item = new Event
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Date = dto.EventDate,
            Location = dto.Location.Trim(),
            Distance = dto.Distance,
            EventType = dto.EventType.Trim(),
            OrganiserID = CurrentUserID
        };

        _context.Events.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = item.EventID },
            item);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id, EventDto dto)
    {
        if (!ValidEventType(dto.EventType))
            return BadRequest("Event type must be Run, Walk or Cycle.");

        var item = await _context.Events
            .SingleOrDefaultAsync(e => e.EventID == id);

        if (item == null)
            return NotFound();

        if (item.OrganiserID != CurrentUserID)
            return Forbid();

        item.Name = dto.Name.Trim();
        item.Description = dto.Description.Trim();
        item.Date = dto.EventDate;
        item.Location = dto.Location.Trim();
        item.Distance = dto.Distance;
        item.EventType = dto.EventType.Trim();

        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var item = await _context.Events
            .SingleOrDefaultAsync(e => e.EventID == id);

        if (item == null)
            return NotFound();

        if (item.OrganiserID != CurrentUserID)
            return Forbid();

        if (await _context.Enrolments.AnyAsync(e => e.EventID == id))
            return Conflict("An event with enrolments cannot be deleted.");

        _context.Events.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}