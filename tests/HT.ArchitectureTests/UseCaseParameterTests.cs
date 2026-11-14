using System.Reflection;
using HT.Application.Habits;
using HT.Domain.Accounts;
using Xunit;

namespace HT.ArchitectureTests;

/// <summary>
/// Research R-12, constitution III: the parameters of the habit use cases and ports, checked by reflection. The
/// public catalog takes no acting user; use cases for signed-in users take the acting user first and never an
/// account identifier; the custom-habit port takes the owner first.
/// </summary>
public sealed class UseCaseParameterTests
{
    private static IEnumerable<MethodInfo> PublicMethods(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName);

    private static bool Takes(MethodInfo method, Type type) =>
        method.GetParameters().Any(p => p.ParameterType == type || Nullable.GetUnderlyingType(p.ParameterType) == type);

    [Fact]
    public void No_public_method_of_CatalogService_takes_an_acting_user_or_an_account_identifier()
    {
        var methods = PublicMethods(typeof(CatalogService)).ToList();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => Takes(m, typeof(ActingUser)) || Takes(m, typeof(AccountId)))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }
}
