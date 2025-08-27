using Cüzdan_Uygulaması.BusinessLogic.Interfaces;

namespace Cüzdan_Uygulaması.Services;

public class RecurringTransactionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringTransactionService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Check every hour

    public RecurringTransactionService(
        IServiceProvider serviceProvider,
        ILogger<RecurringTransactionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Transaction Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRecurringTransactionsAsync();
                await Task.Delay(_interval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing recurring transactions.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes on error
            }
        }

        _logger.LogInformation("Recurring Transaction Service stopped.");
    }

    private async Task ProcessRecurringTransactionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        try
        {
            await transactionService.ProcessRecurringTransactionsAsync();
            _logger.LogInformation("Successfully processed recurring transactions at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process recurring transactions at {Time}", DateTime.UtcNow);
        }
    }
}