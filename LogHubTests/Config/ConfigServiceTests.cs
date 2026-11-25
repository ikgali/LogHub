using LogHub.Config;

namespace LogHubTests.Config;

public class ConfigServiceTests
{
    [Test]
    public void GetValue_MissingKey_ReturnsNull()
    {
        var service = new ConfigService();

        Assert.That(service.GetValue("profile"), Is.Null);
    }

    [Test]
    public void SetValue_ThenGetValue_ReturnsStoredValue()
    {
        var service = new ConfigService();

        service.SetValue("profile", "file");

        Assert.That(service.GetValue("profile"), Is.EqualTo("file"));
    }

    [Test]
    public void SetValue_SameKeyTwice_OverwritesValue()
    {
        var service = new ConfigService();

        service.SetValue("minLevel", "INFO");
        service.SetValue("minLevel", "ERROR");

        Assert.That(service.GetValue("minLevel"), Is.EqualTo("ERROR"));
    }

    [Test]
    public void GetValue_DifferentKeyCase_IsCaseInsensitive()
    {
        var service = new ConfigService();

        service.SetValue("MinLevel", "WARN");

        Assert.That(service.GetValue("minlevel"), Is.EqualTo("WARN"));
    }

    [Test]
    public void EachTest_OwnInstance_DoesNotSeeStateFromOtherTests()
    {
        var service = new ConfigService();

        Assert.That(service.GetValue("profile"), Is.Null);

        service.SetValue("profile", "console");
    }

    [Test]
    public void ConstructorInjection_WithFake_ReturnsFakeValue_WithoutStaticReset()
    {
        var consumer = new ConfigConsumer(new FakeConfigService("file"));

        Assert.That(consumer.Profile, Is.EqualTo("file"));
    }

    private sealed class ConfigConsumer
    {
        private readonly IConfigService _config;

        public ConfigConsumer(IConfigService config)
        {
            _config = config;
        }

        public string Profile => _config.GetValue("profile") ?? "console";
    }

    private sealed class FakeConfigService : IConfigService
    {
        private readonly string _profile;

        public FakeConfigService(string profile)
        {
            _profile = profile;
        }

        public string? GetValue(string key) =>
            string.Equals(key, "profile", StringComparison.OrdinalIgnoreCase) ? _profile : null;

        public void SetValue(string key, string value)
        {
            // Подмена только для чтения: тесту не нужно ничего сбрасывать.
        }
    }
}
