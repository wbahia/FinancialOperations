using FinancialOperations.Application.Interfaces;
using FluentValidation;

namespace FinancialOperations.Application.UseCases;

public class ProcessTransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ProcessTransferValidator : AbstractValidator<ProcessTransferRequest>
{
    public ProcessTransferValidator()
    {
        RuleFor(x => x.FromAccountId).NotEmpty();
        RuleFor(x => x.ToAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.FromAccountId).NotEqual(x => x.ToAccountId);
    }
}

public class ProcessTransferUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<ProcessTransferRequest> _validator;

    public ProcessTransferUseCase(
        IAccountRepository accountRepository,
        IEventPublisher eventPublisher,
        IValidator<ProcessTransferRequest> validator)
    {
        _accountRepository = accountRepository;
        _eventPublisher = eventPublisher;
        _validator = validator;
    }

    public async Task<bool> ExecuteAsync(ProcessTransferRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId, cancellationToken);
        var toAccount = await _accountRepository.GetByIdAsync(request.ToAccountId, cancellationToken);

        if (fromAccount == null || toAccount == null)
            throw new InvalidOperationException("One or both accounts not found");

        // Lock accounts in a consistent order to prevent deadlocks
        var firstLock = request.FromAccountId.CompareTo(request.ToAccountId) < 0 ? fromAccount : toAccount;
        var secondLock = request.FromAccountId.CompareTo(request.ToAccountId) < 0 ? toAccount : fromAccount;

        lock (firstLock)
        {
            lock (secondLock)
            {
                fromAccount.Debit(request.Amount, $"Transfer to {request.ToAccountId} - {request.Description}");
                toAccount.Credit(request.Amount, $"Transfer from {request.FromAccountId} - {request.Description}");
            }
        }

        await _accountRepository.UpdateAsync(fromAccount, cancellationToken);
        await _accountRepository.UpdateAsync(toAccount, cancellationToken);

        foreach (var domainEvent in fromAccount.DomainEvents.Concat(toAccount.DomainEvents))
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        fromAccount.ClearDomainEvents();
        toAccount.ClearDomainEvents();

        return true;
    }
}