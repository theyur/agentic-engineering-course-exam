using HT.Ui.Habits;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Habits;

/// <summary>Research R-15: every use-case call of a circuit runs in its own scope, disposed after the call.</summary>
public sealed class UseCaseScopeTests
{
    private static (UseCaseScope Scope, ServiceProvider Provider) Build()
    {
        var provider = new ServiceCollection()
            .AddScoped<Probe>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        return (new UseCaseScope(provider.GetRequiredService<IServiceScopeFactory>()), provider);
    }

    [Fact]
    public async Task Two_calls_get_two_scoped_instances()
    {
        var (scope, provider) = Build();
        await using var owned = provider;

        var first = await scope.RunAsync<Probe, Probe>((p, _) => Task.FromResult(p), CancellationToken.None);
        var second = await scope.RunAsync<Probe, Probe>((p, _) => Task.FromResult(p), CancellationToken.None);

        Assert.NotSame(first, second);
        Assert.True(first.Disposed);
        Assert.True(second.Disposed);
    }

    [Fact]
    public async Task Overlapping_calls_never_share_an_instance()
    {
        var (scope, provider) = Build();
        await using var owned = provider;
        var gate = new TaskCompletionSource();

        var first = scope.RunAsync<Probe, Probe>(async (p, _) => { await gate.Task; return p; }, CancellationToken.None);
        var second = scope.RunAsync<Probe, Probe>(async (p, _) => { await gate.Task; return p; }, CancellationToken.None);
        gate.SetResult();

        var probes = await Task.WhenAll(first, second);
        Assert.NotSame(probes[0], probes[1]);
    }

    [Fact]
    public async Task The_scope_is_disposed_after_a_call_that_throws()
    {
        var (scope, provider) = Build();
        await using var owned = provider;
        Probe? seen = null;

        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.RunAsync<Probe, int>(
            (p, _) =>
            {
                seen = p;
                throw new InvalidOperationException("fails");
            },
            CancellationToken.None));

        Assert.NotNull(seen);
        Assert.True(seen.Disposed);
    }

    private sealed class Probe : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
