using FinancialOperations.Application.Interfaces;
using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FinancialOperations.Infrastructure.Events;

public class ConsoleEventPublisher : IEventPublisher
{
    private readonly ILogger<ConsoleEventPublisher> _logger;
    private readonly List<IObserver<IDomainEvent>> _observers = new();

    public ConsoleEventPublisher(ILogger<ConsoleEventPublisher> logger)
    {
        _logger = logger;
    }

    public void Subscribe(IObserver<IDomainEvent> observer)
    {
        _observers.Add(observer);
    }

    public void Unsubscribe(IObserver<IDomainEvent> observer)
    {
        _observers.Remove(observer);
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        try
        {
            
            await ProcessEventWithRetryAsync(domainEvent, cancellationToken);

            // Notifica observers
            NotifyObservers(domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar o evento {EventType} ID {EventId}",
                typeof(T).Name, domainEvent.Id);
            throw;
        }
    }

    private async Task ProcessEventWithRetryAsync<T>(T domainEvent, CancellationToken cancellationToken) where T : IDomainEvent
    {
        const int maxRetries = 3;
        var retryCount = 0;
        var delay = TimeSpan.FromMilliseconds(100);

        while (retryCount < maxRetries)
        {
            try
            {
                // Simula 
                await Task.Delay(10, cancellationToken);

                // Simula falhas para testar a resiliencia
                if (Random.Shared.Next(0, 10) == 0 && retryCount == 0)
                {
                    throw new InvalidOperationException("Erro na publicacao de evento simulado");
                }

                _logger.LogInformation("Evento enviado com sucesso: {EventType} - {EventId}",
                    typeof(T).Name, domainEvent.Id);
                return;
            }
            catch (Exception ex) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                _logger.LogWarning(ex, "Erro na publicacao do evento {EventType} - {EventId}. Retry {RetryCount}/{MaxRetries}",
                    typeof(T).Name, domainEvent.Id, retryCount, maxRetries);

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); 
            }
        }
    }

    private void NotifyObservers<T>(T domainEvent) where T : IDomainEvent
    {
        foreach (var observer in _observers)
        {
            try
            {
                observer.OnNext(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar o evento {EventType} - {EventId}",
                    typeof(T).Name, domainEvent.Id);
                observer.OnError(ex);
            }
        }
    }
}