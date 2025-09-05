namespace Cüzdan_Uygulaması.Exceptions;

public class BusinessLogicException : BaseCustomException
{
    public BusinessLogicException(string message) 
        : base(message, "BUSINESS_LOGIC_ERROR", 400)
    {
    }

    public BusinessLogicException(string message, Exception innerException) 
        : base(message, "BUSINESS_LOGIC_ERROR", 400, innerException)
    {
    }
}