using FinancialOperations.Application.Common;
using FinancialOperations.Domain.Entities;

namespace FinancialOperations.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
}