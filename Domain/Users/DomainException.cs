namespace UsersAPI.Domain.Users;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
