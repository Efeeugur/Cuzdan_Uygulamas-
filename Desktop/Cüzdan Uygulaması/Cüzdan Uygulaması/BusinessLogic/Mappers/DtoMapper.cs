using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Mappers;

public static class DtoMapper
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedDate = user.CreatedDate
        };
    }

    public static AccountDto ToDto(this Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Balance = account.Balance,
            Description = account.Description,
            CreatedDate = account.CreatedDate,
            UserId = account.UserId,
            User = account.User?.ToDto(),
            Transactions = account.Transactions?.Select(t => t.ToDto()).ToList() ?? new List<TransactionDto>()
        };
    }

    public static TransactionDto ToDto(this Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            Type = transaction.Type,
            IsRecurring = transaction.IsRecurring,
            RecurrenceType = transaction.RecurrenceType,
            NextRecurrenceDate = transaction.NextRecurrenceDate,
            CreatedDate = transaction.CreatedDate,
            UserId = transaction.UserId,
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            InstallmentId = transaction.InstallmentId,
            User = transaction.User?.ToDto(),
            Account = transaction.Account?.ToDto(),
            Category = transaction.Category?.ToDto(),
            Installment = transaction.Installment?.ToDto()
        };
    }

    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            CreatedDate = category.CreatedDate,
            UserId = category.UserId,
            User = category.User?.ToDto(),
            Transactions = category.Transactions?.Select(t => t.ToDto()).ToList() ?? new List<TransactionDto>()
        };
    }

    public static InstallmentDto ToDto(this Installment installment)
    {
        return new InstallmentDto
        {
            Id = installment.Id,
            Description = installment.Description,
            TotalAmount = installment.TotalAmount,
            TotalInstallments = installment.TotalInstallments,
            MonthlyPayment = installment.MonthlyPayment,
            InterestRate = installment.InterestRate,
            StartDate = installment.StartDate,
            NextPaymentDate = installment.NextPaymentDate,
            RemainingInstallments = installment.RemainingInstallments,
            IsCompleted = installment.Status == InstallmentStatus.Completed,
            CreatedDate = installment.CreatedDate,
            UserId = installment.UserId,
            CategoryId = installment.CategoryId,
            User = installment.User?.ToDto(),
            Category = installment.Category?.ToDto(),
            Transactions = installment.Transactions?.Select(t => t.ToDto()).ToList() ?? new List<TransactionDto>()
        };
    }

    public static Account ToEntity(this CreateAccountDto dto, string userId)
    {
        return new Account
        {
            Name = dto.Name,
            Balance = dto.InitialBalance,
            Description = dto.Description,
            UserId = userId,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static Transaction ToEntity(this CreateTransactionDto dto, string userId)
    {
        return new Transaction
        {
            Amount = dto.Amount,
            Description = dto.Description,
            TransactionDate = dto.TransactionDate,
            Type = dto.Type,
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.RecurrenceType,
            NextRecurrenceDate = dto.IsRecurring ? CalculateNextRecurrenceDate(dto.TransactionDate, dto.RecurrenceType) : null,
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            InstallmentId = dto.InstallmentId,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static Category ToEntity(this CreateCategoryDto dto, string userId)
    {
        return new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            UserId = userId,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static Installment ToEntity(this CreateInstallmentDto dto, string userId)
    {
        var monthlyPayment = CalculateMonthlyPayment(dto.TotalAmount, dto.TotalInstallments, dto.InterestRate);
        
        return new Installment
        {
            Description = dto.Description,
            TotalAmount = dto.TotalAmount,
            TotalInstallments = dto.TotalInstallments,
            MonthlyPayment = monthlyPayment,
            InterestRate = dto.InterestRate,
            StartDate = dto.FirstPaymentDate,
            NextPaymentDate = dto.FirstPaymentDate.AddMonths(1),
            RemainingInstallments = dto.TotalInstallments,
            Status = InstallmentStatus.Active,
            UserId = userId,
            CategoryId = dto.CategoryId,
            CreatedDate = DateTime.UtcNow
        };
    }

    private static DateTime? CalculateNextRecurrenceDate(DateTime transactionDate, RecurrenceType? recurrenceType)
    {
        return recurrenceType switch
        {
            Models.RecurrenceType.Monthly => transactionDate.AddMonths(1),
            Models.RecurrenceType.Weekly => transactionDate.AddDays(7),
            Models.RecurrenceType.Yearly => transactionDate.AddYears(1),
            _ => null
        };
    }

    private static decimal CalculateMonthlyPayment(decimal totalAmount, int totalInstallments, decimal interestRate)
    {
        if (interestRate == 0)
            return totalAmount / totalInstallments;

        var monthlyRate = interestRate / 100 / 12;
        var payment = totalAmount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), totalInstallments)) /
                     ((decimal)Math.Pow((double)(1 + monthlyRate), totalInstallments) - 1);

        return Math.Round(payment, 2);
    }
}