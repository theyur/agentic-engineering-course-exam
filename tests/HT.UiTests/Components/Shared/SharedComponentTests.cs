using Bunit;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

public sealed class SharedComponentTests : BunitContext
{
    [Fact]
    public void TextField_renders_its_label_and_help_text_linked_by_aria_describedby()
    {
        var cut = Render<TextFieldHost>(p => p
            .Add(x => x.Help, "At least 8 characters, with a number.")
            .Add(x => x.Value, "ana@example.com"));

        Assert.Equal("Email", cut.Find(".ht-field__label").TextContent);
        var input = cut.Find("input#email");
        var help = cut.Find(".ht-field__help");
        Assert.Equal("At least 8 characters, with a number.", help.TextContent);
        Assert.Equal(help.Id, input.GetAttribute("aria-describedby"));
        Assert.Null(input.GetAttribute("aria-invalid"));
        Assert.Equal("ana@example.com", input.GetAttribute("value"));
        Assert.Contains("ht-input", input.ClassList);
    }

    [Fact]
    public void TextField_renders_its_error_next_to_the_field_in_place_of_the_help_text()
    {
        var cut = Render<TextFieldHost>(p => p
            .Add(x => x.Help, "At least 8 characters, with a number.")
            .Add(x => x.Error, "Enter a valid email address."));

        var input = cut.Find("input#email");
        var error = cut.Find(".ht-field__error");
        Assert.Equal("Enter a valid email address.", error.TextContent);
        Assert.Equal(error.Id, input.GetAttribute("aria-describedby"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Empty(cut.FindAll(".ht-field__help"));
    }

    [Theory]
    [InlineData("ana@example.com", "A")]
    [InlineData("yuri@example.com", "Y")]
    public void Avatar_shows_the_first_letter_of_the_email_in_upper_case(string email, string letter)
    {
        var cut = Render<Avatar>(p => p.Add(x => x.Email, email));

        var avatar = cut.Find(".ht-avatar");
        Assert.Equal(letter, avatar.TextContent);
        Assert.Equal("Account of " + email, avatar.GetAttribute("aria-label"));
    }

    [Fact]
    public void EmptyState_renders_its_heading_and_text()
    {
        var cut = Render<EmptyState>(p => p
            .Add(x => x.Title, "Page not found")
            .Add(x => x.Text, "The page you asked for does not exist.")
            .Add(x => x.IsPageHeading, true));

        Assert.Equal("Page not found", cut.Find(".ht-empty h1").TextContent);
        Assert.Equal("The page you asked for does not exist.", cut.Find(".ht-empty p").TextContent);
    }

    [Fact]
    public void KeyValue_renders_one_dt_and_dd_pair_per_row_in_order()
    {
        var cut = Render<KeyValue>(p => p.Add(x => x.Rows,
        [
            new KeyValue.Row("Habit", "Yoga · catalog"),
            new KeyValue.Row("Build or reduce", "Build"),
            new KeyValue.Row("Measure", "Yes / no"),
            new KeyValue.Row("Schedule", "Mon · Wed · Fri"),
        ]));

        var list = cut.Find("dl.ht-kv");
        Assert.Equal(
            ["DT", "DD", "DT", "DD", "DT", "DD", "DT", "DD"],
            list.Children.Select(c => c.TagName));
        Assert.Equal(["Habit", "Build or reduce", "Measure", "Schedule"], cut.FindAll(".ht-kv dt").Select(e => e.TextContent));
        Assert.Equal(["Yoga · catalog", "Build", "Yes / no", "Mon · Wed · Fri"], cut.FindAll(".ht-kv dd").Select(e => e.TextContent));
    }

    [Fact]
    public void KeyValue_without_rows_renders_an_empty_list()
    {
        var cut = Render<KeyValue>(p => p.Add(x => x.Rows, []));

        Assert.Empty(cut.Find("dl.ht-kv").Children);
    }
}
