using System.Data.Common;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Persistence.Goals;
using HT.Persistence.Identity;
using HT.Persistence.Records;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The writer of a demo account with history, shared by <see cref="DemoHistoryStore"/> (slice 5 FR-030, research R-08)
/// and <see cref="DemoReportStore"/> (slice 6 FR-032, research R-09), and by <see cref="DemoPauseStore"/> (slice 7
/// FR-031, research R-08), which also writes each goal's excusals and pauses after its records and notes, and by
/// <see cref="DemoGoalChangeStore"/> (slice 8 FR-031, research R-09), which writes each goal's attempts with their endings
/// and per-attempt versions in place of version 1 and attempt 1, so the ports write in exactly the same way (ADR 0013,
/// ADR 0021, ADR 0023, ADR 0026). Each of its members is one transaction on the scoped context,
/// which Identity's user manager uses too: the demo account is inserted with its profile, then each goal whose catalog
/// habit, looked up by name key under the key-range locks of goal creation, is in one of the goal's accepted states,
/// with settings version 1 in the profile's zone and attempt 1 from its start date, its records in list order (one
/// save each, so the stored order is the list's) and its notes, mapped by <see cref="RecordMapping"/>. A goal whose
/// habit does not qualify is left out with its records and notes. A goal, record or note that cannot be written is a
/// fault that rolls the account back too; its exception names the SQL error number only, with no inner exception, so
/// no goal, habit, note or value reaches a log (HP-006). The change tracker is cleared before and after the call, as
/// in the other stores. Each fault text starts with the caller's <c>failure</c> prefix, so each store keeps its own.
/// </summary>
internal sealed class DemoAccountWriter
{
    private const string DuplicateUserNameCode = nameof(IdentityErrorDescriber.DuplicateUserName);

    private readonly HtDbContext _db;
    private readonly UserManager<HtUser> _users;
    private readonly string _failure;

    /// <param name="db">The calling store's context; the account, goals, records and notes are saved through it.</param>
    /// <param name="users">The calling store's user manager, which creates the account.</param>
    /// <param name="failure">The start of every fault text, such as "Demo history account creation failed"; it names no value.</param>
    public DemoAccountWriter(HtDbContext db, UserManager<HtUser> users, string failure)
    {
        _db = db;
        _users = users;
        _failure = failure;
    }

