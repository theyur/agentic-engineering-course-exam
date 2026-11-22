using System.Reflection;
using Xunit;

namespace HT.ArchitectureTests;

/// <summary>
/// No public or protected member of HT.Domain or HT.Application has a NodaTime type in its signature, generic
/// arguments included, so the AC surface (ADR 0003) and every page use BCL dates and instants only (ADR 0017,
/// research R-01, R-13).
/// </summary>
public sealed class PublicSurfaceTests
{
    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static TheoryData<string> SurfaceAssemblies =>
        new(ArchitectureModel.Domain.GetName().Name!, ArchitectureModel.Application.GetName().Name!);

    [Theory]
    [MemberData(nameof(SurfaceAssemblies))]
    public void NoPublicOrProtectedSignatureHasANodaTimeType(string assemblyName)
    {
        var assembly = new[] { ArchitectureModel.Domain, ArchitectureModel.Application }.Single(a => a.GetName().Name == assemblyName);

        Assert.Empty(assembly.GetExportedTypes().SelectMany(Violations));
    }

    /// <summary>
    /// Slice 5 (research R-13): the new public types of <c>HT.Domain.Records</c>, <c>HT.Domain.Evaluation</c> and
    /// <c>HT.Application.Records</c> are in the surface checked above and have no NodaTime type in a signature. The
    /// namespaces must be exported and hold the named types, so moving them cannot make the rule pass on nothing.
    /// <c>NoteWrite</c> and <c>NoteWriteKind</c> join them with specs/006-note-editing (research R-11), and the report
    /// types of <c>HT.Domain.Reports</c> with specs/007-personal-reports (research R-14), and the exemption types of
    /// <c>HT.Domain.Exemptions</c> and <c>TrendMark</c> with specs/008-excusals-and-pauses (research R-11), with the
    /// store write of <c>HT.Application.Records</c> and the texts of <c>HT.Application.Exemptions</c>. The attempt and
    /// ending types of <c>HT.Domain.Goals</c> join them with specs/009-goal-changes (research R-12), with the use cases,
    /// views and results of <c>HT.Application.Goals</c> and <c>HT.Application.Reports</c> and the demo goal-change types
    /// of <c>HT.Application.Operator</c> (research R-09).
    /// </summary>
    [Theory]
    [InlineData("HT.Domain.Records", "RecordValue")]
    [InlineData("HT.Domain.Evaluation", "Evaluator")]
    [InlineData("HT.Application.Records", "RecordingText")]
    [InlineData("HT.Application.Records", "IRecordStore")]
    [InlineData("HT.Application.Records", "RecordPreview")]
    [InlineData("HT.Application.Records", "NoteWrite")]
    [InlineData("HT.Application.Records", "NoteWriteKind")]
    [InlineData("HT.Domain.Reports", "Consistency")]
    [InlineData("HT.Domain.Reports", "Streaks")]
    [InlineData("HT.Domain.Reports", "StreakUnit")]
    [InlineData("HT.Domain.Reports", "StreakUnits")]
    [InlineData("HT.Domain.Reports", "TrendKind")]
    [InlineData("HT.Domain.Reports", "TrendSpan")]
    [InlineData("HT.Domain.Reports", "TrendBar")]
    [InlineData("HT.Domain.Reports", "Trend")]
    [InlineData("HT.Domain.Reports", "TrendBuilder")]
    [InlineData("HT.Domain.Reports", "TrendMark")]
    [InlineData("HT.Domain.Exemptions", "ExemptionLimits")]
    [InlineData("HT.Domain.Exemptions", "ExemptionReason")]
    [InlineData("HT.Domain.Exemptions", "Excusal")]
    [InlineData("HT.Domain.Exemptions", "PauseId")]
    [InlineData("HT.Domain.Exemptions", "Pause")]
    [InlineData("HT.Domain.Exemptions", "PauseTiming")]
    [InlineData("HT.Domain.Exemptions", "GoalExemptions")]
    [InlineData("HT.Domain.Exemptions", "PauseProblem")]
    [InlineData("HT.Domain.Exemptions", "PauseResumption")]
    [InlineData("HT.Domain.Exemptions", "PauseRules")]
    [InlineData("HT.Application.Records", "ExemptionStoreWrite")]
    [InlineData("HT.Application.Records", "ExemptionStoreWriteOutcome")]
    [InlineData("HT.Application.Exemptions", "ExcusalTexts")]
    [InlineData("HT.Application.Exemptions", "PauseTexts")]
    [InlineData("HT.Application.Exemptions", "ExcusalService")]
    [InlineData("HT.Application.Exemptions", "ExcuseResult")]
    [InlineData("HT.Application.Exemptions", "RemoveExcusalResult")]
    [InlineData("HT.Application.Exemptions", "PauseService")]
    [InlineData("HT.Application.Exemptions", "PauseResult")]
    [InlineData("HT.Application.Exemptions", "ResumeResult")]
    [InlineData("HT.Application.Exemptions", "CancelPauseResult")]
    [InlineData("HT.Application.Exemptions", "PausesResult")]
    [InlineData("HT.Application.Exemptions", "PauseView")]
    [InlineData("HT.Domain.Goals", "Outcome")]
    [InlineData("HT.Domain.Goals", "EndingNote")]
    [InlineData("HT.Domain.Goals", "GoalEnding")]
    [InlineData("HT.Domain.Goals", "GoalAttemptVersions")]
    [InlineData("HT.Domain.Goals", "GoalSettingsChange")]
    [InlineData("HT.Domain.Goals", "GoalEndingEffect")]
    [InlineData("HT.Application.Goals", "GoalChangeTexts")]
    [InlineData("HT.Application.Records", "EntrySettings")]
    [InlineData("HT.Application.Goals", "GoalSettingsView")]
    [InlineData("HT.Application.Goals", "GoalChangeService")]
    [InlineData("HT.Application.Goals", "GoalSettingsInput")]
    [InlineData("HT.Application.Goals", "SettingsChangeResult")]
    [InlineData("HT.Application.Goals", "SettingsChangeOutcome")]
    [InlineData("HT.Application.Goals", "GoalEndResult")]
    [InlineData("HT.Application.Goals", "GoalEndOutcome")]
    [InlineData("HT.Application.Goals", "NewAttemptInput")]
    [InlineData("HT.Application.Goals", "NewAttemptValues")]
    [InlineData("HT.Application.Goals", "NewAttemptValuesResult")]
    [InlineData("HT.Application.Goals", "NewAttemptValuesOutcome")]
    [InlineData("HT.Application.Goals", "AttemptStartResult")]
    [InlineData("HT.Application.Goals", "AttemptStartOutcome")]
    [InlineData("HT.Application.Reports", "AttemptService")]
    [InlineData("HT.Application.Reports", "AttemptView")]
    [InlineData("HT.Application.Reports", "AttemptDetails")]
    [InlineData("HT.Application.Reports", "AttemptListResult")]
    [InlineData("HT.Application.Reports", "AttemptListOutcome")]
    [InlineData("HT.Application.Reports", "AttemptReadResult")]
    [InlineData("HT.Application.Reports", "AttemptReadOutcome")]
    [InlineData("HT.Application.Reports", "EndedAttemptView")]
    [InlineData("HT.Application.Reports", "GoalHistoryListResult")]
    [InlineData("HT.Application.Reports", "GoalHistoryListOutcome")]
    [InlineData("HT.Application.Operator", "DemoVersion")]
    [InlineData("HT.Application.Operator", "DemoAttempt")]
    [InlineData("HT.Application.Operator", "DemoGoalChangeGoal")]
    [InlineData("HT.Application.Operator", "DemoGoalChanges")]
    [InlineData("HT.Application.Operator", "IDemoGoalChangeStore")]
    [InlineData("HT.Application.Operator", "DemoGoalChangeService")]
    [InlineData("HT.Application.Operator", "DemoGoalChangeLoadingResult")]
    [InlineData("HT.Application.Operator", "DemoGoalChangeLoadingOutcome")]
    [InlineData("HT.Application.Operator", "DemoGoalChangeAccountResult")]
    public void TheNewRecordingTypesHaveNoNodaTimeTypeInTheirSignatures(string ns, string typeName)
    {
        var types = new[] { ArchitectureModel.Domain, ArchitectureModel.Application }
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.Namespace == ns)
            .ToList();

