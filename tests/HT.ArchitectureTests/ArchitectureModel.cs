using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using HT.Application;
using HT.Domain.Accounts;
using HT.Persistence;

namespace HT.ArchitectureTests;

/// <summary>The one architecture model every rule checks: the four production assemblies.</summary>
internal static class ArchitectureModel
{
    public static readonly System.Reflection.Assembly Domain = typeof(AccountId).Assembly;
    public static readonly System.Reflection.Assembly Application = typeof(ApplicationServiceCollectionExtensions).Assembly;
    public static readonly System.Reflection.Assembly Persistence = typeof(PersistenceAssemblyMarker).Assembly;
    public static readonly System.Reflection.Assembly Ui = typeof(HT.Ui.Components.App).Assembly;

    public static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Domain, Application, Persistence, Ui)
        .Build();
}
