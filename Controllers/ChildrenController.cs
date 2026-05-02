using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PlaygroundDashboard.Data;
using PlaygroundDashboard.Hubs;
using PlaygroundDashboard.Models;

namespace PlaygroundDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChildrenController : ControllerBase
{
    readonly AppDbContext               _db;
    readonly IHubContext<PlaygroundHub> _hub;

    public ChildrenController(AppDbContext db, IHubContext<PlaygroundHub> hub)
    {
        _db  = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Children
            .Where(c => c.IsActive)
            .OrderBy(c => c.StartedAt)
            .Select(c => new { c.Id, c.Name, c.StartedAt, c.Duration, c.GuardianName, c.GuardianPhone })
            .ToListAsync());

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddChildDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Ad boş olamaz.");

        var child = new Child
        {
            Name          = dto.Name.Trim(),
            StartedAt     = DateTime.UtcNow,
            Duration      = Math.Clamp(dto.Duration, 5, 180),
            GuardianName  = dto.GuardianName?.Trim() ?? "",
            GuardianPhone = dto.GuardianPhone?.Trim() ?? "",
        };
        _db.Children.Add(child);
        await _db.SaveChangesAsync();

        var payload = new { child.Id, child.Name, child.StartedAt, child.Duration, child.GuardianName, child.GuardianPhone };
        await _hub.Clients.All.SendAsync("ChildAdded", payload);
        return Ok(payload);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChildDto dto)
    {
        var child = await _db.Children.FindAsync(id);
        if (child is null || !child.IsActive) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Ad boş olamaz.");

        child.Name          = dto.Name.Trim();
        child.Duration      = Math.Clamp(dto.Duration, 5, 180);
        child.GuardianName  = dto.GuardianName?.Trim() ?? "";
        child.GuardianPhone = dto.GuardianPhone?.Trim() ?? "";

        await _db.SaveChangesAsync();

        var payload = new { child.Id, child.Name, child.StartedAt, child.Duration, child.GuardianName, child.GuardianPhone };
        await _hub.Clients.All.SendAsync("ChildUpdated", payload);
        return Ok(payload);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var child = await _db.Children.FindAsync(id);
        if (child is null) return NotFound();

        child.IsActive = false;
        await _db.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("ChildRemoved", id);
        return NoContent();
    }
}

public record AddChildDto(string Name, int Duration, string? GuardianName = null, string? GuardianPhone = null);
public record UpdateChildDto(string Name, int Duration, string? GuardianName = null, string? GuardianPhone = null);