    /// <summary>The demo account with these goals, their records and their notes (demo history and demo report loading).</summary>
    public Task<CreateDemoAccountOutcome> CreateAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoHistoryGoal> goals,
        CancellationToken cancellationToken) =>
        CreateAsync(
            email,
            password,
            profile,
            goals,
            static goal => (goal.Goal.CatalogHabitName, goal.Goal.AcceptedStates),
            (owner, habit, goal, ct) => WriteGoalAsync(owner, habit, goal, profile, ct),
            cancellationToken);

    /// <summary>
    /// The demo account with these goals, their records and their notes, then each written goal's excusals and pauses
    /// (slice 7 FR-031, research R-08), in the same transaction: a goal left out is left out with its excusals and
    /// pauses, and an excusal or pause that cannot be written is a fault that rolls everything back.
    /// </summary>
    public Task<CreateDemoAccountOutcome> CreateWithExemptionsAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoPauseGoal> goals,
        CancellationToken cancellationToken) =>
        CreateAsync(
            email,
            password,
            profile,
            goals,
            static goal => (goal.History.Goal.CatalogHabitName, goal.History.Goal.AcceptedStates),
            async (owner, habit, goal, ct) =>
            {
                var goalId = await WriteGoalAsync(owner, habit, goal.History, profile, ct).ConfigureAwait(false);
                await WriteExemptionsAsync(goalId, goal, ct).ConfigureAwait(false);
            },
            cancellationToken);

    /// <summary>
    /// Slice 8 (FR-031, research R-09): the demo account with these goals, each with its attempts (rows with their
    /// endings) and each attempt's settings versions in the profile's zone, then its records and notes, in the same
    /// transaction: a goal left out is left out with all of them, and one that cannot be written is a fault that rolls
    /// everything back.
    /// </summary>
    public Task<CreateDemoAccountOutcome> CreateWithAttemptsAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoGoalChangeGoal> goals,
        CancellationToken cancellationToken) =>
        CreateAsync(
            email,
            password,
            profile,
            goals,
            static goal => (goal.CatalogHabitName, goal.AcceptedStates),
            (owner, habit, goal, ct) => WriteGoalWithAttemptsAsync(owner, habit, goal, profile, ct),
            cancellationToken);

    /// <summary>
    /// The one transaction: the account, then each goal of <paramref name="goals"/> whose catalog habit, as
    /// <paramref name="habitOf"/> names it with the goal's accepted states, qualifies, written by
    /// <paramref name="writeGoal"/> for the new owner and that habit.
    /// </summary>
    private async Task<CreateDemoAccountOutcome> CreateAsync<TGoal>(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<TGoal> goals,
        Func<TGoal, (string? HabitName, IReadOnlySet<CatalogHabitState> AcceptedStates)> habitOf,
        Func<AccountId, Guid, TGoal, CancellationToken, Task> writeGoal,
        CancellationToken cancellationToken)
        where TGoal : class
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(goals);
        _db.ChangeTracker.Clear();
        try
        {
            // Disposed without a commit, the transaction rolls back: the account, every goal, record and note, or nothing.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            if (await CreateUserAsync(email, password, profile).ConfigureAwait(false) is not { } owner)
            {
                return CreateDemoAccountOutcome.EmailTaken;
            }

            // Identity's user manager tracks the new user; the goals below must not write it again.
            _db.ChangeTracker.Clear();

            foreach (var goal in goals)
            {
                ArgumentNullException.ThrowIfNull(goal);
                var (habitName, acceptedStates) = habitOf(goal);
                if (await PublishedHabitOfAsync(habitName, acceptedStates, cancellationToken).ConfigureAwait(false) is { } habit)
                {
                    await writeGoal(owner, habit, goal, cancellationToken).ConfigureAwait(false);
                }
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return CreateDemoAccountOutcome.Created;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    /// <summary>The new registered user with the profile, written at insert only as account creation does; null when the email is taken.</summary>
    private async Task<AccountId?> CreateUserAsync(Email email, string password, DemoAccountProfile profile)
    {
        var user = new HtUser
        {
            Id = Guid.NewGuid(),
            UserName = email.Value,
            Email = email.Value,
            Role = Role.RegisteredUser,
            Name = profile.Name,
            TimeZone = profile.TimeZone.Id,
            CreatedAt = profile.CreatedAt,
        };

        IdentityResult result;
        try
        {
            result = await _users.CreateAsync(user, password).ConfigureAwait(false);
        }
        catch (DbUpdateException exception) when (HabitMapping.IsUniqueIndexViolation(exception))
        {
            return null;
        }

        if (result.Succeeded)
        {
            return new AccountId(user.Id);
        }

        return result.Errors.Any(e => e.Code == DuplicateUserNameCode)
            ? null
            : throw new InvalidOperationException(
                _failure + ": " + string.Join(", ", result.Errors.Select(e => e.Code)));
    }

    /// <summary>
    /// The goal's catalog habit, or null when it is absent or not in one of the goal's accepted states. Read by name
    /// key with its state under UPDLOCK, HOLDLOCK until the commit, so it cannot change state in between; that state is
    /// the only data this store reads.
    /// </summary>
    private async Task<Guid?> PublishedHabitOfAsync(
        string? catalogHabitName, IReadOnlySet<CatalogHabitState> acceptedStates, CancellationToken cancellationToken)
    {
        var key = HabitName.KeyOf(catalogHabitName
            ?? throw new InvalidOperationException(_failure + ": a demo goal names no catalog habit."));
        var found = await WriteAsync(() => _db.Database.SqlQuery<CatalogHabitStateRow>(
                    $"SELECT [Id], [State] FROM [CatalogHabits] WITH (UPDLOCK, HOLDLOCK) WHERE [NameKey] = {key}")
                .ToListAsync(cancellationToken))
            .ConfigureAwait(false);
        return found is [var row]
               && Enum.TryParse<CatalogHabitState>(row.State, ignoreCase: false, out var state)
               && acceptedStates.Contains(state)
            ? row.Id
            : null;
    }

    /// <summary>
    /// The goal row with settings version 1 in the profile's zone and attempt 1, both from the start date; then its
    /// records, one save each in list order, so the identity column keeps that order; then its notes, one save each,
    /// so a second note under the same entry key is a primary-key fault of the database. Gives the new goal's identifier.
    /// </summary>
    private async Task<Guid> WriteGoalAsync(
        AccountId owner, Guid habitId, DemoHistoryGoal goal, DemoAccountProfile profile, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var demoGoal = goal.Goal;
        var version = new GoalSettingsVersion(new GoalSettings(demoGoal.Target, profile.TimeZone), demoGoal.StartDate);
        await SaveAsync(new GoalRow
        {
            Id = id,
            OwnerId = owner.Value,
            CatalogHabitId = habitId,
            StartDate = demoGoal.StartDate,
            Name = demoGoal.OwnName?.Value,
            Versions = [GoalMapping.ToRow(id, version)],
            Attempts = [new GoalAttemptRow { GoalId = id, Number = GoalAttempt.First, StartDate = demoGoal.StartDate }],
        }, cancellationToken).ConfigureAwait(false);

        await WriteRecordsAndNotesAsync(id, goal.Records, goal.Notes, cancellationToken).ConfigureAwait(false);
        return id;
    }

    /// <summary>
    /// Slice 8 (research R-09): the goal row from attempt 1's start date, with no own name, with its attempt rows, each
    /// with its ending (<see cref="AttemptRowOf"/>), and every attempt's settings versions in the profile's zone
    /// (<see cref="VersionRowsOf"/>), in one save; then its records and notes as <see cref="WriteGoalAsync"/> writes
    /// them. The rows pass <see cref="GoalMapping"/>'s checks on read, because <see cref="DemoGoalChangeGoal"/> has
    /// checked the attempts against the goal invariants.
    /// </summary>
    private async Task WriteGoalWithAttemptsAsync(
        AccountId owner, Guid habitId, DemoGoalChangeGoal goal, DemoAccountProfile profile, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        await SaveAsync(new GoalRow
        {
            Id = id,
            OwnerId = owner.Value,
            CatalogHabitId = habitId,
            StartDate = goal.StartDate,
            Versions = VersionRowsOf(id, goal, profile),
            Attempts = [.. goal.Attempts.Select(a => AttemptRowOf(id, a))],
        }, cancellationToken).ConfigureAwait(false);

        await WriteRecordsAndNotesAsync(id, goal.Records, goal.Notes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>The attempt row of <paramref name="attempt"/>, with its ending's date, outcome and trimmed note; all three null while it is not ended.</summary>
    private static GoalAttemptRow AttemptRowOf(Guid goalId, DemoAttempt attempt) => new()
    {
        GoalId = goalId,
        Number = attempt.Number,
        StartDate = attempt.StartDate,
        EndingDate = attempt.Ending?.Date,
        Outcome = attempt.Ending?.Outcome,
        EndingNote = attempt.Ending?.Note?.Value,
    };

    /// <summary>The version rows of every attempt of <paramref name="goal"/>, each under its attempt's number, in the profile's zone.</summary>
    private static List<GoalSettingsVersionRow> VersionRowsOf(Guid goalId, DemoGoalChangeGoal goal, DemoAccountProfile profile) =>
    [
        .. goal.AttemptsIn(profile.TimeZone)
            .SelectMany(a => a.Versions.Select(v => GoalMapping.ToRow(goalId, a.Attempt.Number, v))),
    ];

    /// <summary>
    /// The goal's records, one save each in list order, so the identity column keeps that order; then its notes, one
    /// save each, so a second note under the same entry key is a primary-key fault of the database.
    /// </summary>
    private async Task WriteRecordsAndNotesAsync(
        Guid goalId, IReadOnlyList<DemoRecord> records, IReadOnlyList<DemoNote> notes, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            await SaveAsync(RecordMapping.ToRow(goalId, record.Date, record.Value), cancellationToken).ConfigureAwait(false);
        }

        foreach (var note in notes)
        {
            await SaveAsync(RecordMapping.ToRow(goalId, note.EntryDate, note.Text), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// The goal's excusals, one save each, so a second excusal on the same date is a primary-key fault of the database;
    /// then its pauses, one save each, mapped by <see cref="ExemptionMapping"/> and stored directly as facts (FR-025).
    /// </summary>
    private async Task WriteExemptionsAsync(Guid goalId, DemoPauseGoal goal, CancellationToken cancellationToken)
    {
        foreach (var excusal in goal.Excusals)
        {
            await SaveAsync(ExemptionMapping.ToRow(goalId, excusal.Date, excusal.Reason), cancellationToken).ConfigureAwait(false);
        }

        foreach (var pause in goal.Pauses)
        {
            await SaveAsync(ExemptionMapping.ToRow(goalId, pause.FirstDay, pause.LastDay, pause.Reason), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>Adds one row and saves it alone, then clears the change tracker.</summary>
    private async Task SaveAsync<TRow>(TRow row, CancellationToken cancellationToken)
        where TRow : class
    {
        _db.Add(row);
        await WriteAsync(() => _db.SaveChangesAsync(cancellationToken)).ConfigureAwait(false);
        _db.ChangeTracker.Clear();
    }

    /// <summary>A database fault becomes an exception that names the SQL error number only, with no inner exception (HP-006).</summary>
    private async Task<T> WriteAsync<T>(Func<Task<T>> write)
    {
        int number;
        try
        {
            return await write().ConfigureAwait(false);
        }
        catch (SqlException exception)
        {
            number = exception.Number;
        }
        catch (DbUpdateException exception)
        {
            number = exception.InnerException is SqlException sql ? sql.Number : 0;
        }
        catch (DbException)
        {
            number = 0;
        }

        throw new InvalidOperationException($"{_failure}: a demo goal could not be written (SQL error {number}).");
    }
}
