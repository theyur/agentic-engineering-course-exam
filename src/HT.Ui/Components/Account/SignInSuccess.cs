using HT.Application.Accounts;

namespace HT.Ui.Components.Account;

/// <summary>What <see cref="SignInForm"/> hands its page on success: the result and the "Keep me signed in" choice.</summary>
public sealed record SignInSuccess(SignInResult Result, bool KeepSignedIn);
