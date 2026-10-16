using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDayAPI.DTOs;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ProfileController(RaceDayContext context)
    {
        _context = context;
    }

    private int CurrentUserID =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.UserID == CurrentUserID)
            .Select(u => new
            {
                u.UserID,
                u.FirstName,
                u.LastName,
                u.Email,
                Role = u.Role!.RoleName
            })
            .SingleOrDefaultAsync();

        return user == null ? NotFound() : Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        UpdateProfileDto dto)
    {
        var user = await _context.Users
            .FindAsync(CurrentUserID);

        if (user == null)
            return NotFound();

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email
        });
    }
}