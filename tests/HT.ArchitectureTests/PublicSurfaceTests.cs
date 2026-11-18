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
