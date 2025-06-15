using FinancialOperations.Domain.Common;

namespace FinancialOperations.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent;
}