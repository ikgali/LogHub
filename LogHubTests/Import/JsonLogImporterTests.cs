using System.Text;
using LogHub.Import;
using LogHub.Parsing;

namespace LogHubTests.Import;

public class JsonLogImporterTests
{
    [Test]
    public void Import_ValidJsonLines_ReturnsParsedEntries()
    {
        const string json =
            "{\"timestamp\":\"2026-11-22 10:00:00\",\"level\":\"INFO\",\"message\":\"Первая запись\"}\n" +
            "{\"timestamp\":\"2026-11-22 10:00:01\",\"level\":\"ERROR\",\"message\":\"Вторая запись\"}\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var importer = new JsonLogImporter(stream);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(entries[0].Timestamp, Is.EqualTo(new DateTime(2026, 11, 22, 10, 0, 0)));
            Assert.That(entries[0].Level, Is.EqualTo(LogLevel.Info));
            Assert.That(entries[1].Message, Is.EqualTo("Вторая запись"));
            Assert.That(importer.SkippedCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void Import_BrokenJsonLine_IsSkippedAndCounted()
    {
        const string json =
            "{\"timestamp\":\"2026-11-22 10:00:00\",\"level\":\"INFO\",\"message\":\"Корректная\"}\n" +
            "{\"timestamp\":\"2026-11-22 10:00:01\",\"level\":\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var importer = new JsonLogImporter(stream);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(importer.SkippedCount, Is.EqualTo(1));
        });
    }
}
