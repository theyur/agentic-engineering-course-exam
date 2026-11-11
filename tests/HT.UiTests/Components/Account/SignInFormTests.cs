using Bunit;
using HT.Application.Accounts;
using HT.Ui.Components.Account;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Components.Account;

public sealed class SignInFormTests : BunitContext
{
    private readonly FakeAccountStore _store = new();
    private readonly List<SignInSuccess> _succeeded = [];

    public SignInFormTests()
    {
        Services.AddSingleton(new AccountService(_store, TimeProvider.System));
    }

    [Fact]
    public void US2_AS10_the_form_has_keep_me_signed_in_and_no_forgot_password()
    {
        var cut = RenderForm();

        var keep = cut.Find("label.ht-check");
        Assert.Equal("Keep me signed in", keep.TextContent.Trim());
        Assert.Equal("checkbox", cut.Find("#sign-in-keep").GetAttribute("type"));
        Assert.Equal("Sign in", cut.Find("button[type=submit]").TextContent);
        Assert.DoesNotContain("Forgot password", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void US2_AS9_a_failure_shows_the_message_under_the_password_field_and_keeps_the_email()
    {
        _store.Add("ana@example.com", "walking42");
        var cut = RenderForm();

        Submit(cut, "ana@example.com", "walking43", keepSignedIn: false);

        Assert.Equal("Email or password is not correct.", cut.Find("#sign-in-password-error").TextContent);
        Assert.Equal("true", cut.Find("#sign-in-password").GetAttribute("aria-invalid"));
        Assert.Empty(cut.FindAll("#sign-in-email-error"));
        Assert.Equal("ana@example.com", cut.Find("#sign-in-email").GetAttribute("value"));
        Assert.True(string.IsNullOrEmpty(cut.Find("#sign-in-password").GetAttribute("value")));
        Assert.Empty(_succeeded);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OnSucceeded_carries_the_result_and_the_keep_signed_in_choice(bool keepSignedIn)
    {
        var account = _store.Add("ana@example.com", "walking42");
        var cut = RenderForm();

        Submit(cut, "ana@example.com", "walking42", keepSignedIn);

        var success = Assert.Single(_succeeded);
        Assert.Equal(keepSignedIn, success.KeepSignedIn);
        Assert.Equal(SignInOutcome.SignedIn, success.Result.Outcome);
        Assert.Equal(account.Id, success.Result.ActingUser!.AccountId);
    }

    private IRenderedComponent<SignInForm> RenderForm() =>
        Render<SignInForm>(p => p.Add(x => x.OnSucceeded, success => _succeeded.Add(success)));

    private static void Submit(IRenderedComponent<SignInForm> cut, string email, string password, bool keepSignedIn)
    {
        cut.Find("#sign-in-email").Change(email);
        cut.Find("#sign-in-password").Change(password);
        cut.Find("#sign-in-keep").Change(keepSignedIn);
        cut.Find("form").Submit();
    }
}
