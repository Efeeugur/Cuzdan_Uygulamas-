namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public string? Description { get; set; }
}

public class UpdateAccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}