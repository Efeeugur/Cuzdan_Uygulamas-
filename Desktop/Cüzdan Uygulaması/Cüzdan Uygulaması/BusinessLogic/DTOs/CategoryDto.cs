namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateTime CreatedDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class UpdateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}