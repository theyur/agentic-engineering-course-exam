using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using CustomHabitPage = HT.Ui.Components.Pages.CustomHabit;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>HP-007, US4-AS13, US4-AS15: the custom habit page over the real CustomHabitService and the fake store.</summary>
public sealed class CustomHabitPageTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCustomHabitStore _store;
    private int _notFound;

    public CustomHabitPageTests()
    {
        _authorization = AddAuthorization();
        this.AddCatalog();
        _store = this.AddCustomHabits();
        Navigation.OnNotFound += (_, _) => _notFound++;
    }

    private BunitNavigationManager Navigation => Services.GetRequiredService<BunitNavigationManager>();

    private AccountId SignInOwner() => new(_authorization.SignIn("RegisteredUser"));

    private IRenderedComponent<CustomHabitPage> RenderNew()
    {
        Navigation.NavigateTo("/custom-habits/new");
        return Render<CustomHabitPage>();
    }

    private IRenderedComponent<CustomHabitPage> RenderEdit(HabitId id)
    {
        Navigation.NavigateTo($"/custom-habits/{id.Value}");
        return Render<CustomHabitPage>(p => p.Add(x => x.Id, id.Value));
    }

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<CustomHabitPage> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text);

    private static void Fill(IRenderedComponent<CustomHabitPage> cut, string name, string category = "Body", string intent = "Build", string icon = "droplet")
    {
        cut.Find("#custom-habit-name").Change(name);
        cut.Find("#custom-habit-category").Change(category);
        cut.FindAll(".ht-seg button").First(b => b.TextContent == intent).Click();
        cut.Find($"button[aria-label={icon}]").Click();
    }

    private static IReadOnlyList<string> Errors(IRenderedComponent<CustomHabitPage> cut) =>
        cut.FindAll(".ht-field__error").Select(e => e.TextContent).ToList();

    [Fact]
    public void US4_AS13_the_new_page_has_the_form_and_the_privacy_card_and_no_later_actions()
    {
        SignInOwner();

        var cut = RenderNew();

        var back = cut.Find("a.ht-back");
        Assert.Equal("← My custom habits", back.TextContent);
        Assert.Equal("/custom-habits", back.GetAttribute("href"));
        Assert.Equal("New custom habit", cut.Find("h1").TextContent);
        Assert.Equal("A private habit for your own goals.", cut.Find(".ht-page-head .ht-muted").TextContent);
        Assert.Equal(["Name", "Description (optional)", "Category", "Usually you want to", "Icon"], cut.FindAll(".ht-field__label").Select(Text));
        Assert.Equal("Short and clear, for example \"Cold shower\" or \"No phone in bed\".", cut.Find(".ht-field__help").TextContent);
        Assert.Equal(["Choose a category", "Body", "Mind", "Health", "Money", "Home", "Other"], cut.FindAll("select option").Select(o => o.TextContent));
        Assert.True(cut.Find("select option").HasAttribute("selected"));
        Assert.Equal(["Build", "Reduce"], cut.FindAll(".ht-seg button").Select(b => b.TextContent));
        Assert.All(cut.FindAll(".ht-seg button"), b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
        Assert.Equal(HabitIcon.All.Select(i => i.Name), cut.FindAll("button.ht-icon-choice").Select(b => b.GetAttribute("aria-label")));
        Assert.All(cut.FindAll("button.ht-icon-choice"), b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
        Assert.Equal(["Save habit", "Cancel"], cut.FindAll(".ht-form-actions .ht-btn").Select(b => b.TextContent));
        Assert.Equal("submit", cut.Find(".ht-form-actions button").GetAttribute("type"));
        Assert.Equal("/custom-habits", cut.Find(".ht-form-actions a").GetAttribute("href"));
        Assert.Contains("ht-btn--ghost", cut.Find(".ht-form-actions a").ClassList);
        Assert.DoesNotContain("Delete", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Save and create goal", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Suggest", cut.Markup, StringComparison.Ordinal);
        var privacy = cut.Find("aside.ht-card");
        Assert.Equal("Private", privacy.QuerySelector("h2")!.TextContent);
        Assert.Equal(["Only you can see custom habits. Admins cannot see them.", "They never appear in the catalog or in community statistics."],
            privacy.QuerySelectorAll(".ht-goal").Select(g => g.TextContent));
        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(0, _notFound);
    }

    [Fact]
    public void Refusals_sit_under_their_field_one_at_a_time_in_FR_005_order()
    {
        SignInOwner();
        var cut = RenderNew();

        Button(cut, "Save habit").Click();
        Assert.Equal(["Enter a name."], Errors(cut));
        Assert.Equal("true", cut.Find("#custom-habit-name").GetAttribute("aria-invalid"));

        cut.Find("#custom-habit-name").Change("Cold shower");
        Button(cut, "Save habit").Click();
        Assert.Equal(["Choose a category."], Errors(cut));
        Assert.Equal("custom-habit-category-error", cut.Find("#custom-habit-category").GetAttribute("aria-describedby"));
        Assert.Null(cut.Find("#custom-habit-name").GetAttribute("aria-invalid"));

        cut.Find("#custom-habit-category").Change("Body");
        Button(cut, "Save habit").Click();
        Assert.Equal(["Choose Build or Reduce."], Errors(cut));

        cut.FindAll(".ht-seg button")[0].Click();
        Button(cut, "Save habit").Click();
        Assert.Equal(["Choose an icon."], Errors(cut));
        Assert.Equal(0, _store.WriteCount);
        Assert.EndsWith("/custom-habits/new", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void A_taken_name_is_refused_under_Name()
    {
        var owner = SignInOwner();
        _store.Add(owner, Definition("Cold shower", Category.Body));
        var cut = RenderNew();

        Fill(cut, " cold SHOWER ");
        Button(cut, "Save habit").Click();

        Assert.Equal(["You already have a custom habit with this name."], Errors(cut));
        Assert.Equal("custom-habit-name-error", cut.Find("#custom-habit-name").GetAttribute("aria-describedby"));
        Assert.Single(_store.Habits);
    }

    [Fact]
    public void HP_007_Save_creates_the_habit_and_returns_to_My_custom_habits()
    {
        var owner = SignInOwner();
        var cut = RenderNew();

        Fill(cut, "Cold shower", intent: "Reduce");
        cut.Find("textarea").Input("At least 1 minute.");
        Button(cut, "Save habit").Click();

        var habit = Assert.Single(_store.Habits);
        Assert.Equal(owner, habit.Owner);
        Assert.Equal("Cold shower", habit.Definition.Name.Value);
        Assert.Equal("At least 1 minute.", habit.Definition.Description);
        Assert.Equal(Intent.Reduce, habit.Definition.Intent);
        Assert.Equal("droplet", habit.Definition.Icon.Name);
        Assert.EndsWith("/custom-habits", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void Editing_shows_the_habit_with_Delete_and_Save_returns_to_My_custom_habits()
    {
        var owner = SignInOwner();
        var habit = _store.Add(owner, Definition("Cold shower", Category.Body, icon: "droplet", description: "Cold water."));
        var cut = RenderEdit(habit.Id);

        Assert.Equal("Edit custom habit", cut.Find("h1").TextContent);
        Assert.Equal("Cold shower", cut.Find("#custom-habit-name").GetAttribute("value"));
        Assert.Equal("Cold water.", cut.Find("textarea").GetAttribute("value") ?? cut.Find("textarea").TextContent);
        Assert.True(cut.FindAll("select option").First(o => o.TextContent == "Body").HasAttribute("selected"));
        Assert.Equal("true", cut.FindAll(".ht-seg button")[0].GetAttribute("aria-pressed"));
        Assert.Equal("true", cut.Find("button[aria-label=droplet]").GetAttribute("aria-pressed"));
        Assert.Empty(cut.FindAll(".ht-field__help"));
        Assert.Equal(["Save habit", "Cancel", "Delete"], cut.FindAll(".ht-form-actions .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--danger", Button(cut, "Delete").ClassList);
        Assert.NotNull(cut.Find(".ht-form-actions .ht-spacer"));

        cut.Find("#custom-habit-name").Change("Cold bath");
        Button(cut, "Save habit").Click();

        Assert.Equal("Cold bath", _store.Get(habit.Id)!.Definition.Name.Value);
        Assert.EndsWith("/custom-habits", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_007_Delete_asks_first_in_a_Dialog_and_Cancel_keeps_the_habit_and_returns_focus_to_Delete()
    {
        var owner = SignInOwner();
        var habit = _store.Add(owner, Definition("Cold shower", Category.Body));
        var cut = RenderEdit(habit.Id);
        var deleteButton = Button(cut, "Delete").GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(deleteButton));

        Button(cut, "Delete").Click();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Delete this custom habit?", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal(dialog.QuerySelector("h2")!.Id, dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("Cold shower · Private", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal("The habit is deleted for good. This cannot be undone.", dialog.QuerySelector(".ht-dialog__body p")!.TextContent);
        Assert.Equal(["Delete", "Cancel"], dialog.QuerySelectorAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--danger", dialog.QuerySelector(".ht-dialog__foot .ht-btn")!.ClassList);
        Assert.Contains("ht-btn--secondary", dialog.QuerySelectorAll(".ht-dialog__foot .ht-btn")[1].ClassList);
        var dialogReference = dialog.GetAttribute("blazor:elementReference");
        Assert.Equal(0, _store.WriteCount);

        cut.Find(".ht-dialog__foot .ht-btn--secondary").Click();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.NotNull(_store.Get(habit.Id));
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.False(string.IsNullOrEmpty(dialogReference));
        Assert.Equal([dialogReference, deleteButton], focused);
    }

    [Theory]
    [InlineData("Escape")]
    [InlineData("Close")]
    public void Esc_and_the_close_button_close_the_Dialog_without_deleting(string way)
    {
        var owner = SignInOwner();
        var habit = _store.Add(owner, Definition("Cold shower", Category.Body));
        var cut = RenderEdit(habit.Id);
        Button(cut, "Delete").Click();

        if (way == "Escape")
        {
            cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        }
        else
        {
            cut.Find("button.ht-dialog__close").Click();
        }

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.NotNull(_store.Get(habit.Id));
        JSInterop.VerifyFocusAsyncInvoke(2);
    }

    [Fact]
    public void HP_007_a_confirmed_Delete_deletes_the_habit_and_returns_to_My_custom_habits()
    {
        var owner = SignInOwner();
        var habit = _store.Add(owner, Definition("Cold shower", Category.Body));
        var cut = RenderEdit(habit.Id);
        Button(cut, "Delete").Click();

        cut.Find(".ht-dialog__foot .ht-btn--danger").Click();

        Assert.Null(_store.Get(habit.Id));
        Assert.EndsWith("/custom-habits", Navigation.Uri, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Save")]
    [InlineData("Delete")]
    public async Task HP_007_a_habit_deleted_elsewhere_shows_the_Notice_and_the_form_can_no_longer_be_saved(string action)
    {
        var owner = SignInOwner();
        var habit = _store.Add(owner, Definition("Cold shower", Category.Body));
        var cut = RenderEdit(habit.Id);
        var address = Navigation.Uri;
        if (action == "Delete")
        {
            Button(cut, "Delete").Click();
        }

        await _store.DeleteAsync(owner, habit.Id, CancellationToken.None);
        var writes = _store.WriteCount;
        if (action == "Save")
        {
            Button(cut, "Save habit").Click();
        }
        else
        {
            cut.Find(".ht-dialog__foot .ht-btn--danger").Click();
        }

        var notice = cut.Find(".ht-notice");
        Assert.Equal("alert", notice.GetAttribute("role"));
        Assert.Equal("This custom habit does not exist.", notice.QuerySelector("span")!.TextContent);
        var link = notice.QuerySelector("a.ht-btn")!;
        Assert.Equal("My custom habits", link.TextContent);
        Assert.Equal("/custom-habits", link.GetAttribute("href"));
        Assert.True(cut.Find("form fieldset").HasAttribute("disabled"));
        Assert.Empty(Errors(cut));
        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(address, Navigation.Uri);

        cut.Find("form").Submit();
        Assert.Equal(writes, _store.WriteCount);
        Assert.Equal(address, Navigation.Uri);
        Assert.Equal(0, _notFound);
    }

    [Theory]
    [InlineData("another user's")]
    [InlineData("deleted")]
    [InlineData("unknown")]
    public async Task US4_AS15_a_habit_that_is_not_the_users_shows_the_Not_found_page(string which)
    {
        var owner = SignInOwner();
        var id = which switch
        {
            "another user's" => _store.Add(new AccountId(Guid.NewGuid()), Definition("Cold shower")).Id,
            "deleted" => await DeletedAsync(owner),
            _ => new HabitId(Guid.NewGuid()),
        };

        var cut = RenderEdit(id);

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("form"));
        Assert.DoesNotContain("Cold shower", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void US4_AS15_an_administrator_gets_the_Not_found_page_on_either_address(bool edit)
    {
        var owner = new AccountId(Guid.NewGuid());
        var habit = _store.Add(owner, Definition("Cold shower"));
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = edit ? RenderEdit(habit.Id) : RenderNew();

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("form"));
        Assert.DoesNotContain("Cold shower", cut.Markup, StringComparison.Ordinal);
    }

    private async Task<HabitId> DeletedAsync(AccountId owner)
    {
        var habit = _store.Add(owner, Definition("Cold shower"));
        await _store.DeleteAsync(owner, habit.Id, CancellationToken.None);
        return habit.Id;
    }
}
