using System.Globalization;
using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Dependencies;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HT.ArchitectureTests;

/// <summary>
/// Constitution II: the domain and the use cases get the time and the culture supplied, never read them. The
/// culture-sensitive base-library members read the current culture inside the library, so they are forbidden too
/// (FR-004, research R-12).
/// </summary>
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
        "System.Globalization.CultureInfo::get_CurrentUICulture(",
        "System.String::ToUpper()",
        "System.String::ToLower()",
        "System.Char::ToUpper(System.Char)",
        "System.Char::ToLower(System.Char)",
        "System.String::Compare(System.String,System.String)",
        "System.String::Compare(System.String,System.String,System.Boolean)",
        "System.String::CompareTo(",
        "System.StringComparer::get_CurrentCulture(",
        "System.StringComparer::get_CurrentCultureIgnoreCase(",
        "NodaTime.SystemClock::get_Instance(",
        "NodaTime.DateTimeZoneProviders::get_Bcl(",
        "System.TimeZoneInfo::FindSystemTimeZoneById(",
        "System.TimeZoneInfo::GetSystemTimeZones(",
        "System.TimeZoneInfo::ConvertTime",
    ];

    /// <summary>The machine's time-zone rules as a NodaTime zone (slice 4 research R-01, R-13).</summary>
    private const string BclDateTimeZone = "NodaTime.TimeZones.BclDateTimeZone";

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

    /// <summary>Each forbidden member name matches a real call, so a misspelt entry cannot pass silently.</summary>
    [Fact]
    public void EveryForbiddenMemberIsRecognisedInACall()
    {
        var canary = new ArchLoader().LoadAssemblies(typeof(Canary).Assembly).Build().Types
            .Single(t => t.FullName == typeof(Canary).FullName);
        var calls = Calls(canary).ToList();

        var missed = ForbiddenMembers.Where(member => !calls.Any(call => call.Contains(member, StringComparison.Ordinal))).ToList();
        Assert.Empty(missed);

        Assert.DoesNotContain(calls, call => call.Contains("ToUpperInvariant", StringComparison.Ordinal)
            && ForbiddenMembers.Any(call.Contains));
    }

    /// <summary>
    /// Every time-zone conversion goes through the embedded tzdb (research R-01): the type that wraps the machine's
    /// rules is forbidden. The call matcher above cannot see a type, hence this rule.
    /// </summary>
    [Fact]
    public void DomainAndApplicationMustNotUseTheMachinesTimeZoneRules()
    {
        BclDateTimeZoneRule(Types().That().ResideInAssembly(ArchitectureModel.Domain).Or().ResideInAssembly(ArchitectureModel.Application))
            .Check(ArchitectureModel.Architecture);
    }

    /// <summary>The rule above recognises a dependency on the type, so a misspelt name cannot pass silently.</summary>
    [Fact]
    public void TheBclDateTimeZoneRuleRecognisesTheType()
    {
        var canaryArchitecture = new ArchLoader().LoadAssemblies(typeof(BclCanary).Assembly).Build();

        Assert.False(BclDateTimeZoneRule(Types().That().Are(typeof(BclCanary))).HasNoViolations(canaryArchitecture));
    }

    private static TypesShouldConjunctionWithDescription BclDateTimeZoneRule(GivenTypesConjunction types) =>
        types.Should().NotDependOnAny(Types(true).That().HaveFullName(BclDateTimeZone))
            .Because("time zones come only from NodaTime's embedded tzdb, never from the machine (research R-01)");

    /// <summary>Every member a type's methods call, by full name.</summary>
    internal static IEnumerable<string> Calls(IType type) =>
        type.Dependencies.OfType<MethodCallDependency>().Select(d => d.TargetMember.FullName)
            .Concat(type.Members.SelectMany(m => m.MemberDependencies).OfType<MethodCallDependency>().Select(d => d.TargetMember.FullName));

    /// <summary>Calls every forbidden member once, and the allowed invariant ones beside them.</summary>
    private static class Canary
    {
#pragma warning disable CA1304, CA1305, CA1307, CA1309, CA1310, CA1311
        public static object[] Use(string text) =>
        [
            DateTime.Now, DateTime.UtcNow, DateTime.Today, DateTimeOffset.Now, DateTimeOffset.UtcNow, TimeZoneInfo.Local,
            CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture,
            text.ToUpper(), text.ToLower(), char.ToUpper(text[0]), char.ToLower(text[0]),
            string.Compare(text, text), string.Compare(text, text, true), text.CompareTo(text),
            StringComparer.CurrentCulture, StringComparer.CurrentCultureIgnoreCase,
            text.ToUpperInvariant(), string.CompareOrdinal(text, text),
            NodaTime.SystemClock.Instance, NodaTime.DateTimeZoneProviders.Bcl,
            TimeZoneInfo.FindSystemTimeZoneById(text), TimeZoneInfo.GetSystemTimeZones(),
            TimeZoneInfo.ConvertTime(DateTimeOffset.MinValue, TimeZoneInfo.Utc),
        ];
#pragma warning restore CA1304, CA1305, CA1307, CA1309, CA1310, CA1311
    }

    /// <summary>Depends on the machine-rules type once.</summary>
    private static class BclCanary
    {
        public static NodaTime.TimeZones.BclDateTimeZone? Zone { get; set; }
    }
}
