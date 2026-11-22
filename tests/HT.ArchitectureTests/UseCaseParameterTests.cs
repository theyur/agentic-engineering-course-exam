using System.Reflection;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using Xunit;

namespace HT.ArchitectureTests;

/// <summary>
/// Research R-12, constitution III: the parameters of the habit use cases and ports, checked by reflection. The
/// public catalog takes no acting user, or the acting user first for its goal information (slice 4 research R-09);
/// use cases for signed-in users take the acting user first and never an
/// account identifier; the custom-habit port and the goal port take the owner first, and the record port the owner
/// and then the goal (slice 5 research R-13).
/// </summary>
public sealed class UseCaseParameterTests
{
    private const string CountAcrossOwners = "CountUsingCatalogHabitAsync";

    private static IEnumerable<MethodInfo> PublicMethods(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName);

    private static bool Takes(MethodInfo method, Type type) =>
        method.GetParameters().Any(p => p.ParameterType == type || Nullable.GetUnderlyingType(p.ParameterType) == type);

    public static TheoryData<Type> SignedInServices =>
        new(typeof(CatalogAdministrationService), typeof(CustomHabitService), typeof(GoalService), typeof(ProfileService),
            typeof(RecordingService), typeof(ReportService), typeof(ExcusalService), typeof(PauseService), typeof(GoalChangeService),
            typeof(AttemptService));

    [Fact]
    public void Every_public_method_of_GoalChangeService_takes_the_acting_user_first_and_the_cancellation_token_last()
    {
        // Slice 8 (T061, contracts/application-api.md "GoalChangeService"): every use case is async, the acting user
        // first and the token last.
        var methods = PublicMethods(typeof(GoalChangeService)).ToList();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => m.GetParameters() is not [{ } first, .., { } last]
                || first.ParameterType != typeof(ActingUser)
                || last.ParameterType != typeof(CancellationToken)
                || !typeof(Task).IsAssignableFrom(m.ReturnType))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Theory]
    [InlineData(nameof(GoalChangeService.ChangeSettingsAsync))]
    [InlineData(nameof(GoalChangeService.EndAsync))]
    [InlineData(nameof(GoalChangeService.GetNewAttemptValuesAsync))]
    [InlineData(nameof(GoalChangeService.StartAttemptAsync))]
    public void GoalChangeService_has_the_use_case(string name)
    {
        // Slice 8 (T061, T071): each use case of contracts/application-api.md "GoalChangeService" is a public method, so
        // the rule above covers it.
        Assert.Single(PublicMethods(typeof(GoalChangeService)), m => m.Name == name);
    }

    [Fact]
    public void Every_public_method_of_AttemptService_takes_the_acting_user_first_and_the_cancellation_token_last()
    {
        // Slice 8 (T083, contracts/application-api.md "AttemptService"): every use case is async, the acting user first and
        // the token last.
        var methods = PublicMethods(typeof(AttemptService)).ToList();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => m.GetParameters() is not [{ } first, .., { } last]
                || first.ParameterType != typeof(ActingUser)
                || last.ParameterType != typeof(CancellationToken)
                || !typeof(Task).IsAssignableFrom(m.ReturnType))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Theory]
    [InlineData(nameof(AttemptService.GetAttemptsAsync))]
    [InlineData(nameof(AttemptService.GetAttemptAsync))]
    [InlineData(nameof(AttemptService.GetGoalHistoryAsync))]
    public void AttemptService_has_the_use_case(string name)
    {
        // Slice 8 (T083, T093): each use case of contracts/application-api.md "AttemptService" is a public method,
        // so the rule above covers it.
        Assert.Single(PublicMethods(typeof(AttemptService)), m => m.Name == name);
    }

    [Fact]
    public void Every_method_of_the_custom_habit_port_takes_the_owner_first()
    {
        var methods = typeof(ICustomHabitStore).GetMethods();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => m.GetParameters() is not [{ } first, ..] || first.ParameterType != typeof(AccountId))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void Every_method_of_the_goal_port_takes_the_owner_first_except_the_count_across_owners()
    {
        var methods = typeof(IGoalStore).GetMethods();
        Assert.NotEmpty(methods);

        // The one exception (slice 4 research R-08, R-13): a count across owners for the unpublish confirmation.
        var offenders = methods
            .Where(m => m.Name != CountAcrossOwners)
            .Where(m => m.GetParameters() is not [{ } first, ..] || first.ParameterType != typeof(AccountId))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void Every_method_of_the_record_port_takes_the_owner_first_and_the_goal_second_with_no_exception()
    {
        var methods = typeof(IRecordStore).GetMethods();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => m.GetParameters() is not [{ } first, { } second, ..]
                || first.ParameterType != typeof(AccountId)
                || second.ParameterType != typeof(GoalId))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Theory]
    [MemberData(nameof(SignedInServices))]
    public void Every_public_method_takes_the_acting_user_first_and_no_account_identifier(Type service)
    {
        var methods = PublicMethods(service).ToList();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => m.GetParameters() is not [{ } first, ..] || first.ParameterType != typeof(ActingUser) || Takes(m, typeof(AccountId)))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void Every_public_method_of_CatalogService_takes_no_acting_user_or_the_acting_user_first_and_no_account_identifier()
    {
        // Slice 4 research R-09: the slice 2 overloads take no acting user (every audience, FR-019); the overloads with
        // the user's own goal information take the acting user first (FR-031, FR-032).
        var methods = PublicMethods(typeof(CatalogService)).ToList();
        Assert.NotEmpty(methods);

        var offenders = methods
            .Where(m => Takes(m, typeof(AccountId))
                || (Takes(m, typeof(ActingUser)) && (m.GetParameters() is not [{ } first, ..] || first.ParameterType != typeof(ActingUser))))
            .Select(m => m.Name)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void CatalogService_has_both_kinds_of_browsing_and_details()
    {
        var methods = PublicMethods(typeof(CatalogService)).ToList();

        foreach (var name in new[] { nameof(CatalogService.BrowseAsync), nameof(CatalogService.GetDetailsAsync) })
        {
            var overloads = methods.Where(m => m.Name == name).ToList();
            Assert.Contains(overloads, m => !Takes(m, typeof(ActingUser)));
            Assert.Contains(overloads, m => m.GetParameters() is [{ } first, ..] && first.ParameterType == typeof(ActingUser));
        }
    }
}
