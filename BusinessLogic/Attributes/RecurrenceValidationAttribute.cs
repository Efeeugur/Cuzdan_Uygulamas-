using System.ComponentModel.DataAnnotations;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Attributes;

public class RecurrenceValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is CreateTransactionDto createDto)
        {
            if (createDto.IsRecurring && !createDto.RecurrenceType.HasValue)
            {
                return new ValidationResult("Recurrence type is required when transaction is recurring.");
            }
        }
        else if (validationContext.ObjectInstance is UpdateTransactionDto updateDto)
        {
            if (updateDto.IsRecurring && !updateDto.RecurrenceType.HasValue)
            {
                return new ValidationResult("Recurrence type is required when transaction is recurring.");
            }
        }

        return ValidationResult.Success!;
    }
}