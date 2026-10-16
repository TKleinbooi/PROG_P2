using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDayAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        RaceDayContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var roleName = dto.Role.Trim();

        if (roleName != "Organiser" && roleName != "Participant")
            return BadRequest("Role must be Organiser or Participant.");

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email))
            return Conflict("An account with this email already exists.");

        var role = await _context.Roles
            .SingleOrDefaultAsync(r => r.RoleName == roleName);

        if (role == null)
            return StatusCode(500, "Required role data is missing.");

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleID = role.RoleID
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created("", new
        {
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            Role = role.RoleName
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        if (user.Role == null)
            return StatusCode(500, "User role is not configured.");

        var key = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
            return StatusCode(500, "Authentication is not configured.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,
                user.UserID.ToString()),
            new Claim(ClaimTypes.Name,
                user.Email),
            new Claim(ClaimTypes.Role,
                user.Role.RoleName)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresUtc = token.ValidTo,
            user = new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = user.Role.RoleName
            }
        });
    }
}