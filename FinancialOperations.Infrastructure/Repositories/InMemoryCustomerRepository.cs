using FinancialOperations.Application.Interfaces;
using FinancialOperations.Domain.Entities;
using System.Collections.Concurrent;

namespace FinancialOperations.Infrastructure.Repositories;

public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly ConcurrentDictionary<Guid, Customer> _customers = new();

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _customers.TryGetValue(id, out var customer);
        return Task.FromResult(customer);
    }

    public Task<Customer> AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _customers.TryAdd(entity.Id, entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _customers.TryUpdate(entity.Id, entity, _customers[entity.Id]);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_customers.Values.AsEnumerable());
    }

    public Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var customer = _customers.Values.FirstOrDefault(c => c.Document == document);
        return Task.FromResult(customer);
    }
}