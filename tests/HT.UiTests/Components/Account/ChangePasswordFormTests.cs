using Bunit;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Ui.Components.Account;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Components.Account;

/// <summary>US6-AS9, HP-014: refusals next to their field, the mismatch check and the success state.</summary>
public sealed class ChangePasswordFormTests : BunitContext
{
    private const string Current = "change-current-password";
    private const string New = "change-new-password";
    private const string Repeat = "change-repeat-new-password";

    private readonly FakeAccountStore _store = new();
    private readonly List<PasswordChangeResult> _succeeded = [];

    public ChangePasswordFormTests()
    {
        Services.AddSingleton(new AccountService(_store, TimeProvider.System));
    }

    [Fact]
    public void The_form_has_its_three_fields_the_rule_as_help_text_and_its_button()
    {
        var cut = RenderForm(_store.Add("ana@example.com", "walking42"));

        Assert.Equal("Change password", cut.Find(".ht-form-section__title").TextContent);
        var fields = cut.FindAll("label.ht-field");
        Assert.Equal(
            ["Current password", "New password", "Repeat new password"],
            fields.Select(f => f.QuerySelector(".ht-field__label")!.TextContent));
        Assert.Equal([Current, New, Repeat], fields.Select(f => f.QuerySelector("input")!.Id));
        Assert.Equal("At least 8 characters, with a number.", cut.Find($"#{New}-help").TextContent);
        Assert.All(cut.FindAll("input.ht-input"), input => Assert.Equal("password", input.GetAttribute("type")));
        Assert.Equal("Change password", cut.Find("button[type=submit]").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice"));
    }

    [Theory]
    [InlineData("wrong4242", "running42", Current, "The current password is not correct.")]
    [InlineData("walking42", "runningfar", New, "At least 8 characters, with a number.")]
    [InlineData("wrong4242", "run", Current, "The current password is not correct.")]
    public void A_refusal_appears_next_to_its_field_only(string current, string next, string fieldId, string message)
    {
        var account = _store.Add("refused@example.com", "walking42");
        var cut = RenderForm(account);

        Submit(cut, current, next, next);

        Assert.Equal(message, cut.Find($"#{fieldId}-error").TextContent);
        Assert.Equal("true", cut.Find($"#{fieldId}").GetAttribute("aria-invalid"));
        Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.Equal("walking42", _store.PasswordOf(account.Id));
        Assert.Empty(_succeeded);
        AssertFieldsEmpty(cut);
    }

    [Fact]
    public void A_mismatch_shows_the_message_next_to_the_repeated_password_and_the_use_case_is_not_called()
    {
        var account = _store.Add("mismatch@example.com", "walking42");
        var cut = RenderForm(account);

        Submit(cut, "walking42", "running42", "running43");

        Assert.Equal("The passwords do not match.", cut.Find($"#{Repeat}-error").TextContent);
        Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(0, _store.PasswordCheckCount);
        Assert.Equal(0, _store.PasswordChangeCount);
        Assert.Empty(_succeeded);
    }

    [Fact]
    public void A_change_shows_the_notice_empties_the_fields_and_raises_on_succeeded()
    {
        var account = _store.Add("success@example.com", "walking42");
        var cut = RenderForm(account);

        Submit(cut, "walking42", "running42", "running42");

        var notice = cut.Find(".ht-notice");
        Assert.Equal("Your password is changed.", notice.TextContent.Trim());
        Assert.Equal("status", notice.GetAttribute("role"));
        Assert.Empty(cut.FindAll(".ht-field__error"));
        AssertFieldsEmpty(cut);
        var result = Assert.Single(_succeeded);
        Assert.Equal(PasswordChangeOutcome.Changed, result.Outcome);
        Assert.Equal("running42", _store.PasswordOf(account.Id));
    }

    [Fact]
    public void A_refusal_after_a_change_removes_the_notice()
    {
        var cut = RenderForm(_store.Add("twice@example.com", "walking42"));
        Submit(cut, "walking42", "running42", "running42");

        Submit(cut, "walking42", "cycling42", "cycling42");

        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.Equal("The current password is not correct.", cut.Find($"#{Current}-error").TextContent);
    }

    [Fact]
    public void The_form_changes_the_password_of_its_acting_user_only()
    {
        var erin = _store.Add("erin@example.com", "walking42");
        var finn = _store.Add("finn@example.com", "walking42");
        var cut = RenderForm(finn);

        Submit(cut, "walking42", "running42", "running42");

        Assert.Equal("running42", _store.PasswordOf(finn.Id));
        Assert.Equal("walking42", _store.PasswordOf(erin.Id));
    }

    private IRenderedComponent<ChangePasswordForm> RenderForm(HT.Domain.Accounts.Account account) =>
        Render<ChangePasswordForm>(parameters => parameters
            .Add(p => p.ActingUser, ActingUser.ForAccount(account.Id, account.Role))
            .Add(p => p.OnSucceeded, result => _succeeded.Add(result)));

    private static void Submit(IRenderedComponent<ChangePasswordForm> cut, string current, string next, string repeat)
    {
        cut.Find($"#{Current}").Change(current);
        cut.Find($"#{New}").Change(next);
        cut.Find($"#{Repeat}").Change(repeat);
        cut.Find("form").Submit();
    }

    private static void AssertFieldsEmpty(IRenderedComponent<ChangePasswordForm> cut)
    {
        foreach (var id in new[] { Current, New, Repeat })
        {
            Assert.True(string.IsNullOrEmpty(cut.Find($"#{id}").GetAttribute("value")), id);
        }
    }
}
