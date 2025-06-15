using Microsoft.Extensions.Logging;

namespace FinancialOperations.Infrastructure.Resilience;

public static class RetryPolicyExtensions
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        this Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        ILogger? logger = null)
    {
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        var retryCount = 0;

        while (retryCount <= maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                logger?.LogWarning(ex, "Erro na operacao. Retry {RetryCount}/{MaxRetries}", retryCount, maxRetries);

                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); 
            }
        }
        
        throw new InvalidOperationException("Max retries exceeded");
    }

    public static async Task ExecuteWithRetryAsync(
        this Func<Task> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        ILogger? logger = null)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return true;
        }, maxRetries, initialDelay, logger);
    }
}
