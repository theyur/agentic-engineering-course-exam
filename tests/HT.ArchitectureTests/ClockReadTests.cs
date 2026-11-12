using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Dependencies;
using Xunit;

namespace HT.ArchitectureTests;

/// <summary>Constitution II: the domain and the use cases get the time and the culture supplied, never read them.</summary>
public sealed class ClockReadTests
{
    private static readonly string[] ForbiddenMembers =
    [
        "System.DateTime::get_Now(",
        "System.DateTime::get_UtcNow(",
        "System.DateTime::get_Today(",
        "System.DateTimeOffset::get_Now(",
        "System.DateTimeOffset::get_UtcNow(",
        "System.TimeZoneInfo::get_Local(",
        "System.Globalization.CultureInfo::get_CurrentCulture(",
    ];

    [Fact]
    public void DomainAndApplicationMustNotReadTheClockOrTheCurrentCulture()
    {
        var violations = ArchitectureModel.Architecture.Types
            .Where(t => t.Assembly.FullName == ArchitectureModel.Domain.FullName
                || t.Assembly.FullName == ArchitectureModel.Application.FullName)
            .SelectMany(t => Calls(t).Select(call => $"{t.FullName} calls {call}"))
            .Where(line => ForbiddenMembers.Any(line.Contains))
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    /// <summary>Every member a type's methods call, by full name.</summary>
    internal static IEnumerable<string> Calls(IType type) =>
        type.Dependencies.OfType<MethodCallDependency>().Select(d => d.TargetMember.FullName)
            .Concat(type.Members.SelectMany(m => m.MemberDependencies).OfType<MethodCallDependency>().Select(d => d.TargetMember.FullName));
}
