using System.Net;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// US3-AS1, FR-020, HP-011 through the real host: a registered user who opens another account's <c>/goals/{id}</c> gets
/// the Not found page with 404, the same page as for an identifier that belongs to nothing. The response contains
/// neither the goal's name nor its habit's name, and no host log entry contains the private ones: the goal's own name
/// and the custom habit's name. The owner has a goal with its own name on a catalog habit and a goal with the derived
/// name on a custom habit. Slice 5 (US6, FR-017, HP-006): another registered user and an administrator who open the
/// page of a goal with records and notes get the same Not found page with 404, and its HTML holds none of the owner's
/// notes or recorded values. Slice 6 (US7, FR-014, FR-015): that Not found page has no GoalStats and no trend, B's Goals
/// page and Home hold none of A's goal names, and an administrator's Home has no tile and no row. Slice 7 (US5, FR-021,
/// FR-022, HP-009): that Not found page has no Pause, Notice, Pauses card, reason or Excused row of A's paused goal, B's
/// Paused tab lists none of A's goals, an administrator gets the Not found page for it, and no host log entry holds a
/// reason. Shared database: own prefixes ("ui-gp-", "Ui-Gp").
/// </summary>
[Collection(HostCollection.Name)]
public sealed class GoalPrivacyHostTests(HostFixture host)
{
    private static string Email(string who) => $"ui-gp-{who}-{Guid.NewGuid():N}@example.com";

    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..30];

    private async Task<GoalId> CreateAsync(AccountId owner, GoalHabit habit, string? name)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
        var created = await services.GetRequiredService<GoalService>().CreateAsync(ActingUser.ForAccount(owner, Role.RegisteredUser), new GoalInput
        {
            Habit = habit,
            Target = new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
            },
            StartDate = today,
            Name = name,
        }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    [Fact]
    public async Task US3_AS1_another_accounts_goal_page_is_Not_found_and_names_neither_the_goal_nor_its_habit()
    {
        var catalogName = Unique("Ui-Gp Reading");
        var customName = Unique("Ui-Gp Ritual");
        var goalName = Unique("Ui-Gp Private goal");
        var catalog = await host.ArrangeCatalogHabitAsync(Definition(catalogName));
        var (ownerBrowser, owner) = await host.SignedInAccountAsync(Email("owner"), Role.RegisteredUser);
        ownerBrowser.Dispose();
        var custom = await host.ArrangeCustomHabitAsync(owner, Definition(customName, Category.Body));
        var named = await CreateAsync(owner, new GoalHabit(catalog, HabitKind.Catalog), goalName);
        var derived = await CreateAsync(owner, new GoalHabit(custom, HabitKind.Custom), null);
        using var browser = await host.SignedInBrowserAsync(Email("other"), Role.RegisteredUser);

        var (unknownStatus, unknownHtml) = await browser.GetPageAsync($"/goals/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, unknownStatus);
        Assert.Contains("Page not found", unknownHtml, StringComparison.Ordinal);

        foreach (var goal in new[] { named, derived })
        {
            var (status, html) = await browser.GetPageAsync($"/goals/{goal.Value}");

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(goalName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(catalogName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(customName, html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(host.Logs.Entries, entry =>
            new[] { goalName, customName }.Any(value =>
                entry.Message.Contains(value, StringComparison.OrdinalIgnoreCase)
                || (entry.Exception?.ToString().Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    [Fact]
    public async Task US6_another_registered_user_and_an_administrator_get_Not_found_for_As_goal_page_with_none_of_its_notes_or_values()
    {
        // The goal starts 2 days before today (UTC, at the host's clock), every day, 20 min: its first day has 47 min and
        // a note, its second day 38 min and another note. The owner's own page shows both, so their absence is the refusal.
        var habitName = Unique("Ui-Gp Records");
        var goalName = Unique("Ui-Gp Private record goal");
        var firstNote = $"Uigpnotemark first {Guid.NewGuid():N}";
        var secondNote = $"Uigpnotemark second {Guid.NewGuid():N}";
        var habit = await host.ArrangeCatalogHabitAsync(Definition(habitName));
        var (ownerBrowser, owner) = await host.SignedInAccountAsync(Email("records-owner"), Role.RegisteredUser);
        GoalId goal;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var first = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()).AddDays(-2);
            Assert.True(GoalTarget.TryCreate(new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
                ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
            }, out var target, out _));
            Assert.True(GoalName.TryCreate(goalName, out var name, out _));
            var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
                owner, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, name, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
            Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
            goal = created.Id!.Value;
            var records = services.GetRequiredService<IRecordStore>();
            foreach (var (date, minutes, text) in new[] { (first, 47, firstNote), (first.AddDays(1), 38, secondNote) })
            {
                Assert.True(NoteText.TryCreate(text, out var note, out _));
                var written = await records.AddAsync(
                    owner, goal, new EntrySpan(date, date, date), true, date, RecordValue.FromAmount(minutes), NoteWrite.Replace(note!), RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
                Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
            }
        }

        var address = $"/goals/{goal.Value}";
        using (ownerBrowser)
        {
            var (status, html) = await ownerBrowser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains(firstNote, html, StringComparison.Ordinal);
            Assert.Contains(secondNote, html, StringComparison.Ordinal);
            Assert.Contains("<td>47 min</td>", html, StringComparison.Ordinal);
            Assert.Contains("<td>38 min</td>", html, StringComparison.Ordinal);
        }

        using var other = await host.SignedInBrowserAsync(Email("records-other"), Role.RegisteredUser);
        using var administrator = await host.SignedInBrowserAsync(Email("records-admin"), Role.Administrator);
        foreach (var browser in new[] { other, administrator })
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Uigpnotemark", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("47 min", html, StringComparison.Ordinal);
            Assert.DoesNotContain("38 min", html, StringComparison.Ordinal);
            Assert.DoesNotContain(">History</h2>", html, StringComparison.Ordinal);
            Assert.DoesNotContain(goalName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(habitName, html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(host.Logs.Entries, entry =>
            new[] { "Uigpnotemark", goalName }.Any(value =>
                entry.Message.Contains(value, StringComparison.OrdinalIgnoreCase)
                || (entry.Exception?.ToString().Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    [Fact]
    public async Task Slice_6_US7_reports_of_As_goal_reach_neither_B_nor_an_administrator()
    {
        // A's goal and B's goal start 2 days before today (UTC, at the host's clock), every day, 20 min, each with 25 min on
        // its first day, so each owner's goal page has the GoalStats and TrendChart cards and each Home has tiles and rows.
        var aGoalName = Unique("Ui-Gp A report goal");
        var bGoalName = Unique("Ui-Gp B report goal");
        var habitName = Unique("Ui-Gp Reports");
        var habit = await host.ArrangeCatalogHabitAsync(Definition(habitName));
        var (aBrowser, a) = await host.SignedInAccountAsync(Email("reports-a"), Role.RegisteredUser);
        var (bBrowser, b) = await host.SignedInAccountAsync(Email("reports-b"), Role.RegisteredUser);
        GoalId aGoal;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var first = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()).AddDays(-2);
            aGoal = await StoreGoalWithARecordAsync(services, a, habit, aGoalName, first);
            await StoreGoalWithARecordAsync(services, b, habit, bGoalName, first);
        }

        var address = $"/goals/{aGoal.Value}";
        using (aBrowser)
        {
            var (status, html) = await aBrowser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains("<div class=\"ht-stats\">", html, StringComparison.Ordinal);
            Assert.Contains("class=\"ht-bars\"", html, StringComparison.Ordinal);
        }

        using var administrator = await host.SignedInBrowserAsync(Email("reports-admin"), Role.Administrator);
        using (bBrowser)
        {
            // Another account's goal page is still the Not found page, with no GoalStats and no trend.
            foreach (var browser in new[] { bBrowser, administrator })
            {
                var (status, html) = await browser.GetPageAsync(address);

                Assert.Equal(HttpStatusCode.NotFound, status);
                Assert.Contains("Page not found", html, StringComparison.Ordinal);
                Assert.DoesNotContain("ht-stats", html, StringComparison.Ordinal);
                Assert.DoesNotContain("ht-bars", html, StringComparison.Ordinal);
                Assert.DoesNotContain(aGoalName, html, StringComparison.OrdinalIgnoreCase);
            }

            // B's Goals page and Home hold B's goal and none of A's.
            foreach (var page in new[] { "/goals", "/" })
            {
                var (status, html) = await bBrowser.GetPageAsync(page);

                Assert.Equal(HttpStatusCode.OK, status);
                Assert.Contains(bGoalName, html, StringComparison.Ordinal);
                Assert.DoesNotContain(aGoalName, html, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain($"{aGoal.Value}", html, StringComparison.OrdinalIgnoreCase);
            }
        }

        // An administrator's Home has no tile and no row.
        var (homeStatus, home) = await administrator.GetPageAsync("/");
        Assert.Equal(HttpStatusCode.OK, homeStatus);
        Assert.DoesNotContain("ht-tile", home, StringComparison.Ordinal);
        Assert.DoesNotContain("<div class=\"ht-row\">", home, StringComparison.Ordinal);
        Assert.DoesNotContain(aGoalName, home, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Slice_7_US5_excusals_and_pauses_of_As_goal_reach_neither_B_nor_an_administrator()
    {
        // A's goal and B's goal start yesterday (UTC, at the host's clock, read once here; the tests of the collection run
        // one at a time, so it does not move during this test), every day, 20 min, with no record. Through the host's
        // services A excuses A's yesterday (Pending) with a reason and pauses A's goal from today for 7 days with another;
        // B pauses B's goal, on a habit of its own, likewise, so B's Paused tab has a goal of its own. A's own page shows them all, so their
        // absence is the refusal.
        var aGoalName = Unique("Ui-Gp A paused goal");
        var bGoalName = Unique("Ui-Gp B paused goal");
        var habitName = Unique("Ui-Gp Pauses");
        var excusalReason = $"Uigpreasonmark excusal {Guid.NewGuid():N}";
        var pauseReason = $"Uigpreasonmark pause {Guid.NewGuid():N}";
        var habit = await host.ArrangeCatalogHabitAsync(Definition(habitName));
        var bHabit = await host.ArrangeCatalogHabitAsync(Definition(Unique("Ui-Gp B pauses")));
        var (aBrowser, a) = await host.SignedInAccountAsync(Email("pauses-a"), Role.RegisteredUser);
        var (bBrowser, b) = await host.SignedInAccountAsync(Email("pauses-b"), Role.RegisteredUser);
        GoalId aGoal;
        GoalId bGoal;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
            aGoal = await StoreGoalAsync(services, a, habit, aGoalName, today.AddDays(-1));
            bGoal = await StoreGoalAsync(services, b, bHabit, bGoalName, today.AddDays(-1));
            var excused = await services.GetRequiredService<ExcusalService>().ExcuseAsync(
                ActingUser.ForAccount(a, Role.RegisteredUser), aGoal, today.AddDays(-1), excusalReason, CancellationToken.None);
            Assert.Equal(ExcuseOutcome.Excused, excused.Outcome);
            var pauses = services.GetRequiredService<PauseService>();
            foreach (var (owner, goal, reason) in new[] { (a, aGoal, pauseReason), (b, bGoal, "B's own reason") })
            {
                var paused = await pauses.PauseAsync(
                    ActingUser.ForAccount(owner, Role.RegisteredUser), goal, today, today.AddDays(6), reason, CancellationToken.None);
                Assert.Equal(PauseOutcome.Paused, paused.Outcome);
            }
        }

        var address = $"/goals/{aGoal.Value}";
        using (aBrowser)
        {
            var (status, html) = await aBrowser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains("<div class=\"ht-notice\">", html, StringComparison.Ordinal);
            Assert.Contains("<h2 class=\"ht-card__title\">Pauses</h2>", html, StringComparison.Ordinal);
            Assert.Contains(pauseReason, html, StringComparison.Ordinal);
            Assert.Contains(excusalReason, html, StringComparison.Ordinal);
            Assert.Contains("ht-status--excused", html, StringComparison.Ordinal);
            Assert.Contains("ht-status--paused", html, StringComparison.Ordinal);

            var (tabStatus, tab) = await aBrowser.GetPageAsync("/goals?tab=paused");
            Assert.Equal(HttpStatusCode.OK, tabStatus);
            Assert.Contains($"href=\"/goals/{aGoal.Value}\"", tab, StringComparison.Ordinal);
        }

        using var administrator = await host.SignedInBrowserAsync(Email("pauses-admin"), Role.Administrator);
        using (bBrowser)
        {
            // A's goal page is the Not found page for B and for an administrator, with nothing of the pause or the excusal.
            foreach (var browser in new[] { bBrowser, administrator })
            {
                var (status, html) = await browser.GetPageAsync(address);

                Assert.Equal(HttpStatusCode.NotFound, status);
                Assert.Contains("Page not found", html, StringComparison.Ordinal);
                AssertNothingOfAsPausedGoal(html);
                Assert.DoesNotContain("<div class=\"ht-notice\">", html, StringComparison.Ordinal);
                Assert.DoesNotContain(">Pause</button>", html, StringComparison.Ordinal);
                Assert.DoesNotContain(">Resume now</button>", html, StringComparison.Ordinal);
                Assert.DoesNotContain("Pauses</h2>", html, StringComparison.Ordinal);
                Assert.DoesNotContain("ht-status--paused", html, StringComparison.Ordinal);
            }

            // B's Paused tab lists B's paused goal and none of A's.
            var (tabStatus, tab) = await bBrowser.GetPageAsync("/goals?tab=paused");
            Assert.Equal(HttpStatusCode.OK, tabStatus);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals?tab=paused\" aria-current=\"page\">Paused</a>", tab, StringComparison.Ordinal);
            Assert.Contains($"href=\"/goals/{bGoal.Value}\"", tab, StringComparison.Ordinal);
            Assert.Contains(bGoalName, tab, StringComparison.Ordinal);
            AssertNothingOfAsPausedGoal(tab);
        }

        // An administrator gets the Not found page for the Paused tab.
        var (adminTabStatus, adminTab) = await administrator.GetPageAsync("/goals?tab=paused");
        Assert.Equal(HttpStatusCode.NotFound, adminTabStatus);
        Assert.Contains("Page not found", adminTab, StringComparison.Ordinal);
        Assert.DoesNotContain(bGoalName, adminTab, StringComparison.OrdinalIgnoreCase);
        AssertNothingOfAsPausedGoal(adminTab);

        Assert.DoesNotContain(host.Logs.Entries, entry =>
            new[] { "Uigpreasonmark", aGoalName }.Any(value =>
                entry.Message.Contains(value, StringComparison.OrdinalIgnoreCase)
                || (entry.Exception?.ToString().Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)));

        void AssertNothingOfAsPausedGoal(string html)
        {
            Assert.DoesNotContain("Uigpreasonmark", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(aGoalName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(habitName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain($"{aGoal.Value}", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ht-status--excused", html, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// A goal of at least 20 min every day of <paramref name="owner"/> with its own <paramref name="name"/>, stored through
    /// the host's goal store from <paramref name="first"/> in UTC, with no record.
    /// </summary>
    private static async Task<GoalId> StoreGoalAsync(IServiceProvider services, AccountId owner, HabitId habit, string name, DateOnly first)
    {
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, out var target, out _));
        Assert.True(GoalName.TryCreate(name, out var goalName, out _));
        var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
            owner, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, goalName, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
        Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
        return created.Id!.Value;
    }

    /// <summary>
    /// A goal of at least 20 min every day of <paramref name="owner"/> with its own <paramref name="name"/>, stored through
    /// the host's goal store from <paramref name="first"/> in UTC, with 25 min recorded on that day.
    /// </summary>
    private static async Task<GoalId> StoreGoalWithARecordAsync(IServiceProvider services, AccountId owner, HabitId habit, string name, DateOnly first)
    {
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, out var target, out _));
        Assert.True(GoalName.TryCreate(name, out var goalName, out _));
        var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
            owner, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, goalName, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
        Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
        var goal = created.Id!.Value;
        var written = await services.GetRequiredService<IRecordStore>().AddAsync(
            owner, goal, new EntrySpan(first, first, first), true, first, RecordValue.FromAmount(25), NoteWrite.Keep,
            RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
        Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
        return goal;
    }
}
