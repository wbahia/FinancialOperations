using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Enums;

namespace FinancialOperations.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public Guid? OriginalTransactionId { get; private set; }

    private Transaction() { } // EF Constructor

    public Transaction(Guid accountId, TransactionType type, decimal amount, string description = "")
    {
        AccountId = accountId;
        Type = type;
        Amount = amount;
        Description = description ?? string.Empty;
        Status = TransactionStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reverse(Guid originalTransactionId)
    {
        OriginalTransactionId = originalTransactionId;
        Status = TransactionStatus.Reversed;
    }
}