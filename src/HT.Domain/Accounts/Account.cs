namespace HT.Domain.Accounts;

/// <summary>One person's way into the app. Immutable: use cases produce a new value and save it.</summary>
public sealed record Account(AccountId Id, Email Email, Role Role, SignInBlock SignInBlock);
