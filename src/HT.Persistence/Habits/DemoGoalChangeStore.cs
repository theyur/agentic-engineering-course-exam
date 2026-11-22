using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace HT.Persistence.Habits;

/// <summary>
/// The demo goal-change store (slice 8 FR-031, research R-09, ADR 0013, ADR 0026), with the contract of
/// <see cref="DemoPauseStore"/> but each goal's attempts, endings and per-attempt settings versions in place of version 1
/// and attempt 1, through the same <see cref="DemoAccountWriter"/>: one transaction for the demo account with its
/// profile, goals, attempts, versions, endings, records and notes; a goal whose catalog habit is not in one of its
/// accepted states left out with all of them; <see cref="CreateDemoAccountOutcome.EmailTaken"/> creating nothing; a
/// fault rolling everything back and naming no goal, habit, note, outcome or value (HP-010).
/// </summary>
internal sealed class DemoGoalChangeStore : IDemoGoalChangeStore
{
    private readonly DemoAccountWriter _writer;

    public DemoGoalChangeStore(HtDbContext db, UserManager<HtUser> users)
    {
        _writer = new DemoAccountWriter(db, users, "Demo goal-change account creation failed");
    }

    public Task<CreateDemoAccountOutcome> CreateDemoGoalChangeAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoGoalChangeGoal> goals,
        CancellationToken cancellationToken) =>
        _writer.CreateWithAttemptsAsync(email, password, profile, goals, cancellationToken);
}
