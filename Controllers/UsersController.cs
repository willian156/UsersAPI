using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersAPI.Application.Users;

namespace UsersAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserDto dto, CancellationToken ct)
    {
        var user = await sender.Send(new RegisterUserCommand { Data = dto }, ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new GetUsersQuery(), ct));
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var user = await sender.Send(new GetUserByIdQuery { Id = id }, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto, CancellationToken ct)
    {
        var user = await sender.Send(new UpdateUserCommand { Id = id, Data = dto }, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await sender.Send(new DeleteUserCommand { Id = id }, ct) ? NoContent() : NotFound();
}
