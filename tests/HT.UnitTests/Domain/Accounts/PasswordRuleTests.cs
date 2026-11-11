using HT.Domain.Accounts;

namespace HT.UnitTests.Domain.Accounts;

public class PasswordRuleTests
{
    [Theory]
    [InlineData("walking1", true)]
    [InlineData("walkin1", false)]
    [InlineData("walkingfar", false)]
    [InlineData("walk4", false)]
    [InlineData("12345678", true)]
    [InlineData("", false)]
    public void Needs_8_characters_and_a_digit(string password, bool expected)
    {
        Assert.Equal(expected, PasswordRule.IsMet(password));
    }

    [Fact]
    public void Refuses_null()
    {
        Assert.False(PasswordRule.IsMet(null));
    }

    [Theory]
    [InlineData(" walk12 ")]
    [InlineData("  walk12")]
    [InlineData("walk12  ")]
    public void Counts_leading_and_trailing_spaces_as_characters(string password)
    {
        Assert.True(PasswordRule.IsMet(password));
    }

    [Fact]
    public void Seven_characters_with_spaces_and_a_digit_are_too_short()
    {
        Assert.False(PasswordRule.IsMet(" walk1 "));
    }
}
