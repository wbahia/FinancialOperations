using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Entities;

namespace FinancialOperations.Domain.Events;

public class TransactionProcessedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Transaction Transaction { get; }

    public TransactionProcessedEvent(Transaction transaction)
    {
        Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }
}