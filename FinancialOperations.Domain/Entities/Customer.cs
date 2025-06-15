using FinancialOperations.Domain.Common;


namespace FinancialOperations.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; private set; }
    public string Document { get; private set; }
    public string Email { get; private set; }
    private readonly List<Account> _accounts = new();
    public IReadOnlyList<Account> Accounts => _accounts.AsReadOnly();

    private Customer() { } // EF Constructor

    public Customer(string name, string document, string email)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Document = document ?? throw new ArgumentNullException(nameof(document));
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public Account CreateAccount(decimal creditLimit = 0)
    {
        var account = new Account(Id, creditLimit);
        _accounts.Add(account);
        return account;
    }
}