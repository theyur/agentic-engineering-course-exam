namespace HT.Domain.Accounts;

/// <summary>The identifier of an account. Created by the account store, never by a use-case caller.</summary>
public readonly record struct AccountId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
