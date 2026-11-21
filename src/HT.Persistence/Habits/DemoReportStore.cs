using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace HT.Persistence.Habits;

/// <summary>
/// The demo report store (FR-032, research R-09, ADR 0013, ADR 0021), with the same contract as
/// <see cref="DemoHistoryStore"/> and the same <see cref="DemoAccountWriter"/>: one transaction for the demo account with
/// its profile, goals, records and notes; a goal whose catalog habit is not in one of its accepted states left out with
/// its records and notes; <see cref="CreateDemoAccountOutcome.EmailTaken"/> creating nothing; a fault rolling everything
/// back and naming no goal, habit, note or value (HP-007).
/// </summary>
internal sealed class DemoReportStore : IDemoReportStore
{
    private readonly DemoAccountWriter _writer;

    public DemoReportStore(HtDbContext db, UserManager<HtUser> users)
    {
        _writer = new DemoAccountWriter(db, users, "Demo report account creation failed");
    }

    public Task<CreateDemoAccountOutcome> CreateDemoReportAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoHistoryGoal> goals,
        CancellationToken cancellationToken) =>
        _writer.CreateAsync(email, password, profile, goals, cancellationToken);
}
