using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiedraAzul.Audit.Data;
using PiedraAzul.Audit.Models;

namespace PiedraAzul.Audit.Controllers;

[ApiController]
[Route("audit")]
public class AuditController : ControllerBase
{
    private readonly AuditDbContext _db;

    public AuditController(AuditDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Log([FromBody] AuditRequest req)
    {
        _db.Entries.Add(new AuditEntry
        {
            Action     = req.Action,
            EntityType = req.EntityType,
            EntityId   = req.EntityId,
            UserId     = req.UserId,
            Detail     = req.Detail,
            Timestamp  = req.Timestamp
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? userId,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.Entries.AsQueryable();

        if (userId is not null)
            query = query.Where(e => e.UserId == userId);
        if (entityType is not null)
            query = query.Where(e => e.EntityType == entityType);

        var entries = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(entries);
    }
}
