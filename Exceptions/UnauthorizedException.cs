namespace Cüzdan_Uygulaması.Exceptions;

public class UnauthorizedException : BaseCustomException
{
    public UnauthorizedException(string message = "You are not authorized to perform this operation.") 
        : base(message, "UNAUTHORIZED", 401)
    {
    }

    public UnauthorizedException(string resource, string operation) 
        : base($"You are not authorized to {operation} {resource}.", "UNAUTHORIZED", 401)
    {
    }
}