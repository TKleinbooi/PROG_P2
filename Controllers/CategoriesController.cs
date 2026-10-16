
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] Category category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName))
            return BadRequest("Category name is required.");

        category.CategoryName = category.CategoryName.Trim();

        if (category.EventID <= 0)
            return BadRequest("A valid EventID is required.");

        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == category.EventID);

        if (!eventExists)
            return BadRequest("The specified event does not exist.");

        var duplicate = await _context.Categories.AnyAsync(c =>
            c.EventID == category.EventID &&
            c.CategoryName == category.CategoryName);

        if (duplicate)
            return Conflict("This category already exists for the event.");

        category.CategoryID = 0;
        category.Event = null;
        category.Enrolments = new List<Enrolment>();

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategories),
            new { id = category.CategoryID },
            category);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(e => e.CategoryID == id);

        if (hasEnrolments)
        {
            return Conflict(
                "A category with existing enrolments cannot be deleted.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
