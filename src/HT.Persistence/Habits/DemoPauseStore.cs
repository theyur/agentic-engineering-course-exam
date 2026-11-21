using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace HT.Persistence.Habits;

/// <summary>
/// The demo pause store (FR-031, research R-08, ADR 0013, ADR 0023), with the contract of <see cref="DemoReportStore"/>
/// plus each goal's excusals and pauses, through the same <see cref="DemoAccountWriter"/>: one transaction for the demo
/// account with its profile, goals, records, notes, excusals and pauses; a goal whose catalog habit is not in one of its
/// accepted states left out with all of them; <see cref="CreateDemoAccountOutcome.EmailTaken"/> creating nothing; a
/// fault rolling everything back and naming no goal, habit, note, reason or value (HP-009).
/// </summary>
internal sealed class DemoPauseStore : IDemoPauseStore
{
    private readonly DemoAccountWriter _writer;

    public DemoPauseStore(HtDbContext db, UserManager<HtUser> users)
    {
        _writer = new DemoAccountWriter(db, users, "Demo pause account creation failed");
    }

    public Task<CreateDemoAccountOutcome> CreateDemoPauseAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoPauseGoal> goals,
        CancellationToken cancellationToken) =>
        _writer.CreateWithExemptionsAsync(email, password, profile, goals, cancellationToken);
}
