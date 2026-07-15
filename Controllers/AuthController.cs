using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersAPI.Application.Users;

namespace UsersAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand { Data = dto }, ct);
        return result is null ? Unauthorized() : Ok(result);
    }
}
