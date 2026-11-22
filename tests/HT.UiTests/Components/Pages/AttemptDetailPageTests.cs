using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.UiTests.Components.Pages.MeditationAttempts;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 8 (T092; HP-014, FR-010, FR-014, US3-AS4, AS11, AS15, SC-008, contracts/host.md "Attempt view"): the attempt
/// view over the real GoalService, AttemptService, RecordingService, ExcusalService and the fakes, for attempt 1 of
/// "Meditation, 10 min daily" (<see cref="MeditationAttempts"/>: 10 minutes on 2 to 13 November, ended on Monday 16
/// November with "Abandoned" and a note, so 14 and 15 November are Missed once final). The user has no stored zone: UTC.
/// </summary>
public sealed class AttemptDetailPageTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeRecordStore _records;
    private int _notFound;

    public AttemptDetailPageTests()
    {
        _authorization = AddAuthorization();
        this.AddCatalog();
        this.AddCustomHabits();
        this.AddGoals();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        Services.GetRequiredService<BunitNavigationManager>().OnNotFound += (_, _) => _notFound++;
    }

    private FakeTimeProvider Clock => (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();

    private ActingUser SignIn(string role = "RegisteredUser") =>
        ActingUser.ForAccount(new AccountId(_authorization.SignIn(role)), role == "Administrator" ? Role.Administrator : Role.RegisteredUser);

    private IRenderedComponent<AttemptDetail> RenderAttempt(GoalId goal, int number) =>
        Render<AttemptDetail>(p => p.Add(x => x.Id, goal.Value).Add(x => x.Number, number));

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static IElement HistoryRow(IRenderedComponent<AttemptDetail> cut, string date) =>
        cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == date);

    private static List<string> HistoryActions(IRenderedComponent<AttemptDetail> cut, string date) =>
        HistoryRow(cut, date).QuerySelectorAll("td.ht-actions > button").Select(b => b.TextContent).ToList();

    private static List<string> Stats(IRenderedComponent<AttemptDetail> cut) =>
        cut.FindAll(".ht-stats .ht-stat__value").Select(v => v.TextContent).ToList();

    private string LastFocusedId(int count) =>
        Assert.IsType<ElementReference>(JSInterop.VerifyFocusAsyncInvoke(count)[^1].Arguments[0]).Id;

    [Fact]
    public async Task HP_014_US3_AS11_the_attempt_view_shows_the_head_the_figures_and_the_History_of_attempt_1_only()
    {
        var user = SignIn();
        var goal = await RestartedAsync(this, user, Utc(23, 12));

        var cut = RenderAttempt(goal, 1);

        Assert.Equal(0, _notFound);
        var back = cut.Find("a.ht-back");
        Assert.Equal(("← " + Name, $"/goals/{goal.Value}"), (back.TextContent, back.GetAttribute("href")));
        Assert.Equal("Attempt 1 · 2 Nov – 15 Nov", cut.Find(".ht-page-head h1").TextContent);
        var tags = cut.Find(".ht-page-head .ht-inline.ht-head-tags");
        Assert.Equal([("ht-attempt", Name), ("ht-chip", "Abandoned")], tags.Children.Select(c => (c.ClassName!, c.TextContent)));
        Assert.Equal(Note, cut.Find(".ht-page-head div.ht-muted.ht-head-tags").TextContent);
        Assert.Empty(cut.FindAll(".ht-page-head .ht-btn"));
        Assert.Empty(cut.FindAll(".ht-page-head .ht-spacer"));

        // One column: GoalStats with the eyebrow "Attempt 1", then History over 2 to 15 November, newest first.
        var column = cut.Find(".ht-container > .ht-stack");
        Assert.Equal(["Consistency and streaks", "History"], column.QuerySelectorAll(".ht-card__title").Select(Text));
        Assert.Equal("Attempt 1", cut.Find(".ht-eyebrow").TextContent);
        Assert.Equal(["85% (12 of 14)", "0", "12"], Stats(cut));
        Assert.Equal(["Consistency", "Streak, commitments", "Longest, commitments"], cut.FindAll(".ht-stat__label").Select(l => l.TextContent));
        Assert.Equal("Last 14 commitments: 12 met, 2 missed", cut.Find(".ht-strip").GetAttribute("aria-label"));
        var dates = cut.FindAll("table[aria-label=History] tbody tr").Select(r => Text(r.QuerySelector("td")!)).ToList();
        Assert.Equal(["Sun 15 Nov", "Sat 14 Nov", "Fri 13 Nov"], dates.Take(3));
        Assert.Equal("Missed", Text(HistoryRow(cut, "Sat 14 Nov").QuerySelectorAll("td")[2]));
        Assert.Equal("10 min", Text(HistoryRow(cut, "Fri 13 Nov").QuerySelectorAll("td")[1]));
        Assert.Contains("Show all 14", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Mon 16 Nov", cut.Markup, StringComparison.Ordinal);

        // HP-014: none of the goal page's other parts.
        Assert.Empty(cut.FindAll(".ht-grid-2"));
        Assert.Empty(cut.FindAll("dl.ht-kv"));
        var buttons = cut.FindAll("button").Select(b => b.TextContent.Trim()).ToList();
        foreach (var absent in new[] { "Record", "Edit settings", "Pause", "End goal", "Resume now", "Start new attempt" })
        {
            Assert.DoesNotContain(absent, buttons);
        }

        foreach (var absent in new[] { "Edit settings", "End goal", "Recent notes", "Earlier attempts", "Pauses", "Settings", "trend" })
        {
            Assert.DoesNotContain(">" + absent + "<", cut.Markup, StringComparison.Ordinal);
        }

        Assert.Empty(cut.FindAll(".ht-trend"));
    }

    [Fact]
    public async Task US3_AS15_US3_AS4_Add_record_on_14_November_opens_the_RecordDialog_with_attempt_1s_settings_and_the_save_counts_in_attempt_1()
    {
        // US3-AS4: attempt 2 starts on Monday 23 November with 5 minutes; read on Thursday 19 November at 10:00.
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        await StartAttemptAsync(this, user, goal, Utc(18, 8), MinutesDaily(5), Mon23);
        Clock.SetUtcNow(Utc(19, 10));
        var cut = RenderAttempt(goal, 1);
        Assert.Equal("← Meditation, 5 min daily", cut.Find("a.ht-back").TextContent);
        Assert.Equal(Name, cut.Find(".ht-attempt").TextContent);
        var addRecord = HistoryRow(cut, "Sat 14 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(("Add record", "Add record for Sat 14 Nov"), (addRecord.TextContent, addRecord.GetAttribute("aria-label")));
        var recordControlId = addRecord.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(recordControlId));

        await addRecord.ClickAsync(new MouseEventArgs());

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · " + Name, dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Build · at least 10 min · every day", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Contains("Sat 14 Nov", Text(dialog), StringComparison.Ordinal);
        Assert.Empty(dialog.QuerySelectorAll("select"));

        cut.Find("[role=dialog] .ht-stepper input").Change("10");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Contains(_records.RecordsOfGoal(goal), r => r.Date == Sat14);
        // 2 to 14 November are now 13 Met commitments in a row; Sunday 15 stays Missed.
        Assert.Equal(["92% (13 of 14)", "0", "13"], Stats(cut));
        Assert.Equal("Met", Text(HistoryRow(cut, "Sat 14 Nov").QuerySelectorAll("td")[2]));
        // The row's record control is now "Edit", the same element, where the focus goes back (HP-009).
        Assert.Equal("Edit", HistoryRow(cut, "Sat 14 Nov").QuerySelector("td.ht-actions > button:last-of-type")!.TextContent);
        Assert.Equal(recordControlId, LastFocusedId(2));
    }

    [Fact]
    public async Task HP_009_cancelling_the_RecordDialog_gives_the_focus_back_to_the_rows_Add_record()
    {
        var user = SignIn();
        var goal = await RestartedAsync(this, user, Utc(23, 12));
        var cut = RenderAttempt(goal, 1);
        var addRecord = HistoryRow(cut, "Sat 14 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        var addRecordId = addRecord.GetAttribute("blazor:elementReference");

        await addRecord.ClickAsync(new MouseEventArgs());
        await cut.FindAll(".ht-dialog__foot .ht-btn")[1].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.DoesNotContain(_records.RecordsOfGoal(goal), r => r.Date == Sat14);
        Assert.Equal(addRecordId, LastFocusedId(2));
    }

    [Fact]
    public async Task HP_014_Add_note_opens_the_NoteDialog_fixed_to_its_row_and_the_note_shows_after_the_save()
    {
        var user = SignIn();
        var goal = await RestartedAsync(this, user, Utc(23, 12));
        var cut = RenderAttempt(goal, 1);
        var addNote = HistoryRow(cut, "Sat 14 Nov").QuerySelector("td.ht-actions > button")!;
        Assert.Equal("Add note", addNote.TextContent);
        // The note control stays the same element when the save turns it into "Edit note", so the focus goes back to it.
        var noteId = addNote.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(noteId));

        await addNote.ClickAsync(new MouseEventArgs());
        var dialog = cut.Find("[role=dialog]");
        Assert.Contains(Name, dialog.QuerySelector("h2")!.TextContent, StringComparison.Ordinal);
        Assert.Equal("Sat 14 Nov · Missed", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        cut.Find("[role=dialog] textarea").Input("Travelling.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Travelling.", _records.NotesOfGoal(goal)[Sat14].Value);
        Assert.Equal("Travelling.", Text(HistoryRow(cut, "Sat 14 Nov").QuerySelectorAll("td")[3]));
        Assert.Equal("Edit note", HistoryRow(cut, "Sat 14 Nov").QuerySelector("td.ht-actions > button")!.TextContent);
        Assert.Equal(noteId, LastFocusedId(2));
    }

    /// <summary>
    /// The ended goal restarted on Monday 16 November at 11:00 with unchanged settings from Tuesday 17 November, read at
    /// 12:00: attempt 1's Sunday 15 is still Pending (its grace period ends at 00:00 on Wednesday 18), so it can be excused,
    /// and an excusal on it can be removed (FR-010).
    /// </summary>
    private async Task<GoalId> RestartedOnTheDayOfTheEndingAsync(ActingUser user, Action<GoalId>? arrange = null)
    {
        var goal = await EndedAsync(this, user);
        arrange?.Invoke(goal);
        await StartAttemptAsync(this, user, goal, Utc(16, 11), MinutesDaily(10), new DateOnly(2026, 11, 17));
        Clock.SetUtcNow(Utc(16, 12));
        return goal;
    }

    [Fact]
    public async Task FR_010_Remove_excusal_on_an_excused_row_of_the_ended_attempt_makes_it_Pending_and_focuses_Add_record()
    {
        var user = SignIn();
        var goal = await RestartedOnTheDayOfTheEndingAsync(user, g => _records.SeedExcusal(g, Sun15));
        var cut = RenderAttempt(goal, 1);
        Assert.Equal("Excused", Text(HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td")[2]));
        var remove = HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal("Remove excusal of Sun 15 Nov", remove.GetAttribute("aria-label"));

        await remove.ClickAsync(new MouseEventArgs());

        Assert.Empty(_records.ExemptionsOfGoal(goal).Excusals);
        Assert.Equal("Pending", Text(HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td")[2]));
        Assert.Equal(["Add note", "Add record"], HistoryActions(cut, "Sun 15 Nov"));
        Assert.Empty(cut.FindAll("[role=alert]"));
        var addRecord = HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(addRecord.GetAttribute("blazor:elementReference"), LastFocusedId(1));
    }

    [Fact]
    public async Task FR_010_Excuse_instead_from_a_Pending_row_of_the_ended_attempt_excuses_it_and_shows_Excused()
    {
        var user = SignIn();
        var goal = await RestartedOnTheDayOfTheEndingAsync(user);
        var cut = RenderAttempt(goal, 1);
        Assert.Equal("Pending", Text(HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td")[2]));

        await HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button:last-of-type")!.ClickAsync(new MouseEventArgs());
        var excuseInstead = cut.FindAll(".ht-dialog__foot .ht-btn")[^1];
        Assert.Equal("Excuse instead", excuseInstead.TextContent);
        await excuseInstead.ClickAsync(new MouseEventArgs());

        var dialog = Assert.Single(cut.FindAll("[role=dialog]"));
        Assert.Equal("Excuse · " + Name, dialog.QuerySelector("h2")!.TextContent);
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.NotNull(_records.ExemptionsOfGoal(goal).ExcusalOf(Sun15));
        Assert.Equal("Excused", Text(HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td")[2]));
        Assert.Equal(["Add note", "Remove excusal"], HistoryActions(cut, "Sun 15 Nov"));
    }

    [Fact]
    public async Task AC_102_an_attempt_with_no_date_says_ended_before_it_began_with_N_A_and_no_History_card()
    {
        // Created on Monday 16 November with the start date Monday 23 November, ended on Tuesday 17 November with "No
        // longer relevant", restarted on Wednesday 18 November from that date; read on Thursday 19 November.
        var user = SignIn();
        var catalog = (FakeCatalogHabitStore)Services.GetRequiredService<ICatalogHabitStore>();
        var meditation = catalog.Add(HabitSamples.Catalog("Meditation"));
        Clock.SetUtcNow(Utc(16, 8));
        GoalId goal;
        await using (var scope = Services.CreateAsyncScope())
        {
            var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(user, new GoalInput
            {
                Habit = new GoalHabit(meditation.Id, HabitKind.Catalog), Target = MinutesDaily(10), StartDate = Mon23,
            }, CancellationToken.None);
            goal = created.GoalId!.Value;
        }

        Clock.SetUtcNow(Utc(17, 10));
        await using (var scope = Services.CreateAsyncScope())
        {
            var ended = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().EndAsync(
                user, goal, Outcome.NoLongerRelevant, null, CancellationToken.None);
            Assert.Equal(GoalEndOutcome.Ended, ended.Outcome);
        }

        await StartAttemptAsync(this, user, goal, Utc(18, 8), MinutesDaily(10), Wed18);
        Clock.SetUtcNow(Utc(19, 10));

        var cut = RenderAttempt(goal, 1);

        Assert.Equal("Attempt 1 · ended before it began", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal([Name, "No longer relevant"], cut.Find(".ht-page-head .ht-head-tags").Children.Select(c => c.TextContent));
        Assert.Empty(cut.FindAll(".ht-page-head div.ht-muted"));
        Assert.Equal(["N/A", "0", "0"], Stats(cut));
        Assert.Equal("Last 0 commitments", cut.Find(".ht-strip").GetAttribute("aria-label"));
        Assert.Equal(["Consistency and streaks"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public async Task HP_014_the_latest_attempt_an_unknown_number_another_account_an_unknown_goal_and_an_administrator_get_the_Not_found_page()
    {
        var owner = SignIn();
        var goal = await RestartedAsync(this, owner, Utc(23, 12));

        var latest = RenderAttempt(goal, 2);
        var third = RenderAttempt(goal, 3);
        var zero = RenderAttempt(goal, 0);
        SignIn();
        var other = RenderAttempt(goal, 1);
        var unknown = RenderAttempt(new GoalId(Guid.NewGuid()), 1);
        SignIn("Administrator");
        var administrator = RenderAttempt(goal, 1);

        Assert.Equal(6, _notFound);
        foreach (var cut in new[] { latest, third, zero, other, unknown, administrator })
        {
            Assert.Empty(cut.FindAll("table"));
            Assert.Empty(cut.FindAll(".ht-stats"));
            Assert.DoesNotContain("Meditation", cut.Markup, StringComparison.Ordinal);
        }
    }
}
