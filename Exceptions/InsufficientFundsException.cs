namespace Cüzdan_Uygulaması.Exceptions;

public class InsufficientFundsException : BaseCustomException
{
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }
    public string AccountName { get; }

    public InsufficientFundsException(decimal requestedAmount, decimal availableBalance, string accountName) 
        : base($"Insufficient funds in account '{accountName}'. Requested: {requestedAmount:C}, Available: {availableBalance:C}", 
               "INSUFFICIENT_FUNDS", 400)
    {
        RequestedAmount = requestedAmount;
        AvailableBalance = availableBalance;
        AccountName = accountName;
    }

    public InsufficientFundsException(string message) 
        : base(message, "INSUFFICIENT_FUNDS", 400)
    {
        RequestedAmount = 0;
        AvailableBalance = 0;
        AccountName = "Unknown";
    }
}