namespace FinancialOperations.Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
}