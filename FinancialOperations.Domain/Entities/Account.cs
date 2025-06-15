using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Enums;
using FinancialOperations.Domain.Events;


namespace FinancialOperations.Domain.Entities;

public class Account : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal ReservedBalance { get; private set; }
    public decimal CreditLimit { get; private set; }
    public AccountStatus Status { get; private set; }
    private readonly object _lockObject = new();
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { } // EF Constructor

    public Account(Guid customerId, decimal creditLimit = 0)
    {
        CustomerId = customerId;
        CreditLimit = creditLimit;
        Status = AccountStatus.Active;
        AvailableBalance = 0;
        ReservedBalance = 0;
    }

    public decimal GetTotalBalance() => AvailableBalance + ReservedBalance;
    public decimal GetTotalLimit() => AvailableBalance + CreditLimit;

    public void Credit(decimal amount, string description = "")
    {
        lock (_lockObject)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            AvailableBalance += amount;
            var transaction = new Transaction(Id, TransactionType.Credit, amount, description);
            _transactions.Add(transaction);

            AddDomainEvent(new TransactionProcessedEvent(transaction));
        }
    }

    public void Debit(decimal amount, string description = "")
    {
        lock (_lockObject)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (AvailableBalance + CreditLimit < amount)
                throw new InvalidOperationException("Insufficient funds");

            AvailableBalance -= amount;
            var transaction = new Transaction(Id, TransactionType.Debit, amount, description);
            _transactions.Add(transaction);

            AddDomainEvent(new TransactionProcessedEvent(transaction));
        }
    }

    public void Reserve(decimal amount, string description = "")
    {
        lock (_lockObject)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient available balance for reservation");

            AvailableBalance -= amount;
            ReservedBalance += amount;
            var transaction = new Transaction(Id, TransactionType.Reserve, amount, description);
            _transactions.Add(transaction);

            AddDomainEvent(new TransactionProcessedEvent(transaction));
        }
    }

    public void Capture(decimal amount, string description = "")
    {
        lock (_lockObject)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (ReservedBalance < amount)
                throw new InvalidOperationException("Insufficient reserved balance for capture");

            ReservedBalance -= amount;
            var transaction = new Transaction(Id, TransactionType.Capture, amount, description);
            _transactions.Add(transaction);

            AddDomainEvent(new TransactionProcessedEvent(transaction));
        }
    }

    public void Refund(decimal amount, string description = "")
    {
        lock (_lockObject)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (ReservedBalance < amount)
                throw new InvalidOperationException("Insufficient reserved balance for refund");

            ReservedBalance -= amount;
            AvailableBalance += amount;
            var transaction = new Transaction(Id, TransactionType.Refund, amount, description);
            _transactions.Add(transaction);

            AddDomainEvent(new TransactionProcessedEvent(transaction));
        }
    }
}