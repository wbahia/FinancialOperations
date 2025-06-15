using FinancialOperations.Application.Interfaces;
using FluentValidation;


namespace FinancialOperations.Application.UseCases;

public class ProcessCreditRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ProcessCreditValidator : AbstractValidator<ProcessCreditRequest>
{
    public ProcessCreditValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class ProcessCreditUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<ProcessCreditRequest> _validator;

    public ProcessCreditUseCase(
        IAccountRepository accountRepository,
        IEventPublisher eventPublisher,
        IValidator<ProcessCreditRequest> validator)
    {
        _accountRepository = accountRepository;
        _eventPublisher = eventPublisher;
        _validator = validator;
    }

    public async Task<bool> ExecuteAsync(ProcessCreditRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new InvalidOperationException("Account not found");

        account.Credit(request.Amount, request.Description);
        await _accountRepository.UpdateAsync(account, cancellationToken);

        foreach (var domainEvent in account.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        account.ClearDomainEvents();

        return true;
    }
}