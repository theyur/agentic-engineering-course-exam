using HT.Domain.Accounts;

namespace HT.UnitTests.Domain.Accounts;

public class EmailTests
{
    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("a@b.c")]
    [InlineData("ana@sub.example.com")]
    [InlineData("o'neil@example.com")]
    public void Accepts_valid_emails(string text)
    {
        Assert.True(Email.TryParse(text, out var email));
        Assert.Equal(text, email.Value);
    }

    [Theory]
    [InlineData("ana@example..com")]
    [InlineData("ana@.example")]
    [InlineData("ana@.example.com")]
    [InlineData("ana@example.com.")]
    [InlineData("ana @example.com")]
    [InlineData("ana@.")]
    [InlineData("ana@example")]
    [InlineData("@example.com")]
    [InlineData("ana@@example.com")]
    [InlineData("ana@b@example.com")]
    [InlineData("ana\t@example.com")]
    [InlineData("anaexample.com")]
    public void Refuses_invalid_emails(string text)
    {
        Assert.False(Email.TryParse(text, out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuses_missing_text(string? text)
    {
        Assert.False(Email.TryParse(text, out _));
    }

    [Fact]
    public void Trims_and_lower_cases_into_the_normal_form()
    {
        Assert.True(Email.TryParse(" Taken@Example.com ", out var spaced));
        Assert.True(Email.TryParse("taken@example.com", out var plain));

        Assert.Equal("taken@example.com", spaced.Value);
        Assert.Equal(plain, spaced);
        Assert.Equal(plain.GetHashCode(), spaced.GetHashCode());
    }

    [Fact]
    public void Different_addresses_are_not_equal()
    {
        Assert.True(Email.TryParse("ana@example.com", out var ana));
        Assert.True(Email.TryParse("bo@example.com", out var bo));

        Assert.NotEqual(ana, bo);
    }

    [Fact]
    public void Accepts_254_characters_after_trimming_and_refuses_255()
    {
        var at254 = EmailOfLength(254);
        var at255 = EmailOfLength(255);

        Assert.True(Email.TryParse("  " + at254 + "  ", out var email));
        Assert.Equal(254, email.Value.Length);
        Assert.False(Email.TryParse(at255, out _));
    }

    private static string EmailOfLength(int length)
    {
        const string domain = "@example.com";
        return new string('a', length - domain.Length) + domain;
    }
}
