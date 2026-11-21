using System.Reflection;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.SmokeCheck;

namespace HT.UiTests.SmokeChecks;

/// <summary>
/// The coverage rule of contracts/smoke-check.md (research R-06, R-11; slice 5 research R-10): every data parameter of
/// every method of the demo data ports <see cref="IDemoDataStore"/>, <see cref="IDemoHistoryStore"/>,
/// <see cref="IDemoReportStore"/> (slice 6 research R-11) and <see cref="IDemoPauseStore"/> (slice 7 HP-011) has an entry in
/// <see cref="SmokeCheckCoverage.Entries"/> with its port, and every entry still matches a parameter of its port. A slice
/// that adds demo data changes a port, so this test fails until it extends the smoke check.
/// </summary>
public sealed class CoverageTests
{
    private const string Rule = "see specs/003-demo-hosting/contracts/smoke-check.md \"Coverage rule\"";

    /// <summary>The demo data ports: every demo write goes through one of them.</summary>
    private static readonly IReadOnlyList<Type> Ports =
        [typeof(IDemoDataStore), typeof(IDemoHistoryStore), typeof(IDemoReportStore), typeof(IDemoPauseStore)];

    public static TheoryData<string, string, string> PortParameters()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var (port, method, parameter) in DataParameters())
        {
            data.Add(port.Name, method.Name, parameter.Name!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PortParameters))]
    public void Every_data_parameter_of_a_demo_data_port_has_a_coverage_entry(string port, string method, string parameter)
    {
        var type = DataParameters()
            .Single(p => p.Port.Name == port && p.Method.Name == method && p.Parameter.Name == parameter)
            .Parameter.ParameterType;

        var entry = SmokeCheckCoverage.Entries.SingleOrDefault(e => e.Port.Name == port && e.Method == method && e.Parameter == parameter);

        Assert.True(entry is not null,
            $"{port}.{method}({type.Name} {parameter}) has no SmokeCheckCoverage entry: extend the smoke check and add one ({Rule}).");
        Assert.True(entry.Type == type,
            $"The SmokeCheckCoverage entry of {port}.{method}({parameter}) names type {entry.Type.Name}, the port has {type.Name}: check that the smoke check still verifies it and update the entry ({Rule}).");
    }

    [Fact]
    public void Every_coverage_entry_matches_a_data_parameter_of_its_demo_data_port()
    {
        var parameters = DataParameters().ToList();

        Assert.All(SmokeCheckCoverage.Entries, entry => Assert.True(
            parameters.Any(p => p.Port == entry.Port && p.Method.Name == entry.Method && p.Parameter.Name == entry.Parameter && p.Parameter.ParameterType == entry.Type),
            $"The SmokeCheckCoverage entry {entry.Port.Name}.{entry.Method}({entry.Type.Name} {entry.Parameter}) matches no parameter of a demo data port: remove or update it ({Rule})."));
    }

    [Fact]
    public void Every_coverage_entry_names_the_check_that_verifies_it()
    {
        Assert.All(SmokeCheckCoverage.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Check)));
        Assert.Equal(
            SmokeCheckCoverage.Entries.Count,
            SmokeCheckCoverage.Entries.Select(e => (e.Port, e.Method, e.Parameter)).Distinct().Count());
    }

    [Fact]
    public void Slice_4_adds_the_profile_and_the_goals_of_a_demo_account_with_check_5()
    {
        // Slice 4 HP-018, contracts/host.md "Smoke check": the two new data parameters of CreateDemoAccountAsync.
        Assert.Contains(
            new CoverageEntry(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "profile", typeof(DemoAccountProfile), "5 demo <email>: the account name on Profile"),
            SmokeCheckCoverage.Entries);
        Assert.Contains(
            new CoverageEntry(typeof(IDemoDataStore), nameof(IDemoDataStore.CreateDemoAccountAsync), "goals", typeof(IReadOnlyList<DemoGoal>), "5 demo <email>: the goal names on Goals"),
            SmokeCheckCoverage.Entries);
        Assert.Equal(6, SmokeCheckCoverage.Entries.Count(e => e.Port == typeof(IDemoDataStore)));
    }

    [Fact]
    public void Slice_5_adds_the_demo_history_port_with_check_6()
    {
        // Slice 5 contracts/host.md "Smoke check": the 4 data parameters of CreateDemoHistoryAccountAsync.
        const string method = nameof(IDemoHistoryStore.CreateDemoHistoryAccountAsync);
        CoverageEntry[] expected =
        [
            new(typeof(IDemoHistoryStore), method, "email", typeof(Email), "6 demo history: the sign-in"),
            new(typeof(IDemoHistoryStore), method, "password", typeof(string), "6 demo history: the sign-in"),
            new(typeof(IDemoHistoryStore), method, "profile", typeof(DemoAccountProfile), "6 demo history: the sign-in"),
            new(typeof(IDemoHistoryStore), method, "goals", typeof(IReadOnlyList<DemoHistoryGoal>),
                "6 demo history: the goal names, and the states, partial amount and note on the goal pages"),
        ];

        Assert.Equal(expected, SmokeCheckCoverage.Entries.Where(e => e.Port == typeof(IDemoHistoryStore)));
        Assert.Equal(10, SmokeCheckCoverage.Entries.Count(e => e.Port != typeof(IDemoReportStore) && e.Port != typeof(IDemoPauseStore)));
    }

    [Fact]
    public void Slice_6_adds_the_demo_report_port_with_check_7()
    {
        // Slice 6 contracts/host.md "Smoke check": the 4 data parameters of CreateDemoReportAccountAsync, 14 entries in all.
        const string method = nameof(IDemoReportStore.CreateDemoReportAccountAsync);
        CoverageEntry[] expected =
        [
            new(typeof(IDemoReportStore), method, "email", typeof(Email), "7 demo report: the sign-in"),
            new(typeof(IDemoReportStore), method, "password", typeof(string), "7 demo report: the sign-in"),
            new(typeof(IDemoReportStore), method, "profile", typeof(DemoAccountProfile), "7 demo report: the sign-in"),
            new(typeof(IDemoReportStore), method, "goals", typeof(IReadOnlyList<DemoHistoryGoal>),
                "7 demo report: the goal names on Goals, the weeks streak label on a goal page, and a Missed result this week on the Home"),
        ];

        Assert.Equal(expected, SmokeCheckCoverage.Entries.Where(e => e.Port == typeof(IDemoReportStore)));
        Assert.Equal(14, SmokeCheckCoverage.Entries.Count(e => e.Port != typeof(IDemoPauseStore)));
    }

    [Fact]
    public void Slice_7_adds_the_demo_pause_port_with_check_8()
    {
        // Slice 7 contracts/host.md "Smoke check": the 4 data parameters of CreateDemoPauseAccountAsync, 18 entries in all.
        const string method = nameof(IDemoPauseStore.CreateDemoPauseAccountAsync);
        CoverageEntry[] expected =
        [
            new(typeof(IDemoPauseStore), method, "email", typeof(Email), "8 demo pause: the sign-in"),
            new(typeof(IDemoPauseStore), method, "password", typeof(string), "8 demo pause: the sign-in"),
            new(typeof(IDemoPauseStore), method, "profile", typeof(DemoAccountProfile), "8 demo pause: the sign-in"),
            new(typeof(IDemoPauseStore), method, "goals", typeof(IReadOnlyList<DemoPauseGoal>),
                "8 demo pause: an Excused result on a goal page, and the paused goal with its Paused until meta on the Paused tab"),
        ];

        Assert.Equal(expected, SmokeCheckCoverage.Entries.Where(e => e.Port == typeof(IDemoPauseStore)));
        Assert.Equal(18, SmokeCheckCoverage.Entries.Count);
    }

    /// <summary>Every parameter of every method of each demo data port, except the <see cref="CancellationToken"/>.</summary>
    private static IEnumerable<(Type Port, MethodInfo Method, ParameterInfo Parameter)> DataParameters() =>
        Ports.SelectMany(port => port.GetMethods()
            .SelectMany(method => method.GetParameters().Select(parameter => (port, method, parameter))))
            .Where(p => p.parameter.ParameterType != typeof(CancellationToken));
}
