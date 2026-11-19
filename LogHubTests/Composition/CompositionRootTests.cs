using LogHub.Composition;
using LogHub.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace LogHubTests.Composition;

public class CompositionRootTests
{
    [Test]
    public void Build_ConsoleProfile_RegistersConsoleSink()
    {
        using var config = new TempConfig("profile=console");
        using var provider = CompositionRoot.Build(config.Path);

        Assert.That(provider.GetRequiredService<ILogSink>(), Is.InstanceOf<ConsoleLogSink>());
    }

    [Test]
    public void Build_FileProfile_RegistersFileSink()
    {
        using var config = new TempConfig("profile=file");
        using var provider = CompositionRoot.Build(config.Path);

        Assert.That(provider.GetRequiredService<ILogSink>(), Is.InstanceOf<FileLogSink>());
    }

    [Test]
    public void Build_UnknownProfile_ThrowsInvalidOperationException()
    {
        using var config = new TempConfig("profile=telegram");

        var exception = Assert.Throws<InvalidOperationException>(
            () => CompositionRoot.Build(config.Path));

        Assert.That(exception!.Message, Does.Contain("Неизвестный профиль"));
    }

    private sealed class TempConfig : IDisposable
    {
        public TempConfig(string profileLine)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"loghub-config-{Guid.NewGuid():N}.txt");
            File.WriteAllLines(Path, ["input=data/sample-utf8.log", "minLevel=INFO", profileLine]);
        }

        public string Path { get; }

        public void Dispose() => File.Delete(Path);
    }
}
