using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaygroundDashboard.Data;
using PlaygroundDashboard.Models;

namespace PlaygroundDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var s = await _db.Settings.FirstOrDefaultAsync();
        return Ok(new { responsibilityText = s?.ResponsibilityText ?? "" });
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsDto dto)
    {
        var s = await _db.Settings.FirstOrDefaultAsync();
        if (s is null)
        {
            s = new PlaygroundSettings { ResponsibilityText = dto.ResponsibilityText ?? "" };
            _db.Settings.Add(s);
        }
        else
        {
            s.ResponsibilityText = dto.ResponsibilityText ?? "";
        }
        await _db.SaveChangesAsync();
        return Ok(new { responsibilityText = s.ResponsibilityText });
    }
}

public record UpdateSettingsDto(string? ResponsibilityText);
