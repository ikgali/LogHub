using LogHub.Config;
using LogHub.Parsing;

namespace LogHubTests.Config;

public class LogHubConfigTests
{
    [Test]
    public void From_AllValuesProvided_ReadsThem()
    {
        var config = CreateConfig(
            ("input", "data/custom.log"),
            ("minLevel", "WARN"),
            ("profile", "file"),
            ("output", "data/custom-out.log"),
            ("source", "json"));

        var result = LogHubConfig.From(config);

        Assert.Multiple(() =>
        {
            Assert.That(result.InputPath, Is.EqualTo("data/custom.log"));
            Assert.That(result.MinLevel, Is.EqualTo(LogLevel.Warning));
            Assert.That(result.Profile, Is.EqualTo("file"));
            Assert.That(result.OutputPath, Is.EqualTo("data/custom-out.log"));
            Assert.That(result.Source, Is.EqualTo("json"));
        });
    }

    [Test]
    public void From_ProfileMissing_DefaultsToConsole()
    {
        var config = CreateConfig(("input", "data/sample-utf8.log"));

        Assert.That(LogHubConfig.From(config).Profile, Is.EqualTo("console"));
    }

    [Test]
    public void From_MinLevelMissing_DefaultsToInfo()
    {
        var config = CreateConfig(("input", "data/sample-utf8.log"));

        Assert.That(LogHubConfig.From(config).MinLevel, Is.EqualTo(LogLevel.Info));
    }

    [Test]
    public void From_SourceMissing_DefaultsToText()
    {
        var config = CreateConfig(("input", "data/sample-utf8.log"));

        Assert.That(LogHubConfig.From(config).Source, Is.EqualTo("text"));
    }

    [Test]
    public void From_InputAndOutputMissing_UseSampleDefaults()
    {
        var config = CreateConfig();

        var result = LogHubConfig.From(config);

        Assert.Multiple(() =>
        {
            Assert.That(result.InputPath, Is.EqualTo("data/sample-utf8.log"));
            Assert.That(result.OutputPath, Is.EqualTo("data/out.log"));
        });
    }

    private static ConfigService CreateConfig(params (string Key, string Value)[] values)
    {
        var config = new ConfigService();
        foreach (var (key, value) in values)
        {
            config.SetValue(key, value);
        }

        return config;
    }
}
