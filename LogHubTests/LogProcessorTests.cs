using System.Text;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Import;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;

namespace LogHubTests;

public class LogProcessorTests
{
    [Test]
    public void Run_AllValidLines_WritesFormattedLinesToSink_AndCountsWritten()
    {
        var sink = new RecordingSink();
        var processor = CreateProcessor(
            new InMemoryLogImporter(
                "[2026-09-15 10:00:01] INFO Первая строка",
                "[2026-09-15 10:00:02] WARN Вторая строка",
                "[2026-09-15 10:00:03] ERROR Третья строка"),
            LogLevel.Debug,
            sink);

        var result = processor.Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.Read, Is.EqualTo(3));
            Assert.That(result.Written, Is.EqualTo(3));
            Assert.That(result.Skipped, Is.EqualTo(0));
            Assert.That(sink.Lines, Is.EqualTo(new[]
            {
                "2026-09-15 10:00:01 [INFO] Первая строка",
                "2026-09-15 10:00:02 [WARN] Вторая строка",
                "2026-09-15 10:00:03 [ERROR] Третья строка"
            }));
        });
    }

    [Test]
    public void Run_MalformedLines_AreSkippedByImporter_AndOnlyValidEntriesAreProcessed()
    {
        var sink = new RecordingSink();
        var importer = new InMemoryLogImporter(
            "[2026-09-15 10:00:01] INFO Корректная строка",
            "### corrupted ###",
            string.Empty);
        var processor = CreateProcessor(importer, LogLevel.Debug, sink);

        var result = processor.Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.Read, Is.EqualTo(1));
            Assert.That(result.Written, Is.EqualTo(1));
            Assert.That(result.Skipped, Is.EqualTo(0));
            Assert.That(importer.SkippedCount, Is.EqualTo(2));
            Assert.That(sink.Lines, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Run_EntryBelowMinLevel_IsFilteredOut_AndCountedAsSkipped()
    {
        var sink = new RecordingSink();
        var processor = CreateProcessor(
            new InMemoryLogImporter(
                "[2026-09-15 10:00:01] DEBUG Отладочная строка",
                "[2026-09-15 10:00:02] INFO Информационная строка"),
            LogLevel.Info,
            sink);

        var result = processor.Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.Read, Is.EqualTo(2));
            Assert.That(result.Written, Is.EqualTo(1));
            Assert.That(result.Skipped, Is.EqualTo(1));
            Assert.That(sink.Lines, Is.EqualTo(new[] { "2026-09-15 10:00:02 [INFO] Информационная строка" }));
        });
    }

    [Test]
    public void Run_TwoSinks_BothReceiveSameLines()
    {
        var firstSink = new RecordingSink("first");
        var secondSink = new RecordingSink("second");
        var processor = CreateProcessor(
            new InMemoryLogImporter(
                "[2026-09-15 10:00:01] INFO Первая строка",
                "[2026-09-15 10:00:02] ERROR Вторая строка"),
            LogLevel.Debug,
            firstSink,
            secondSink);

        var result = processor.Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.Written, Is.EqualTo(2));
            Assert.That(firstSink.Lines, Is.EqualTo(secondSink.Lines));
            Assert.That(firstSink.Lines, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Run_EmptyInput_ReturnsZeroCounters()
    {
        var sink = new RecordingSink();
        var processor = CreateProcessor(new InMemoryLogImporter(), LogLevel.Debug, sink);

        var result = processor.Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.Read, Is.EqualTo(0));
            Assert.That(result.Written, Is.EqualTo(0));
            Assert.That(result.Skipped, Is.EqualTo(0));
            Assert.That(sink.Lines, Is.Empty);
        });
    }

    [Test]
    public void Run_JsonImporter_ProcessesEntriesFromStream()
    {
        var sink = new RecordingSink();
        var json = string.Join(
            '\n',
            "{\"timestamp\":\"2026-11-22 10:00:00\",\"level\":\"INFO\",\"message\":\"Из JSON\"}",
            "{\"timestamp\":\"2026-11-22 10:00:01\",\"level\":\"ERROR\",\"message\":\"Ошибка JSON\"}");
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var processor = CreateProcessor(new JsonLogImporter(stream), LogLevel.Debug, sink);

        var result = processor.Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.Read, Is.EqualTo(2));
            Assert.That(result.Written, Is.EqualTo(2));
            Assert.That(sink.Lines, Has.Count.EqualTo(2));
        });
    }

    private static LogProcessor CreateProcessor(ILogImporter importer, LogLevel minLevel, params ILogSink[] sinks) =>
        new(importer, LogFilters.ByMinLevel(minLevel), new LogFormatter(), sinks);
}
