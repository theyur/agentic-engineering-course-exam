using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Time;

namespace HT.Application.Accounts;

/// <summary>
/// The acting user's own profile (FR-024 to FR-027; research R-04, R-07, R-08): any signed-in account, registered user
/// or administrator, on its own account only. No method takes an account identifier (constitution III, US10-AS9). An
/// anonymous visitor, and an acting user whose account is not stored (deleted after sign-in, or a stale cookie), get
/// "You are not signed in.", as in <see cref="GoalService"/>. Results carry texts only, never an input value (HP-011).
/// </summary>
public sealed class ProfileService
{
    /// <summary>How many times a time-zone change is written while other changes of the account keep committing (research R-08).</summary>
    public const int MaxTimeZoneWrites = 10;

    private readonly IAccountStore _accounts;
    private readonly IGoalStore _goals;
    private readonly TimeProvider _clock;
    private readonly InstallationTimeZone _defaultZone;

    public ProfileService(IAccountStore accounts, IGoalStore goals, TimeProvider clock, InstallationTimeZone defaultZone)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(defaultZone);
        _accounts = accounts;
        _goals = goals;
        _clock = clock;
        _defaultZone = defaultZone;
    }

    /// <summary>
    /// The acting user's time zone (FR-024, FR-026): the stored named time zone exactly as given, or the installation's
    /// default for an account that has none stored (FR-025, research R-04).
    /// </summary>
    public async Task<TimeZoneReadResult> GetTimeZoneAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (await FindActingAccountAsync(actingUser, cancellationToken).ConfigureAwait(false) is not { } account)
        {
            return TimeZoneReadResult.NotSignedIn;
        }

        return TimeZoneReadResult.Found(ZoneOf(account.StoredTimeZone).Id);
    }

    /// <summary>
    /// Changes the acting user's time zone (FR-024, FR-026, FR-027), checking in order: signed in with a stored account,
    /// then a named time zone ("Choose a time zone from the list."). The account's own zone is a success that changes
    /// nothing and computes nothing. Otherwise each of the account's goals follows on its own timeline from its current
    /// date at the supplied time (<see cref="GoalTimeline.ChangeOf"/>), and the account's zone and every goal's versions
    /// are written together, only if the stored zone and the goal revision are still the ones read. When another change
    /// of the account committed in between, everything is read and computed again, at most
    /// <see cref="MaxTimeZoneWrites"/> times (research R-08).
    /// </summary>
    /// <exception cref="InvalidOperationException">Every write lost to another change. The message names no value.</exception>
    public async Task<TimeZoneChangeResult> ChangeTimeZoneAsync(
        ActingUser actingUser, string? timeZone, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (await FindActingAccountAsync(actingUser, cancellationToken).ConfigureAwait(false) is not { } account)
        {
            return TimeZoneChangeResult.NotSignedIn;
        }

        if (!NamedTimeZone.TryParse(timeZone, out var next))
        {
            return TimeZoneChangeResult.NotANamedTimeZone;
        }

        if (ZoneOf(account.StoredTimeZone) == next)
        {
            return TimeZoneChangeResult.Unchanged;
        }

        for (var write = 1; write <= MaxTimeZoneWrites; write++)
        {
            if (await _goals.ReadOwnerGoalsAsync(account.Id, cancellationToken).ConfigureAwait(false) is not { } owned)
            {
                return TimeZoneChangeResult.NotSignedIn;
            }

            if (ZoneOf(owned.StoredTimeZone) == next)
            {
                return TimeZoneChangeResult.Unchanged;
            }

            var now = _clock.GetUtcNow();
            var writes = owned.Goals
                .Select(stored => new GoalVersionWrite(stored.Goal.Id, GoalTimeline.ChangeOf(stored.Goal, now, next)))
                .Where(change => change.Change.Kind != GoalVersionChangeKind.None)
                .ToList();
            if (await _goals.TryChangeTimeZoneAsync(
                    account.Id, owned.StoredTimeZone, owned.GoalsRevision, next, writes, cancellationToken).ConfigureAwait(false))
            {
                return TimeZoneChangeResult.Changed;
            }
        }

        throw new InvalidOperationException("The account's goals kept changing while its time zone was changed.");
    }

    /// <summary>The acting user's own account, or null for the anonymous visitor or an account that is not stored.</summary>
    private async Task<Account?> FindActingAccountAsync(ActingUser actingUser, CancellationToken cancellationToken) =>
        actingUser.AccountId is { } id ? await _accounts.FindByIdAsync(id, cancellationToken).ConfigureAwait(false) : null;

    /// <summary>A stored zone, or the installation's default when none is stored (FR-025, research R-04).</summary>
    private NamedTimeZone ZoneOf(string? storedTimeZone) =>
        NamedTimeZone.TryParse(storedTimeZone, out var zone) ? zone : _defaultZone.Value;
}
