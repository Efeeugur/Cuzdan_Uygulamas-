using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesByUserIdAsync(string userId);
    Task<CategoryDto?> GetCategoryByIdAsync(int id, string userId);
    Task<CategoryDto?> GetCategoryWithTransactionsAsync(int id, string userId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string userId);
    Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, string userId);
    Task<bool> DeleteCategoryAsync(int id, string userId);
}