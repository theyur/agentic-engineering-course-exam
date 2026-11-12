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
        Services.AddSingleton(new AccountService(_store, TimeProvider.System));
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
