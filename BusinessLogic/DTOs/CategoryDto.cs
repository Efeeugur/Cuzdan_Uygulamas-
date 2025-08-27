using System.ComponentModel.DataAnnotations;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public CategoryType Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [StringLength(7, ErrorMessage = "Color must be a valid hex color")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in hex format (e.g., #FF0000)")]
    [Display(Name = "Color")]
    public string? Color { get; set; }
    
    [Required(ErrorMessage = "Category type is required")]
    [Display(Name = "Category Type")]
    public CategoryType Type { get; set; }
}

public class UpdateCategoryDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [StringLength(7, ErrorMessage = "Color must be a valid hex color")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in hex format (e.g., #FF0000)")]
    [Display(Name = "Color")]
    public string? Color { get; set; }
    
    [Required(ErrorMessage = "Category type is required")]
    [Display(Name = "Category Type")]
    public CategoryType Type { get; set; }
}