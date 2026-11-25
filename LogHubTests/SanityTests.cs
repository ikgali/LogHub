namespace LogHubTests;

public class SanityTests
{
    [Test]
    public void ProjectIsWired_Passes()
    {
        var sum = 1 + 1;

        Assert.That(sum, Is.EqualTo(2));
    }
}
