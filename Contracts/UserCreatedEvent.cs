using MassTransit;

namespace UsersAPI.Contracts;

[MessageUrn("Fcg.Contracts:UserCreatedEvent")]
[EntityName("fcg-user-created")]
public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
