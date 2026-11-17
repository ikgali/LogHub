using LogHub.Parsing;

namespace LogHubTests;

public class LogLineParserTests
{
    [Test]
    public void TryParse_ValidLine_ReturnsTrue_AndParsesAllFields()
    {
        var parser = new LogLineParser();
        const string line = "[2026-09-15 10:23:11] INFO Сервис запущен";

        var parsed = parser.TryParse(line, out var entry);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(entry.Timestamp, Is.EqualTo(new DateTime(2026, 9, 15, 10, 23, 11)));
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(entry.Message, Is.EqualTo("Сервис запущен"));
        });
    }

    [Test]
    public void TryParse_LineWithoutTimestamp_ReturnsFalse()
    {
        var parser = new LogLineParser();

        var parsed = parser.TryParse("INFO Сервис запущен", out _);

        Assert.That(parsed, Is.False);
    }

    [Test]
    public void TryParse_UnknownLevel_ReturnsFalse()
    {
        var parser = new LogLineParser();

        var parsed = parser.TryParse("[2026-09-15 10:23:11] TRACE Подробности", out _);

        Assert.That(parsed, Is.False);
    }

    [Test]
    public void TryParse_EmptyOrGarbageLine_ReturnsFalse()
    {
        var parser = new LogLineParser();

        Assert.Multiple(() =>
        {
            Assert.That(parser.TryParse(string.Empty, out _), Is.False);
            Assert.That(parser.TryParse("### corrupted ###", out _), Is.False);
            Assert.That(parser.TryParse("[2026-09-15 10:23:11] INFO", out _), Is.False);
        });
    }

    [Test]
    public void TryParse_CyrillicMessage_IsPreserved()
    {
        var parser = new LogLineParser();
        const string line = "[2026-09-15 10:23:11] WARN Диск заполнен на 91 процент";

        var parsed = parser.TryParse(line, out var entry);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(entry.Message, Is.EqualTo("Диск заполнен на 91 процент"));
        });
    }
}
