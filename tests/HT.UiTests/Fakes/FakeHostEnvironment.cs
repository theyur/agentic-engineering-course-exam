using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace HT.UiTests.Fakes;

/// <summary>A hand-written <see cref="IHostEnvironment"/> with a chosen environment name.</summary>
public sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;

    public string ApplicationName { get; set; } = "HT.Ui";

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
