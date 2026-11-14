namespace HT.Ui.Habits;

/// <summary>
/// Runs one use-case call in a DI scope of its own (research R-15). An interactive circuit's scope lives as long
/// as the page, and a database context is not safe for concurrent use, so interactive pages never inject the
/// use-case services: each call gets its own context and stores, as a request does on a static page, and two
/// handlers that overlap on an <c>await</c> never share one. The scope is disposed after the call, also when it
/// throws.
/// </summary>
public sealed class UseCaseScope
{
    private readonly IServiceScopeFactory _scopes;

    public UseCaseScope(IServiceScopeFactory scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        _scopes = scopes;
    }

    public async Task<TResult> RunAsync<TService, TResult>(
        Func<TService, CancellationToken, Task<TResult>> call, CancellationToken cancellationToken)
        where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(call);
        await using var scope = _scopes.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await call(service, cancellationToken).ConfigureAwait(false);
    }
}
