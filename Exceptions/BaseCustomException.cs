namespace Cüzdan_Uygulaması.Exceptions;

public abstract class BaseCustomException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    protected BaseCustomException(string message, string errorCode, int statusCode) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected BaseCustomException(string message, string errorCode, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}