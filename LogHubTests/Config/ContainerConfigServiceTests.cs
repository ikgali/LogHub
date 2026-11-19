using LogHub.Config;
using Microsoft.Extensions.DependencyInjection;

namespace LogHubTests.Config;

[NonParallelizable]
public class ContainerConfigServiceTests
{
    [Test]
    public void GetValue_ProfileInFreshProvider_ReturnsNull()
    {
        using var provider = BuildProvider();

        Assert.That(provider.GetRequiredService<IConfigService>().GetValue("profile"), Is.Null);
    }

    [Test]
    public void SetValue_ThenGetValue_InSameProvider_ReturnsStoredValue()
    {
        using var provider = BuildProvider();

        provider.GetRequiredService<IConfigService>().SetValue("profile", "file");

        Assert.That(
            provider.GetRequiredService<IConfigService>().GetValue("profile"),
            Is.EqualTo("file"));
    }

    [Test]
    public void Resolve_TwiceFromSameProvider_ReturnsSameInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IConfigService>();
        var second = provider.GetRequiredService<IConfigService>();

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void Resolve_FromTwoProviders_ReturnsDifferentInstances()
    {
        using var firstProvider = BuildProvider();
        using var secondProvider = BuildProvider();

        var first = firstProvider.GetRequiredService<IConfigService>();
        var second = secondProvider.GetRequiredService<IConfigService>();

        Assert.That(second, Is.Not.SameAs(first));
    }

    [Test]
    public void Resolve_WithFakeRegistration_ReturnsFakeValue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfigService>(new FakeConfigService("file"));
        using var provider = services.BuildServiceProvider();

        Assert.That(
            provider.GetRequiredService<IConfigService>().GetValue("profile"),
            Is.EqualTo("file"));
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfigService, ConfigService>();
        return services.BuildServiceProvider();
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
        }
    }
}
