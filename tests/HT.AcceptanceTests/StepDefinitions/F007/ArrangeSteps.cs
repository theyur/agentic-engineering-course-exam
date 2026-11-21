using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F007;
using HT.Application.Accounts;
using HT.Domain.Goals;

namespace HT.AcceptanceTests.StepDefinitions.F007;

/// <summary>The Given steps of F007 that arrange accounts, habits, goals, records, notes, time zones and earlier reads.</summary>
[Binding]
[Scope(Tag = "F007")]
public sealed class ArrangeSteps(F007Context context, F007Driver driver, F007Arrange arrange)
{
    private const string D = "(" + F007Text.D + ")";
    private const string T = "(" + F007Text.T + ")";

    /// <summary>The optional "at T " prefix; the group always takes part (empty when the step gives no time).</summary>
    private const string At = "((?:at " + F007Text.T + " )?)";

    private const string Habit = "(their custom habit `[^`]+`|the catalog habit `[^`]+`)";

    private static string? TimeOf(string prefix)
    {
        var match = Regex.Match(prefix, "^at (" + F007Text.T + ") $");
        return prefix.Length == 0 ? null : match.Success ? match.Groups[1].Value : throw new InvalidOperationException($"Cannot read the time of \"{prefix}\".");
    }

    // ------------------------------------------------------------ accounts and habits

    /// <summary>"the registered user `x`( in `Z`), who created the custom habit `A`( and the custom habit `B` …)".</summary>
    [Given("^the registered user `([^`]+)`((?: in `[^`]+`)?), who created (the custom habit `.+)$")]
    public async Task GivenTheRegisteredUserWhoCreatedCustomHabits(string email, string zone, string habits)
    {
        var timeZone = zone.Length == 0 ? null : F007Text.Backticked(zone).Single();
        await arrange.RegisteredUserAsync(email, timeZone);
        var names = Regex.Matches(habits, "the custom habit `([^`]+)`").Select(match => match.Groups[1].Value).ToList();
        Assert.NotEmpty(names);
        foreach (var name in names)
        {
            await arrange.CustomHabitAsync(email, name);
        }
    }

    [Given("^the registered user `([^`]+)`$")]
    public Task GivenTheRegisteredUser(string email) => arrange.RegisteredUserAsync(email, null);

    [Given("^the administrator `([^`]+)`$")]
    public Task GivenTheAdministrator(string email) => arrange.AdministratorAsync(email);

    [Given("^the administrator `([^`]+)`, who at " + T + " created the catalog habit `([^`]+)` and published it$")]
    public async Task GivenTheAdministratorWhoCreatedAndPublishedACatalogHabit(string email, string time, string habit)
    {
        await arrange.AtAsync(time);
        await arrange.AdministratorNowAsync(email)();
        await arrange.CatalogHabitAsync(email, habit);
    }

    [Given("^" + At + "`([^`]+)` unpublished the catalog habit `([^`]+)` with confirmation$")]
    public async Task GivenTheAdministratorUnpublishedTheCatalogHabit(string time, string email, string habit)
    {
        var at = TimeOf(time);
        if (at is null)
        {
            await arrange.SetupAsync(() => arrange.UnpublishAsync(email, habit));
            return;
        }

        await arrange.AtAsync(at);
        await arrange.UnpublishAsync(email, habit);
    }

    [Given("^at " + T + " `([^`]+)` changed the time zone of their account to `([^`]+)`$")]
    public async Task GivenTheUserChangedTheTimeZone(string time, string email, string zone)
    {
        await arrange.AtAsync(time);
        var changed = await driver.ChangeTimeZoneAsync(await arrange.AccountAsync(email), zone);
        Assert.Equal(TimeZoneChangeOutcome.Changed, changed.Outcome);
    }

    // ------------------------------------------------------------ goals

    [Given("^at " + T + " `([^`]+)` created a goal on " + Habit + " with (.+) and the start date " + D + "$")]
    public async Task GivenTheUserCreatedAGoal(string time, string email, string habit, string settings, string startDate)
    {
        await arrange.AtAsync(time);
        await arrange.CreateGoalAsync(email, habit, settings, F007Text.Date(startDate));
    }

    [Given("^at " + T + " `([^`]+)` created a second goal on " + Habit + " with (.+) and the start date " + D + "$")]
    public async Task GivenTheUserCreatedASecondGoal(string time, string email, string habit, string settings, string startDate)
    {
        await arrange.AtAsync(time);
        Assert.Single(arrange.GoalsOf(email));
        await arrange.CreateGoalAsync(email, habit, settings, F007Text.Date(startDate));
    }

    /// <summary>"created, in this order, goals on their custom habit `W` with S: with the name `C` and the start date D; …" (AC-030, AC-063).</summary>
    [Given("^at " + T + " `([^`]+)` created, in this order, goals on " + Habit + " with ([^:]+): (.+)$")]
    public async Task GivenTheUserCreatedGoalsInThisOrderWithSettings(string time, string email, string habit, string settings, string items)
    {
        await arrange.AtAsync(time);
        foreach (var item in items.Split("; "))
        {
            var match = Regex.Match(item.Trim(), "^with the name `([^`]+)` and the start date " + D + "$");
            Assert.True(match.Success, $"Cannot read the goal \"{item}\".");
            await arrange.CreateGoalAsync(email, habit, $"{settings}, the name `{match.Groups[1].Value}`", F007Text.Date(match.Groups[2].Value));
        }
    }

