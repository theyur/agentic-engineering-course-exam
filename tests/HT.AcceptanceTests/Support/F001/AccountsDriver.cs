using HT.Application;
using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.AcceptanceTests.Support.F001;

/// <summary>
/// Builds the application without HT.Ui (ADR 0003: the persistence composition entry point plus the
/// application registration) and runs each use case in its own service scope, as one request would.
/// The current time comes from a <see cref="FakeTimeProvider"/> (constitution II).
/// </summary>
public sealed class AccountsDriver : IDisposable
{
    /// <summary>
    /// Start of the scenario clock. Steps without a time may use any current time; every time written in
    /// this feature is later than this instant, so the clock only moves forward.
    /// </summary>
    public static readonly DateTimeOffset ClockStart = new(2026, 11, 11, 0, 0, 0, TimeSpan.Zero);

    private readonly ServiceProvider _services;

    public AccountsDriver()
    {
        Time = new FakeTimeProvider(ClockStart);

        var services = new ServiceCollection();
        services.AddHtPersistence(F001Database.ConnectionString);
        services.AddHtApplication();
        services.AddSingleton<TimeProvider>(Time);
        _services = services.BuildServiceProvider();
    }

    public FakeTimeProvider Time { get; }

    public void SetNow(DateTimeOffset now) => Time.SetUtcNow(now);

    public Task<RegistrationResult> RegisterAsync(string? email, string? password) =>
        WithStoreAsync(store => new AccountService(store, Time).RegisterAsync(email, password, CancellationToken.None));

    public Task<SignInResult> SignInAsync(string? email, string? password) =>
        WithStoreAsync(store => new AccountService(store, Time).SignInAsync(email, password, CancellationToken.None));

    public Task<AccountDetailsResult> GetAccountDetailsAsync(ActingUser actingUser) =>
        WithStoreAsync(store => new AccountService(store, Time).GetAccountDetailsAsync(actingUser, CancellationToken.None));

    public Task<PasswordChangeResult> ChangePasswordAsync(ActingUser actingUser, string? currentPassword, string? newPassword) =>
        WithStoreAsync(store => new AccountService(store, Time)
            .ChangePasswordAsync(actingUser, currentPassword, newPassword, CancellationToken.None));

    public Task<AdministratorCreationResult> CreateAdministratorAsync(string? email, string? password) =>
        WithStoreAsync(store => new OperatorService(store).CreateAdministratorAsync(email, password, CancellationToken.None));

    public async Task<DemoLoadingResult> LoadDemoDataAsync(string? demoPassword)
    {
        await using var scope = _services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        var demoData = scope.ServiceProvider.GetRequiredService<IDemoDataStore>();
        return await new DemoDataService(store, demoData).LoadAsync(demoPassword, CancellationToken.None);
    }

    public static Task ResetDatabaseAsync() =>
        HtDatabase.ResetAsync(F001Database.ConnectionString, CancellationToken.None);

    public void Dispose() => _services.Dispose();

    private async Task<T> WithStoreAsync<T>(Func<IAccountStore, Task<T>> useCase)
    {
        await using var scope = _services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        return await useCase(store);
    }
}
