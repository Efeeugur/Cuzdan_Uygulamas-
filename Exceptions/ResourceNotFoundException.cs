namespace Cüzdan_Uygulaması.Exceptions;

public class ResourceNotFoundException : BaseCustomException
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public ResourceNotFoundException(string resourceType, string resourceId) 
        : base($"{resourceType} with ID '{resourceId}' was not found.", "RESOURCE_NOT_FOUND", 404)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public ResourceNotFoundException(string resourceType, int resourceId) 
        : base($"{resourceType} with ID '{resourceId}' was not found.", "RESOURCE_NOT_FOUND", 404)
    {
        ResourceType = resourceType;
        ResourceId = resourceId.ToString();
    }

    public ResourceNotFoundException(string message) 
        : base(message, "RESOURCE_NOT_FOUND", 404)
    {
        ResourceType = "Resource";
        ResourceId = "Unknown";
    }
}