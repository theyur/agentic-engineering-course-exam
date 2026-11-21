using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace HT.Persistence.Habits;

/// <summary>
/// The demo history store (FR-030, research R-08, ADR 0013). Its one member is one transaction on the scoped context,
/// written by <see cref="DemoAccountWriter"/>, which <see cref="DemoReportStore"/> shares: the demo account with its
/// profile, then each goal whose catalog habit is in one of the goal's accepted states, with its records and notes; a
/// goal whose habit does not qualify is left out with its records and notes; a fault rolls the account back too and
/// names no goal, habit, note or value (HP-006).
/// </summary>
internal sealed class DemoHistoryStore : IDemoHistoryStore
{
    private readonly DemoAccountWriter _writer;

    public DemoHistoryStore(HtDbContext db, UserManager<HtUser> users)
    {
        _writer = new DemoAccountWriter(db, users, "Demo history account creation failed");
    }

    public Task<CreateDemoAccountOutcome> CreateDemoHistoryAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoHistoryGoal> goals,
        CancellationToken cancellationToken) =>
        _writer.CreateAsync(email, password, profile, goals, cancellationToken);
}
