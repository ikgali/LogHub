using System.Text;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Import;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;

namespace LogHubTests.Import;

public class ImporterStrategyTests
{
    [Test]
    public void Run_ReplacingImporter_ChangesProcessorResult()
    {
        var firstSink = new RecordingSink();
        var secondSink = new RecordingSink();

        var firstResult = CreateProcessor(
            new InMemoryLogImporter("[2026-11-22 10:00:00] INFO Из памяти"),
            firstSink).Run();
        var secondResult = CreateProcessor(
            new InMemoryLogImporter("[2026-11-22 10:00:00] INFO Из другого источника"),
            secondSink).Run();

        Assert.Multiple(() =>
        {
            Assert.That(firstResult.Written, Is.EqualTo(1));
            Assert.That(secondResult.Written, Is.EqualTo(1));
            Assert.That(firstSink.Lines, Is.Not.EqualTo(secondSink.Lines));
            Assert.That(firstSink.Lines[0], Does.Contain("Из памяти"));
            Assert.That(secondSink.Lines[0], Does.Contain("Из другого источника"));
        });
    }

    [Test]
    public void InMemoryAndJsonImporters_ForSameData_ProduceEqualEntries()
    {
        const string jsonLine =
            "{\"timestamp\":\"2026-11-22 10:00:00\",\"level\":\"INFO\",\"message\":\"Одинаковая запись\"}";
        var inMemoryEntries = new InMemoryLogImporter(
            "[2026-11-22 10:00:00] INFO Одинаковая запись").Import();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonLine + "\n"));
        var jsonEntries = new JsonLogImporter(stream).Import();

        Assert.That(jsonEntries, Is.EqualTo(inMemoryEntries));
    }

    [Test]
    public void LogProcessor_Constructor_DependsOnImporterAbstractionOnly()
    {
        var constructor = typeof(LogProcessor).GetConstructors().Single();
        var parameterTypes = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(parameterTypes, Does.Contain(typeof(ILogImporter)));
            Assert.That(parameterTypes, Does.Not.Contain(typeof(MultiEncodingFileImporter)));
            Assert.That(parameterTypes, Does.Not.Contain(typeof(JsonLogImporter)));
            Assert.That(parameterTypes, Does.Not.Contain(typeof(InMemoryLogImporter)));
        });
    }

    private static LogProcessor CreateProcessor(ILogImporter importer, ILogSink sink) =>
        new(importer, LogFilters.ByMinLevel(LogLevel.Debug), new LogFormatter(), [sink]);
}
