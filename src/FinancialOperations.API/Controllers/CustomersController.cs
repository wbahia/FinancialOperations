using FinancialOperations.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinancialOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;

    public CustomersController(ICustomerRepository customerRepository, IAccountRepository accountRepository)
    {
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerRepository.GetAllAsync();
        return Ok(customers.Select(c => new
        {
            c.Id,
            c.Name,
            c.Document,
            c.Email,
            AccountsCount = c.Accounts.Count
        }));
    }

    [HttpGet("{customerId}/accounts")]
    public async Task<IActionResult> GetCustomerAccounts(Guid customerId)
    {
        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId);
        return Ok(accounts.Select(a => new
        {
            a.Id,
            a.AvailableBalance,
            a.ReservedBalance,
            a.CreditLimit,
            a.Status,
            TotalBalance = a.GetTotalBalance(),
            TotalLimit = a.GetTotalLimit(),
            TransactionsCount = a.Transactions.Count
        }));
    }
}