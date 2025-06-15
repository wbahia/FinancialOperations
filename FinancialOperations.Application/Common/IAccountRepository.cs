using FinancialOperations.Application.Common;
using FinancialOperations.Domain.Entities;

namespace FinancialOperations.Application.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}