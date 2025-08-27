using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.Mappers;
using Cüzdan_Uygulaması.DataAccess.Interfaces;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesByUserIdAsync(string userId)
    {
        var categories = await _unitOfWork.Categories.GetCategoriesByUserIdAsync(userId);
        return categories.Select(c => c.ToDto());
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id, string userId)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        return category?.ToDto();
    }

    public async Task<CategoryDto?> GetCategoryWithTransactionsAsync(int id, string userId)
    {
        var category = await _unitOfWork.Categories.GetCategoryWithTransactionsAsync(id);

        if (category == null || category.UserId != userId)
            return null;

        return category.ToDto();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string userId)
    {
        if (string.IsNullOrWhiteSpace(createCategoryDto.Name))
            throw new ArgumentException("Category name is required.");

        var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.UserId == userId && c.Name.ToLower() == createCategoryDto.Name.ToLower());

        if (existingCategory != null)
            throw new InvalidOperationException("A category with this name already exists.");

        if (!string.IsNullOrEmpty(createCategoryDto.Color) && !IsValidColor(createCategoryDto.Color))
            throw new ArgumentException("Color must be in valid hex format (e.g., #FF0000).");

        var category = createCategoryDto.ToEntity(userId);
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, string userId)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.Id == updateCategoryDto.Id && c.UserId == userId);

        if (category == null)
            return null;

        if (string.IsNullOrWhiteSpace(updateCategoryDto.Name))
            throw new ArgumentException("Category name is required.");

        var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
            c => c.UserId == userId && 
                 c.Name.ToLower() == updateCategoryDto.Name.ToLower() && 
                 c.Id != updateCategoryDto.Id);

        if (existingCategory != null)
            throw new InvalidOperationException("A category with this name already exists.");

        if (!string.IsNullOrEmpty(updateCategoryDto.Color) && !IsValidColor(updateCategoryDto.Color))
            throw new ArgumentException("Color must be in valid hex format (e.g., #FF0000).");

        category.Name = updateCategoryDto.Name;
        category.Description = updateCategoryDto.Description;
        category.Color = updateCategoryDto.Color;
        category.Type = updateCategoryDto.Type;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<bool> DeleteCategoryAsync(int id, string userId)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return false;

        var hasTransactions = await _unitOfWork.Transactions.AnyAsync(t => t.CategoryId == id);
        if (hasTransactions)
            throw new InvalidOperationException("Cannot delete category with existing transactions.");

        var hasInstallments = await _unitOfWork.Installments.AnyAsync(i => i.CategoryId == id);
        if (hasInstallments)
            throw new InvalidOperationException("Cannot delete category with existing installments.");

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static bool IsValidColor(string color)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
    }
}