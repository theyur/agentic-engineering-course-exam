using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace HT.Persistence.Identity;

/// <summary>
/// Returns the name and the email unchanged (research R-04). Both already hold the domain normal form, so the
/// normalized columns compare exactly as <c>Email</c> equality does.
/// </summary>
internal sealed class EmailLookupNormalizer : ILookupNormalizer
{
    [return: NotNullIfNotNull(nameof(name))]
    public string? NormalizeName(string? name) => name;

    [return: NotNullIfNotNull(nameof(email))]
    public string? NormalizeEmail(string? email) => email;
}
