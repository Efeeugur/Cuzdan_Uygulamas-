using Cüzdan_Uygulaması.BusinessLogic.Interfaces;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class CategoryInterestRateService : ICategoryInterestRateService
{
    private readonly ISimpleCategoryService _categoryService;
    
    // Market-based interest rates for installment categories (monthly %)
    private static readonly Dictionary<int, decimal> CategoryRates = new()
    {
        // Electronics & Technology (competitive market, high-value items)
        { 26, 1.99m }, // Elektronik Taksiti
        { 32, 1.99m }, // Teknoloji Taksiti
        
        // Furniture & Home (stable market, moderate competition)
        { 27, 1.49m }, // Mobilya Taksiti
        { 28, 1.49m }, // Beyaz Eşya Taksiti  
        { 31, 1.49m }, // Ev Eşyası Taksiti
        
        // Automotive (bank partnerships, competitive loans)
        { 29, 0.99m }, // Otomobil Taksiti
        
        // Credit Card (highest risk, premium rates)
        { 30, 2.49m }, // Kredi Kartı Taksiti
        
        // Clothing (fashion retail, moderate risk)
        { 33, 1.79m }, // Giyim Taksiti
        
        // Education (social benefit, government support)
        { 34, 0.89m }, // Eğitim Taksiti
        
        // Default/Other
        { 35, 1.99m }  // Diğer Taksitler
    };

    private static readonly Dictionary<int, string> RateExplanations = new()
    {
        { 26, "Elektronik ürünlerde rekabetçi piyasa koşulları nedeniyle ortalama faiz oranı" },
        { 27, "Mobilya sektöründe uzun vadeli ödeme planları için standart oran" },
        { 28, "Beyaz eşya taksitlerinde mağaza ve banka ortaklıkları sayesinde uygun oran" },
        { 29, "Otomobil kredilerinde banka rekabeti nedeniyle en düşük faiz oranları" },
        { 30, "Kredi kartı taksitlerinde yüksek risk nedeniyle prim içeren oran" },
        { 31, "Ev eşyaları için orta segment faiz oranı" },
        { 32, "Teknoloji ürünlerinde hızlı değer kaybı nedeniyle standart oran" },
        { 33, "Tekstil ve giyim sektöründe mevsimsel kampanyalar dikkate alınarak belirlenen oran" },
        { 34, "Eğitim hizmetlerinde sosyal sorumluluk kapsamında uygulanan düşük faiz oranı" },
        { 35, "Genel kategori için piyasa ortalaması faiz oranı" }
    };

    private static readonly Dictionary<int, string> RateSources = new()
    {
        { 26, "Elektronik mağaza zincirleri ortalaması" },
        { 27, "Mobilya sektörü piyasa araştırması" },
        { 28, "Beyaz eşya bayileri faiz oranları" },
        { 29, "Banka otomobil kredisi oranları" },
        { 30, "Kredi kartı taksit oranları" },
        { 31, "Ev eşyası mağazaları taksit oranları" },
        { 32, "Teknoloji perakende sektörü ortalaması" },
        { 33, "Tekstil sektörü taksit kampanyaları" },
        { 34, "Eğitim kurumları ödeme planları" },
        { 35, "Genel piyasa ortalaması" }
    };

    public CategoryInterestRateService(ISimpleCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public decimal GetSuggestedRate(int categoryId)
    {
        try
        {
            // Return specific rate for installment categories (26-35)
            if (CategoryRates.ContainsKey(categoryId))
            {
                return CategoryRates[categoryId];
            }

            // For non-installment categories, return 0 (no interest expected)
            if (categoryId < 26)
            {
                return 0m;
            }

            // For installment categories without specific rates, use fallback logic
            if (categoryId >= 26 && categoryId <= 35)
            {
                return GetFallbackRate(categoryId);
            }

            // Default rate for unknown categories
            return 1.99m;
        }
        catch (Exception)
        {
            // Ultimate fallback in case of any errors
            return categoryId >= 26 && categoryId <= 35 ? 1.99m : 0m;
        }
    }

    private decimal GetFallbackRate(int categoryId)
    {
        // Intelligent fallback based on category patterns
        return categoryId switch
        {
            >= 26 and <= 28 => 1.79m, // Electronics and appliances - moderate risk
            29 => 0.99m,              // Automotive - bank partnerships
            30 => 2.49m,              // Credit card - high risk
            >= 31 and <= 33 => 1.69m, // Home and clothing - moderate risk
            34 => 0.89m,              // Education - low risk, social benefit
            _ => 1.99m                // Default for any other installment category
        };
    }

    public CategoryRateInfo GetRateInfo(int categoryId)
    {
        var rate = GetSuggestedRate(categoryId);
        var categoryName = _categoryService.GetCategoryName(categoryId);
        
        return new CategoryRateInfo
        {
            CategoryId = categoryId,
            CategoryName = categoryName,
            Rate = rate,
            RateSource = RateSources.GetValueOrDefault(categoryId, "Varsayılan oran"),
            Explanation = RateExplanations.GetValueOrDefault(categoryId, 
                rate == 0m ? "Bu kategori için faiz uygulanmaz" : "Standart taksit faiz oranı"),
            LastUpdated = DateTime.UtcNow,
            IsMarketRate = categoryId >= 26 && categoryId <= 35
        };
    }

    public Dictionary<int, CategoryRateInfo> GetAllCategoryRates()
    {
        var result = new Dictionary<int, CategoryRateInfo>();
        
        // Get all installment category rates
        foreach (var categoryId in CategoryRates.Keys)
        {
            result[categoryId] = GetRateInfo(categoryId);
        }

        return result;
    }

    public void UpdateCategoryRate(int categoryId, decimal newRate)
    {
        if (newRate < 0 || newRate > 99.99m)
        {
            throw new ArgumentException("Interest rate must be between 0 and 99.99%");
        }

        if (categoryId >= 26 && categoryId <= 35)
        {
            CategoryRates[categoryId] = newRate;
            // In a real implementation, this would save to database
            // For now, it only updates the in-memory dictionary
        }
        else
        {
            throw new ArgumentException("Can only update rates for installment categories (26-35)");
        }
    }

    /// <summary>
    /// Gets the best rate available for a category (future enhancement for API integration)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Best available rate from multiple sources</returns>
    public async Task<decimal> GetBestRateAsync(int categoryId)
    {
        // Future implementation could fetch from external APIs
        // For now, return the static rate
        await Task.Delay(1); // Simulate async call
        return GetSuggestedRate(categoryId);
    }

    /// <summary>
    /// Compares rate with market average (future enhancement)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="userRate">User's proposed rate</param>
    /// <returns>Comparison result</returns>
    public RateComparisonResult CompareWithMarket(int categoryId, decimal userRate)
    {
        var marketRate = GetSuggestedRate(categoryId);
        var difference = userRate - marketRate;
        
        return new RateComparisonResult
        {
            MarketRate = marketRate,
            UserRate = userRate,
            Difference = difference,
            IsGoodDeal = userRate <= marketRate,
            SavingsText = difference <= 0 
                ? $"Piyasa ortalamasından {Math.Abs(difference):F2} puan düşük - İyi anlaşma!"
                : $"Piyasa ortalamasından {difference:F2} puan yüksek"
        };
    }
}

public class RateComparisonResult
{
    public decimal MarketRate { get; set; }
    public decimal UserRate { get; set; }
    public decimal Difference { get; set; }
    public bool IsGoodDeal { get; set; }
    public string SavingsText { get; set; } = string.Empty;
}