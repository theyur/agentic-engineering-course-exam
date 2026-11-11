using HT.Domain.Accounts;

namespace HT.UnitTests.Domain.Accounts;

public class ActingUserTests
{
    [Fact]
    public void Anonymous_has_no_account_and_no_role()
    {
        var visitor = ActingUser.Anonymous;

        Assert.True(visitor.IsAnonymous);
        Assert.Null(visitor.AccountId);
        Assert.Null(visitor.Role);
    }

    [Theory]
    [InlineData(Role.RegisteredUser)]
    [InlineData(Role.Administrator)]
    public void ForAccount_keeps_the_identifier_and_the_role(Role role)
    {
        var id = new AccountId(Guid.NewGuid());

        var user = ActingUser.ForAccount(id, role);

        Assert.False(user.IsAnonymous);
        Assert.Equal(id, user.AccountId);
        Assert.Equal(role, user.Role);
    }
}
