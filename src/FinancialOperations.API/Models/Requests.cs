namespace FinancialOperations.Api.Controllers;

public class CreditRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class DebitRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ReserveRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}