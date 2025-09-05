namespace Cüzdan_Uygulaması.Exceptions;

public class ValidationException : BaseCustomException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(string message) 
        : base(message, "VALIDATION_ERROR", 400)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> validationErrors) 
        : base(message, "VALIDATION_ERROR", 400)
    {
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error) 
        : base($"Validation failed for {field}", "VALIDATION_ERROR", 400)
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}