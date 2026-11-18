using HT.Domain.Accounts;

namespace HT.UnitTests.Domain.Accounts;

/// <summary>FR-028: an account name is trimmed, 1 to 60 code units, and empty means none.</summary>
public class AccountNameTests
{
    [Fact]
    public void The_name_is_trimmed()
    {
        Assert.True(AccountName.TryCreate("  Yuri  ", out var name, out var tooLong));
        Assert.False(tooLong);
        Assert.Equal("Yuri", name!.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Spaces_only_is_no_name(string? text)
    {
        Assert.True(AccountName.TryCreate(text, out var name, out var tooLong));
        Assert.False(tooLong);
        Assert.Null(name);
    }

    [Fact]
    public void Sixty_code_units_are_accepted()
    {
        var sixty = new string('y', 60);

        Assert.True(AccountName.TryCreate(" " + sixty + " ", out var name, out _));
        Assert.Equal(sixty, name!.Value.Value);
    }

    [Fact]
    public void Sixty_one_code_units_are_too_long()
    {
        Assert.False(AccountName.TryCreate(new string('y', 61), out var name, out var tooLong));
        Assert.True(tooLong);
        Assert.Null(name);
    }

    [Fact]
    public void An_account_keeps_its_new_properties()
    {
        Assert.True(Email.TryParse("yuri@example.com", out var email));
        var account = new Account(new AccountId(Guid.NewGuid()), email, Role.RegisteredUser, SignInBlock.None)
        {
            StoredTimeZone = "Europe/Kyiv",
            Name = "Yuri",
            CreatedAt = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero),
        };

        Assert.Equal("Europe/Kyiv", account.StoredTimeZone);
        Assert.Equal("Yuri", account.Name);
        Assert.Equal(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero), account.CreatedAt);
        Assert.Null((account with { StoredTimeZone = null }).StoredTimeZone);
    }
}
