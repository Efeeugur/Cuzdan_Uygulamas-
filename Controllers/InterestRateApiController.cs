using Microsoft.AspNetCore.Mvc;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.Services;

namespace Cüzdan_Uygulaması.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterestRateApiController : ControllerBase
{
    private readonly ICategoryInterestRateService _interestRateService;

    public InterestRateApiController(ICategoryInterestRateService interestRateService)
    {
        _interestRateService = interestRateService;
    }

    /// <summary>
    /// Gets the suggested interest rate for a specific category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Interest rate information</returns>
    [HttpGet("category/{categoryId:int}")]
    public ActionResult<object> GetCategoryRate(int categoryId)
    {
        try
        {
            // Validate category ID range
            if (categoryId < 1 || categoryId > 35)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Geçersiz kategori ID",
                    details = "Kategori ID 1-35 arasında olmalıdır"
                });
            }

            var rateInfo = _interestRateService.GetRateInfo(categoryId);
            
            // Provide helpful response even for non-installment categories
            var responseData = new
            {
                categoryId = rateInfo.CategoryId,
                categoryName = rateInfo.CategoryName,
                rate = rateInfo.Rate,
                rateSource = rateInfo.RateSource,
                explanation = rateInfo.Explanation,
                isMarketRate = rateInfo.IsMarketRate,
                lastUpdated = rateInfo.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss"),
                isInstallmentCategory = categoryId >= 26 && categoryId <= 35,
                fallbackUsed = !rateInfo.IsMarketRate && categoryId >= 26 && categoryId <= 35
            };

            return Ok(new
            {
                success = true,
                data = responseData
            });
        }
        catch (ArgumentException argEx)
        {
            return BadRequest(new
            {
                success = false,
                error = "Geçersiz parametre",
                details = argEx.Message
            });
        }
        catch (Exception ex)
        {
            // Log the exception (in real app, use proper logging)
            Console.WriteLine($"Error in GetCategoryRate: {ex}");
            
            // Return a fallback response to keep the UI working
            return Ok(new
            {
                success = true,
                data = new
                {
                    categoryId = categoryId,
                    categoryName = "Bilinmeyen Kategori",
                    rate = categoryId >= 26 && categoryId <= 35 ? 1.99m : 0m,
                    rateSource = "Varsayılan oran",
                    explanation = "Teknik bir sorun nedeniyle varsayılan oran gösteriliyor",
                    isMarketRate = false,
                    lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    isInstallmentCategory = categoryId >= 26 && categoryId <= 35,
                    fallbackUsed = true
                }
            });
        }
    }

    /// <summary>
    /// Gets all category interest rates
    /// </summary>
    /// <returns>All interest rate information</returns>
    [HttpGet("all")]
    public ActionResult<object> GetAllRates()
    {
        try
        {
            var allRates = _interestRateService.GetAllCategoryRates();
            
            var result = allRates.Select(kvp => new
            {
                categoryId = kvp.Key,
                categoryName = kvp.Value.CategoryName,
                rate = kvp.Value.Rate,
                rateSource = kvp.Value.RateSource,
                explanation = kvp.Value.Explanation,
                isMarketRate = kvp.Value.IsMarketRate
            });

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = "Faiz oranları alınamadı",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Compares user rate with market rate
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="userRate">User's proposed rate</param>
    /// <returns>Rate comparison result</returns>
    [HttpGet("compare/{categoryId:int}")]
    public ActionResult<object> CompareRate(int categoryId, [FromQuery] decimal userRate)
    {
        try
        {
            var service = _interestRateService as CategoryInterestRateService;
            if (service == null)
            {
                return BadRequest(new { success = false, error = "Karşılaştırma servisi kullanılamıyor" });
            }

            var comparison = service.CompareWithMarket(categoryId, userRate);
            
            return Ok(new
            {
                success = true,
                data = new
                {
                    marketRate = comparison.MarketRate,
                    userRate = comparison.UserRate,
                    difference = comparison.Difference,
                    isGoodDeal = comparison.IsGoodDeal,
                    savingsText = comparison.SavingsText
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = "Faiz oranı karşılaştırması yapılamadı",
                details = ex.Message
            });
        }
    }
}