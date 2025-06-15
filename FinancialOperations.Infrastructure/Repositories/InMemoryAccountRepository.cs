using FinancialOperations.Application.Interfaces;
using FinancialOperations.Domain.Entities;
using System.Collections.Concurrent;

namespace FinancialOperations.Infrastructure.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<Guid, Account> _accounts = new();

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _accounts.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<Account> AddAsync(Account entity, CancellationToken cancellationToken = default)
    {
        _accounts.TryAdd(entity.Id, entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(Account entity, CancellationToken cancellationToken = default)
    {
        _accounts.TryUpdate(entity.Id, entity, _accounts[entity.Id]);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_accounts.Values.AsEnumerable());
    }

    public Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var accounts = _accounts.Values.Where(a => a.CustomerId == customerId);
        return Task.FromResult(accounts);
    }
}