    /// <summary>"created goals on their custom habit `W`, in this order: with S, the name `M` and the start date D; …" (AC-038).</summary>
    [Given("^at " + T + " `([^`]+)` created goals on " + Habit + ", in this order: (.+)$")]
    public async Task GivenTheUserCreatedGoalsInThisOrder(string time, string email, string habit, string items)
    {
        await arrange.AtAsync(time);
        foreach (var item in items.Split("; "))
        {
            var match = Regex.Match(item.Trim(), "^with (.+) and the start date " + D + "$");
            Assert.True(match.Success, $"Cannot read the goal \"{item}\".");
            await arrange.CreateGoalAsync(email, habit, match.Groups[1].Value, F007Text.Date(match.Groups[2].Value));
        }
    }

    // ------------------------------------------------------------ records and notes

    /// <summary>
    /// "(at T )`x` records …": the records of the phrase (at T, or with no time at the When step's time); the phrase may
    /// end with " and saves the note "N" for D" (AC-040) or " and reads the goals overview, which lists `B`, `A`, `C`
    /// in this order" (AC-063).
    /// </summary>
    [Given("^" + At + "`([^`]+)` records(,? .+)$")]
    public Task GivenTheUserRecords(string time, string email, string body)
    {
        // "… on the `Sugar` goal, at T" (AC-027): the step's time is written at its end.
        var at = TimeOf(time);
        var trailing = Regex.Match(body, ", at (" + F007Text.T + ")$");
        if (trailing.Success)
        {
            Assert.Null(at);
            at = trailing.Groups[1].Value;
            body = body[..trailing.Index];
        }

        return arrange.RecordingStepAsync(at, async () =>
        {
            var text = body;
            var overview = Regex.Match(text, " and reads the goals overview, which lists (.+) in this order$");
            if (overview.Success)
            {
                text = text[..overview.Index];
            }

            var note = Regex.Match(text, " and saves the note (\"[^\"]*\" for " + F007Text.D + ")$");
            if (note.Success)
            {
                text = text[..note.Index];
            }

            await arrange.RecordAllAsync(email, text);
            if (note.Success)
            {
                await arrange.SaveNotesAsync(email, note.Groups[1].Value);
            }

            if (overview.Success)
            {
                var read = await arrange.OverviewAsync(email);
                var expected = F007Text.Backticked(overview.Groups[1].Value).Select(name => arrange.Goal(email, $"`{name}`").Id).ToList();
                Assert.Equal(expected, read.Goals.Select(goal => goal.Item.Id).ToList());
            }
        });
    }

    /// <summary>"on the first goal `x` records 20 for D1 at T1, 12 for D2 at T2, …": each record at its own time (AC-019).</summary>
    [Given("^on the (first|second) goal `([^`]+)` records (.+)$")]
    public async Task GivenOnAGoalTheUserRecordsEachAtItsTime(string ordinal, string email, string body)
    {
        var goal = arrange.Goal(email, $"the {ordinal} goal");
        var items = Regex.Matches(body, "(`Yes`|`No`|-?\\d+) for " + D + " at " + T);
        Assert.NotEmpty(items);
        foreach (Match item in items)
        {
            await arrange.AtAsync(item.Groups[3].Value);
            var (answer, amount) = F007Text.ValueOf(item.Groups[1].Value);
            await arrange.RecordOneAsync(await arrange.AccountAsync(email), goal, F007Text.Date(item.Groups[2].Value), answer, amount);
        }
    }

    /// <summary>"at T, on the second goal, `x` records 20 for D1, 12 for D2 …" (AC-019).</summary>
    [Given("^at " + T + ", on the (first|second) goal, `([^`]+)` records (.+)$")]
    public async Task GivenAtTimeOnAGoalTheUserRecords(string time, string ordinal, string email, string body)
    {
        await arrange.AtAsync(time);
        var goal = arrange.Goal(email, $"the {ordinal} goal");
        var user = await arrange.AccountAsync(email);
        var items = F007Text.Records(body);
        Assert.NotEmpty(items);
        foreach (var item in items)
        {
            Assert.Null(item.Goal);
            await arrange.RecordOneAsync(user, goal, item.Date, item.Answer, item.Amount);
        }
    }

    [Given("^" + At + "`([^`]+)` saves(,? .+)$")]
    public Task GivenTheUserSavesNotes(string time, string email, string body) =>
        arrange.RecordingStepAsync(TimeOf(time), () => arrange.SaveNotesAsync(email, body));

    // ------------------------------------------------------------ earlier reads that the item checks

    /// <summary>"at T `x` read the report of the goal and got the consistency "…", the current streak "…" …".</summary>
    [Given("^at " + T + " `([^`]+)` read the report of the goal and got (.+)$")]
    public async Task GivenTheUserReadTheReportAndGot(string time, string email, string figures)
    {
        await arrange.AtAsync(time);
        var report = await arrange.ReportAsync(email, arrange.Goal(email, "the goal"));
        F007Checks.Figures(report.Stats, figures);
    }

    /// <summary>"at T `x` read the goal list and got the entries of `A`, `B` and `C`, in this order" (AC-063).</summary>
    [Given("^at " + T + " `([^`]+)` read the goal list and got the entries of (.+), in this order$")]
    public async Task GivenTheUserReadTheGoalList(string time, string email, string names)
    {
        await arrange.AtAsync(time);
        var list = await arrange.ListGoalsAsync(email);
        var expected = F007Text.Backticked(names).Select(name => arrange.Goal(email, $"`{name}`").Id).ToList();
        Assert.Equal(expected, list.Select(item => item.Id).ToList());
        context.EarlierGoalList = list;
    }
}
