using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.IntegrationTests.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// Short ways to arrange record data in the integration tests (slice 5), through the public ports and plain SQL. In the
/// shared database every account email starts with <see cref="EmailPrefix"/> and every habit name with
/// <see cref="HabitPrefix"/>, each with a fresh suffix, so the tests check by inclusion and never by totals.
/// </summary>
public static class RecordSamples
{
    public const string EmailPrefix = "it-rec-";
    public const string HabitPrefix = "It-Rec ";

    /// <summary>Wednesday 18 November 2026, the start date of the sample goals.</summary>
    public static readonly DateOnly Start = new(2026, 11, 18);

    /// <summary>A fresh suffix for one test's emails and habit names.</summary>
    public static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>A registered user with the email <c>it-rec-{name}-{suffix}@example.com</c>, in UTC.</summary>
    public static async Task<AccountId> OwnerAsync(DatabaseFixture database, string name, string suffix)
    {
        ArgumentNullException.ThrowIfNull(database);
        var user = await HabitAccounts.RegisteredUserAsync(
            database.Services, database.ConnectionString, $"{EmailPrefix}{name}-{suffix}@example.com");
        return user.AccountId!.Value;
    }

    /// <summary>
    /// A goal of <paramref name="owner"/> on a new Published catalog habit named <c>It-Rec {name} {suffix}</c>, with
    /// <paramref name="target"/>, starting on <paramref name="start"/> (default <see cref="Start"/>) in
    /// <paramref name="zone"/> (default UTC). The account's zone is set first, so the store accepts the expected zone.
    /// </summary>
    public static async Task<GoalId> GoalAsync(
        DatabaseFixture database, AccountId owner, string name, string suffix, GoalTarget target, DateOnly? start = null, string zone = "UTC")
    {
        ArgumentNullException.ThrowIfNull(database);
        var habit = await CatalogHabitAsync(database.Services, $"{HabitPrefix}{name} {suffix}");
        await SetZoneAsync(database.ConnectionString, owner, zone);
        await using var scope = database.CreateScope();
        var outcome = await scope.ServiceProvider.GetRequiredService<IGoalStore>().CreateAsync(
            owner,
            new NewGoal(new GoalHabit(habit, HabitKind.Catalog), start ?? Start, null, target),
            Zone(zone),
            Zone(zone),
            CancellationToken.None);
        Assert.Equal(GoalWriteOutcomeKind.Created, outcome.Kind);
        return outcome.Id!.Value;
    }

    /// <summary>Build, duration, at least 20 min, every day: commitment-based.</summary>
    public static GoalTarget Reading20MinDaily() => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    });

    /// <summary>Build, yes/no, Monday, Wednesday and Friday: commitment-based.</summary>
    public static GoalTarget YogaMonWedFri() => YesNoOn(DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday);

    /// <summary>Build, count, at least 20 pages, 3 times a week: period-based, per week.</summary>
    public static GoalTarget Pages20ThreeTimesAWeek() => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 20, CountUnit = "pages",
        ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    });

    /// <summary>Reduce, count, at most 2 cups a day: period-based, per day.</summary>
    public static GoalTarget CoffeeAtMost2Cups() => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "cups",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    });

    /// <summary>The entry of one date: a commitment or a day, keyed by that date.</summary>
    public static EntrySpan Day(DateOnly date) => new(date, date, date);

    /// <summary>The entry of the week that starts on <paramref name="monday"/>, keyed by it.</summary>
    public static EntrySpan Week(DateOnly monday) => new(monday, monday, monday.AddDays(6));

    public static RecordValue Minutes(int minutes) => RecordValue.FromAmount(minutes);

    public static RecordValue Yes() => RecordValue.FromAnswer(YesNoAnswer.Yes);

    public static RecordValue No() => RecordValue.FromAnswer(YesNoAnswer.No);

    /// <summary>The goal's <c>GoalRecords</c> rows in the stored order, each as column name to value.</summary>
    public static Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> RecordRowsAsync(string connectionString, GoalId goal) =>
        Sql.QueryAsync(connectionString,
            "SELECT [Id], [GoalId], [Date], [Answer], [Amount], [Sequence] FROM [GoalRecords] WHERE [GoalId] = @goal ORDER BY [Date], [Sequence]",
            ("@goal", goal.Value));

    /// <summary>The goal's <c>GoalNotes</c> rows by entry date, each as column name to value.</summary>
    public static Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> NoteRowsAsync(string connectionString, GoalId goal) =>
        Sql.QueryAsync(connectionString,
            "SELECT [GoalId], [EntryDate], [Text] FROM [GoalNotes] WHERE [GoalId] = @goal ORDER BY [EntryDate]",
            ("@goal", goal.Value));
}