        Assert.Contains(types, t => t.Name == typeName);
        Assert.Empty(types.SelectMany(Violations));
    }

    /// <summary>The check sees every kind of signature, so a gap in it cannot pass silently.</summary>
    [Fact]
    public void TheCheckRecognisesEveryKindOfSignature()
    {
        var violations = Violations(typeof(Canary)).ToList();

        string[] expected =
        [
            "Field", "Property", "get_Property", "Return", "Parameter", "ProtectedParameter", ".ctor", "Event", "add_Event",
            "Generic", "Constrained", "Array", "ByRef", "CompareTo", "op_Implicit",
        ];
        Assert.All(expected, member => Assert.Contains($"{typeof(Canary).FullName}::{member}", violations));
        Assert.Contains(violations, line => line.EndsWith("implements System.IComparable`1[NodaTime.LocalDate]", StringComparison.Ordinal));
        Assert.DoesNotContain(violations, line => line.Contains("Hidden", StringComparison.Ordinal));
        Assert.Empty(Violations(typeof(CleanCanary)));
    }

    /// <summary>One line per public or protected signature of <paramref name="type"/> that mentions a NodaTime type.</summary>
    private static IEnumerable<string> Violations(Type type)
    {
        var name = type.FullName;
        if (type.BaseType is { } baseType && HasNodaTime(baseType))
        {
            yield return $"{name} derives from {baseType}";
        }

        foreach (var contract in type.GetInterfaces().Where(HasNodaTime))
        {
            yield return $"{name} implements {contract}";
        }

        foreach (var member in type.GetMembers(Declared))
        {
            Type[] types = member switch
            {
                FieldInfo field when field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly => [field.FieldType],
                PropertyInfo property when property.GetAccessors(true).Any(Visible) =>
                    [property.PropertyType, .. property.GetIndexParameters().Select(p => p.ParameterType)],
                EventInfo e when e.AddMethod is { } add && Visible(add) => [e.EventHandlerType!],
                MethodBase method when Visible(method) => Signature(method),
                _ => [],
            };

            if (types.Any(HasNodaTime))
            {
                yield return $"{name}::{member.Name}";
            }
        }
    }

    /// <summary>Parameters, the return type and the constraints of generic parameters.</summary>
    private static Type[] Signature(MethodBase method)
    {
        Type[] returnType = method is MethodInfo info ? [info.ReturnType] : [];
        IEnumerable<Type> constraints = method.IsGenericMethodDefinition
            ? method.GetGenericArguments().SelectMany(argument => argument.GetGenericParameterConstraints())
            : [];
        return [.. method.GetParameters().Select(p => p.ParameterType), .. returnType, .. constraints];
    }

    /// <summary>Public, protected or protected internal: what another assembly can see.</summary>
    private static bool Visible(MethodBase method) => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;

    /// <summary>The type, its element type (array, by-ref, pointer) and its generic arguments, at any depth.</summary>
    private static bool HasNodaTime(Type type)
    {
        if (type.HasElementType)
        {
            return HasNodaTime(type.GetElementType()!);
        }

        if (type.Namespace is { } ns && (ns == "NodaTime" || ns.StartsWith("NodaTime.", StringComparison.Ordinal)))
        {
            return true;
        }

        return type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericArguments().Any(HasNodaTime);
    }

    /// <summary>One visible signature of each kind with a NodaTime type, and hidden ones, which are allowed.</summary>
    public class Canary : IComparable<NodaTime.LocalDate>
    {
        public readonly NodaTime.Instant Field = NodaTime.Instant.MinValue;

        public Canary(NodaTime.Duration duration) => _ = duration;

        public static event EventHandler<NodaTime.Period>? Event
        {
            add => _ = value;
            remove => _ = value;
        }

        public static NodaTime.LocalDate Property => NodaTime.LocalDate.MinIsoValue;

        public static implicit operator NodaTime.Instant(Canary canary) => canary.Field;

        public static NodaTime.Instant Return() => NodaTime.Instant.MinValue;

        public static void Parameter(NodaTime.Instant instant) => _ = instant;

        public static IReadOnlyList<KeyValuePair<string, NodaTime.LocalDate?>> Generic() => [];

        public static void Constrained<T>(T value)
            where T : IComparable<NodaTime.Instant> => _ = value;

        public static NodaTime.Offset[] Array() => [];

        public static void ByRef(out NodaTime.Instant instant) => instant = NodaTime.Instant.MinValue;

        public int CompareTo(NodaTime.LocalDate other) => Property.CompareTo(other);

        internal static NodaTime.Instant InternalHidden() => PrivateHidden();

        protected static void ProtectedParameter(NodaTime.DateTimeZone zone) => _ = zone;

        private static NodaTime.Instant PrivateHidden() => NodaTime.Instant.MaxValue;
    }

    /// <summary>A surface with BCL types only; its internal NodaTime member is allowed.</summary>
    public class CleanCanary
    {
        public DateOnly Date { get; } = DateOnly.MinValue;

        public static DateTimeOffset At(DateOnly date) => new(date, TimeOnly.MinValue, TimeSpan.Zero);

        internal static NodaTime.Instant InternalHidden() => NodaTime.Instant.MinValue;
    }
}
