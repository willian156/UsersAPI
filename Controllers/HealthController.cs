using UsersAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersAPI.Controllers;

[ApiController]
[Route("health")]
public class HealthController(UsersDbContext database) : ControllerBase
{
    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        return await database.Database.CanConnectAsync(cancellationToken)
            ? Ok(new { status = "ready" })
            : StatusCode(503, new { status = "unavailable" });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
