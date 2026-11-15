using System.Reflection;
using HT.Application.Operator;
using HT.SmokeCheck;

namespace HT.UiTests.SmokeChecks;

/// <summary>
/// The coverage rule of contracts/smoke-check.md (research R-06, R-11): every data parameter of every method of the
/// demo data port <see cref="IDemoDataStore"/> has an entry in <see cref="SmokeCheckCoverage.Entries"/>, and every
/// entry still matches a parameter. A slice that adds demo data changes the port, so this test fails until it
/// extends the smoke check.
/// </summary>
public sealed class CoverageTests
{
    private const string Rule = "see specs/003-demo-hosting/contracts/smoke-check.md \"Coverage rule\"";

    public static TheoryData<string, string> PortParameters()
    {
        var data = new TheoryData<string, string>();
        foreach (var (method, parameter) in DataParameters())
        {
            data.Add(method.Name, parameter.Name!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PortParameters))]
    public void Every_data_parameter_of_the_demo_data_port_has_a_coverage_entry(string method, string parameter)
    {
        var type = DataParameters().Single(p => p.Method.Name == method && p.Parameter.Name == parameter).Parameter.ParameterType;

        var entry = SmokeCheckCoverage.Entries.SingleOrDefault(e => e.Method == method && e.Parameter == parameter);

        Assert.True(entry is not null,
            $"IDemoDataStore.{method}({type.Name} {parameter}) has no SmokeCheckCoverage entry: extend the smoke check and add one ({Rule}).");
        Assert.True(entry.Type == type,
            $"The SmokeCheckCoverage entry of IDemoDataStore.{method}({parameter}) names type {entry.Type.Name}, the port has {type.Name}: check that the smoke check still verifies it and update the entry ({Rule}).");
    }

    [Fact]
    public void Every_coverage_entry_matches_a_data_parameter_of_the_demo_data_port()
    {
        var parameters = DataParameters().ToList();

        Assert.All(SmokeCheckCoverage.Entries, entry => Assert.True(
            parameters.Any(p => p.Method.Name == entry.Method && p.Parameter.Name == entry.Parameter && p.Parameter.ParameterType == entry.Type),
            $"The SmokeCheckCoverage entry {entry.Method}({entry.Type.Name} {entry.Parameter}) matches no parameter of IDemoDataStore: remove or update it ({Rule})."));
    }

    [Fact]
    public void Every_coverage_entry_names_the_check_that_verifies_it()
    {
        Assert.All(SmokeCheckCoverage.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Check)));
        Assert.Equal(
            SmokeCheckCoverage.Entries.Count,
            SmokeCheckCoverage.Entries.Select(e => (e.Method, e.Parameter)).Distinct().Count());
    }

    /// <summary>Every parameter of every method of the port, except the <see cref="CancellationToken"/>.</summary>
    private static IEnumerable<(MethodInfo Method, ParameterInfo Parameter)> DataParameters() =>
        typeof(IDemoDataStore).GetMethods()
            .SelectMany(method => method.GetParameters().Select(parameter => (method, parameter)))
            .Where(p => p.parameter.ParameterType != typeof(CancellationToken));
}
