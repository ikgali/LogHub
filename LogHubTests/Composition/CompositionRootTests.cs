using LogHub.Composition;
using LogHub.Config;
using LogHub.Import;
using LogHub.Parsing;
using LogHub.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace LogHubTests.Composition;

public class CompositionRootTests
{
    [Test]
    public void Build_ConsoleProfile_RegistersConsoleSink()
    {
        using var provider = Build();

        Assert.That(provider.GetRequiredService<ILogSink>(), Is.InstanceOf<ConsoleLogSink>());
    }

    [Test]
    public void Build_FileProfile_RegistersFileSink()
    {
        using var provider = Build(profile: "file");

        Assert.That(provider.GetRequiredService<ILogSink>(), Is.InstanceOf<FileLogSink>());
    }

    [Test]
    public void Build_UnknownProfile_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Build(profile: "telegram"));

        Assert.That(exception!.Message, Does.Contain("Неизвестный профиль"));
    }

    [Test]
    public void Build_SourceMissing_RegistersMultiEncodingFileImporter()
    {
        using var provider = Build();

        Assert.That(
            provider.GetRequiredService<ILogImporter>(),
            Is.InstanceOf<MultiEncodingFileImporter>());
    }

    [Test]
    public void Build_JsonSource_RegistersJsonLogImporter()
    {
        using var provider = Build(source: "json");

        Assert.That(
            provider.GetRequiredService<ILogImporter>(),
            Is.InstanceOf<JsonLogImporter>());
    }

    [Test]
    public void Build_UnknownSource_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => Build(source: "xml"));

        Assert.That(exception!.Message, Does.Contain("Неизвестный источник"));
    }

    private static ServiceProvider Build(string profile = "console", string source = "text")
    {
        var configService = new ConfigService();
        var config = new LogHubConfig(
            "data/sample-utf8.log",
            LogLevel.Info,
            profile,
            "data/out.log",
            source);
        return CompositionRoot.Build(config, configService);
    }
}
