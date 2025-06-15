using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FinancialOperations.Infrastructure.Events;

public class TransactionObserver : IObserver<IDomainEvent>
{
    private readonly ILogger<TransactionObserver> _logger;

    public TransactionObserver(ILogger<TransactionObserver> logger)
    {
        _logger = logger;
    }

    public void OnNext(IDomainEvent value)
    {
        switch (value)
        {
            case TransactionProcessedEvent transactionEvent:
                _logger.LogInformation("Transacao Processada: {Type} | Account: {AccountId} | Amount: {Amount:C} | Description: {Description}",
                    transactionEvent.Transaction.Type,
                    transactionEvent.Transaction.AccountId,
                    transactionEvent.Transaction.Amount,
                    transactionEvent.Transaction.Description);
                break;
            default:
                if(value != null)
                    _logger.LogInformation("Evento: {EventType} - {EventId}",
                        value.GetType().Name, value.Id);
                break;
        }
    }

    public void OnError(Exception error)
    {
        _logger.LogError(error, "Erro em TransactionObserver");
    }

    public void OnCompleted()
    {
        _logger.LogInformation("TransactionObserver concluido");
    }
}