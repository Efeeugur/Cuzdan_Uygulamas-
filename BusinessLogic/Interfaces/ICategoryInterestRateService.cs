namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface ICategoryInterestRateService
{
    /// <summary>
    /// Gets the suggested interest rate for a specific category
    /// </summary>
    /// <param name="categoryId">Category ID (26-35 for installment categories)</param>
    /// <returns>Suggested monthly interest rate as decimal (e.g., 1.99 for 1.99%)</returns>
    decimal GetSuggestedRate(int categoryId);

    /// <summary>
    /// Gets the suggested interest rate with explanation
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Rate information with explanation</returns>
    CategoryRateInfo GetRateInfo(int categoryId);

    /// <summary>
    /// Gets all category rates for display purposes
    /// </summary>
    /// <returns>Dictionary of category rates</returns>
    Dictionary<int, CategoryRateInfo> GetAllCategoryRates();

    /// <summary>
    /// Updates a category's interest rate (for admin functionality)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="newRate">New interest rate</param>
    void UpdateCategoryRate(int categoryId, decimal newRate);
}

public class CategoryRateInfo
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string RateSource { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public bool IsMarketRate { get; set; } = true;
}