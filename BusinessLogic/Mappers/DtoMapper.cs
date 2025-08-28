using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Services;

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
            //User = account.User?.ToDto(),
            Transactions = new List<TransactionDto>() // Avoid circular reference
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
            Account = transaction.Account?.ToBasicDto(), // Use basic DTO to avoid circular reference
            Category = transaction.Category?.ToBasicDto(), // Use basic DTO to avoid circular reference
            Installment = transaction.Installment?.ToBasicDto() // Use basic DTO to avoid circular reference
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
            Type = category.Type,
            CreatedDate = category.CreatedDate,
            UserId = category.UserId,
            User = category.User?.ToDto(),
            Transactions = new List<TransactionDto>() // Avoid circular reference
        };
    }

    // Basic DTO methods to avoid circular references
    public static AccountDto ToBasicDto(this Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Balance = account.Balance,
            Description = account.Description,
            CreatedDate = account.CreatedDate,
            UserId = account.UserId,
            Transactions = new List<TransactionDto>()
        };
    }

    public static CategoryDto ToBasicDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            Type = category.Type,
            CreatedDate = category.CreatedDate,
            UserId = category.UserId,
            Transactions = new List<TransactionDto>()
        };
    }

    public static InstallmentDto ToDto(this Installment installment)
    {
        var dto = new InstallmentDto
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
            Transactions = new List<TransactionDto>() // Avoid circular reference
        };

        // Handle Category mapping
        if (installment.Category != null)
        {
            dto.Category = installment.Category.ToBasicDto();
        }
        else if (dto.CategoryId.HasValue && dto.CategoryId.Value <= 35)
        {
            // Handle SimpleCategoryService categories (hardcoded categories 1-35)
            var simpleCategoryService = new SimpleCategoryService();
            dto.Category = new CategoryDto
            {
                Id = dto.CategoryId.Value,
                Name = simpleCategoryService.GetCategoryName(dto.CategoryId.Value),
                Type = dto.CategoryId.Value <= 5 ? Models.CategoryType.Income : Models.CategoryType.Expense,
                Transactions = new List<TransactionDto>()
            };
        }

        return dto;
    }

    public static InstallmentDto ToBasicDto(this Installment installment)
    {
        var dto = new InstallmentDto
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
            Transactions = new List<TransactionDto>()
        };

        // Handle SimpleCategoryService categories for basic DTO as well
        if (dto.CategoryId.HasValue && dto.CategoryId.Value <= 35)
        {
            var simpleCategoryService = new SimpleCategoryService();
            dto.Category = new CategoryDto
            {
                Id = dto.CategoryId.Value,
                Name = simpleCategoryService.GetCategoryName(dto.CategoryId.Value),
                Type = dto.CategoryId.Value <= 5 ? Models.CategoryType.Income : Models.CategoryType.Expense,
                Transactions = new List<TransactionDto>()
            };
        }

        return dto;
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
            TransactionDate = dto.TransactionDate.ToUniversalTime(), // Ensure UTC
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
            Type = dto.Type,
            UserId = userId,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static Installment ToEntity(this CreateInstallmentDto dto, string userId)
    {
        var monthlyPayment = CalculateMonthlyPayment(dto.TotalAmount, dto.TotalInstallments, dto.InterestRate);
        var startDateUtc = dto.FirstPaymentDate.ToUniversalTime(); // Ensure UTC
        
        return new Installment
        {
            Description = dto.Description,
            TotalAmount = dto.TotalAmount,
            TotalInstallments = dto.TotalInstallments,
            MonthlyPayment = monthlyPayment,
            InterestRate = dto.InterestRate,
            StartDate = startDateUtc,
            NextPaymentDate = startDateUtc.AddMonths(1),
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