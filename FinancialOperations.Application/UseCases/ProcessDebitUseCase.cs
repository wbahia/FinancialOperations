using FinancialOperations.Application.Interfaces;
using FluentValidation;

namespace FinancialOperations.Application.UseCases;

public class ProcessDebitRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ProcessDebitValidator : AbstractValidator<ProcessDebitRequest>
{
    public ProcessDebitValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class ProcessDebitUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<ProcessDebitRequest> _validator;

    public ProcessDebitUseCase(
        IAccountRepository accountRepository,
        IEventPublisher eventPublisher,
        IValidator<ProcessDebitRequest> validator)
    {
        _accountRepository = accountRepository;
        _eventPublisher = eventPublisher;
        _validator = validator;
    }

    public async Task<bool> ExecuteAsync(ProcessDebitRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new InvalidOperationException("Account not found");

        account.Debit(request.Amount, request.Description);
        await _accountRepository.UpdateAsync(account, cancellationToken);

        foreach (var domainEvent in account.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        account.ClearDomainEvents();

        return true;
    }
}