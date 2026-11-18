using Bunit;
using HT.Application.Accounts;
using HT.Ui.Components.Account;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Components.Account;

public sealed class RegisterFormTests : BunitContext
{
    private readonly FakeAccountStore _store = new();
    private readonly List<RegistrationResult> _succeeded = [];

    public RegisterFormTests()
    {
        Services.AddSingleton(new AccountService(_store, TimeProvider.System, InstallationTimeZone.From(null)));
    }

    [Fact]
    public void US1_AS7_the_form_shows_the_password_rule_as_help_text_and_the_neutral_notice()
    {
        var cut = RenderForm();

        Assert.Equal("At least 8 characters, with a number.", cut.Find("#register-password-help").TextContent);
        var notice = cut.Find(".ht-notice.ht-notice--neutral");
        Assert.Equal("Community comparison is off. You can opt in later in your profile.", notice.TextContent.Trim());
        Assert.Equal("Create account", cut.Find("button[type=submit]").TextContent);
    }

    [Theory]
    [InlineData("ana@example", "walking42", "register-email", "Enter a valid email address.")]
    [InlineData("short.pw@example.com", "walk4", "register-password", "At least 8 characters, with a number.")]
    public void A_refusal_appears_next_to_its_field_and_the_email_is_kept(
        string email, string password, string fieldId, string message)
    {
        var cut = RenderForm();

        Submit(cut, email, password, password);

        Assert.Equal(message, cut.Find($"#{fieldId}-error").TextContent);
        Assert.Equal("true", cut.Find($"#{fieldId}").GetAttribute("aria-invalid"));
        Assert.Equal(email, cut.Find("#register-email").GetAttribute("value"));
        Assert.Empty(_store.Accounts);
        Assert.Empty(_succeeded);
    }

    [Fact]
    public void US1_AS9_a_taken_email_shows_the_message_with_a_sign_in_link_and_keeps_the_email()
    {
        _store.Add("taken@example.com", "walking12");
        var cut = RenderForm();

        Submit(cut, " Taken@Example.com ", "walking42", "walking42");

        var error = cut.Find("#register-email-error");
        Assert.Equal("An account with this email already exists. Sign in", error.TextContent);
        Assert.Equal("/sign-in", error.QuerySelector("a")!.GetAttribute("href"));
        Assert.Equal(" Taken@Example.com ", cut.Find("#register-email").GetAttribute("value"));
        Assert.Single(_store.Accounts);
    }

    [Fact]
    public void US1_AS8_mismatched_passwords_show_the_message_and_do_not_call_the_use_case()
    {
        var cut = RenderForm();

        Submit(cut, "mismatch@example.com", "walking42", "walking43");

        Assert.Equal("The passwords do not match.", cut.Find("#register-repeat-password-error").TextContent);
        Assert.Equal("mismatch@example.com", cut.Find("#register-email").GetAttribute("value"));
        Assert.Empty(cut.FindAll("#register-email-error"));
        Assert.Equal(0, _store.CreateCount);
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public void A_refusal_empties_both_password_fields()
    {
        var cut = RenderForm();

        Submit(cut, "short.pw@example.com", "walk4", "walk4");

        Assert.True(string.IsNullOrEmpty(cut.Find("#register-password").GetAttribute("value")));
        Assert.True(string.IsNullOrEmpty(cut.Find("#register-repeat-password").GetAttribute("value")));
    }

    [Fact]
    public void A_successful_registration_raises_OnSucceeded_once_with_the_result()
    {
        var cut = RenderForm();

        Submit(cut, "new.user@example.com", "walking42", "walking42");

        var result = Assert.Single(_succeeded);
        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Equal(_store.Find("new.user@example.com")!.Id, result.AccountId);
    }

    [Fact]
    public void HP_016_the_form_has_the_hidden_browser_time_zone_input_and_no_visible_time_zone_field()
    {
        var cut = RenderForm();

        var marked = Assert.Single(cut.FindAll("input[data-ht-browser-time-zone]"));
        Assert.Equal("hidden", marked.GetAttribute("type"));
        Assert.Equal("TimeZone", marked.GetAttribute("name"));
        Assert.True(string.IsNullOrEmpty(marked.GetAttribute("value")));
        Assert.Empty(cut.FindAll("select"));
        Assert.Equal(
            ["register-name", "register-email", "register-password", "register-repeat-password"],
            cut.FindAll("input:not([type=hidden])").Select(input => input.Id));
        Assert.DoesNotContain("time zone", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HP_016_a_registration_without_a_browser_time_zone_stores_the_installations_default()
    {
        var cut = RenderForm();

        Submit(cut, "no.zone@example.com", "walking42", "walking42");

        Assert.Single(_succeeded);
        Assert.Equal("UTC", _store.Find("no.zone@example.com")!.StoredTimeZone);
    }

    [Fact]
    public void HP_017_the_optional_name_field_comes_first_with_its_help()
    {
        var cut = RenderForm();

        var fields = cut.FindAll(".ht-field");
        Assert.Equal("register-name", fields[0].QuerySelector("input")!.Id);
        Assert.Equal("Name (optional)", fields[0].QuerySelector(".ht-field__label")!.TextContent.Trim());
        Assert.Equal("(optional)", fields[0].QuerySelector(".ht-field__label .ht-muted")!.TextContent);
        Assert.Equal("name", cut.Find("#register-name").GetAttribute("autocomplete"));
        Assert.Equal("Only you can see it. Home uses it to greet you.", cut.Find("#register-name-help").TextContent);
        Assert.Equal("register-name-help", cut.Find("#register-name").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void US11_AS1_the_name_is_stored_trimmed()
    {
        var cut = RenderForm();

        cut.Find("#register-name").Change("  Yuri  ");
        Submit(cut, "named@example.com", "walking42", "walking42");

        Assert.Single(_succeeded);
        Assert.Equal("Yuri", _store.Find("named@example.com")!.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void US11_AS1_no_name_or_only_spaces_gives_no_name(string name)
    {
        var cut = RenderForm();

        cut.Find("#register-name").Change(name);
        Submit(cut, "unnamed@example.com", "walking42", "walking42");

        Assert.Single(_succeeded);
        Assert.Null(_store.Find("unnamed@example.com")!.Name);
    }

    [Fact]
    public void US11_AS2_a_name_of_61_characters_is_refused_under_the_field_and_keeps_the_name_and_the_email()
    {
        var name = new string('n', 61);
        var cut = RenderForm();

        cut.Find("#register-name").Change(name);
        Submit(cut, "long.name@example.com", "walking42", "walking42");

        Assert.Equal("The name can have at most 60 characters.", cut.Find("#register-name-error").TextContent);
        Assert.Equal("true", cut.Find("#register-name").GetAttribute("aria-invalid"));
        Assert.Equal(name, cut.Find("#register-name").GetAttribute("value"));
        Assert.Equal("long.name@example.com", cut.Find("#register-email").GetAttribute("value"));
        Assert.True(string.IsNullOrEmpty(cut.Find("#register-password").GetAttribute("value")));
        Assert.Empty(_store.Accounts);
        Assert.Empty(_succeeded);
    }

    private IRenderedComponent<RegisterForm> RenderForm() =>
        Render<RegisterForm>(p => p.Add(x => x.OnSucceeded, result => _succeeded.Add(result)));

    private static void Submit(IRenderedComponent<RegisterForm> cut, string email, string password, string repeat)
    {
        cut.Find("#register-email").Change(email);
        cut.Find("#register-password").Change(password);
        cut.Find("#register-repeat-password").Change(repeat);
        cut.Find("form").Submit();
    }
}
