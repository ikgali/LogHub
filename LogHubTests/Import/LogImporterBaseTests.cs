using System.Reflection;
using LogHub.Import;
using LogHub.Parsing;

namespace LogHubTests.Import;

public class LogImporterBaseTests
{
    private const string InfoLine = "[2026-11-22 10:00:00] INFO Первая строка";
    private const string WarnLine = "[2026-11-22 10:00:01] WARN Вторая строка";
    private const string ErrorLine = "[2026-11-22 10:00:02] ERROR Третья строка";

    [Test]
    public void Import_CallsStepsInTemplateOrder()
    {
        var importer = new RecordingImporter([InfoLine, WarnLine]);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(importer.Calls, Is.EqualTo(new[]
            {
                "Open", "ReadRaw", "TryParse", "Validate", "TryParse", "Validate", "Close", "Release"
            }));
        });
    }

    [Test]
    public void Import_IsNotVirtual()
    {
        var method = typeof(LogImporterBase).GetMethod(
            nameof(LogImporterBase.Import),
            BindingFlags.Public | BindingFlags.Instance);

        Assert.That(method, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(method!.IsVirtual, Is.False);
            Assert.That(method.IsFinal, Is.False);
        });
    }

    [Test]
    public void Import_DefaultValidationHook_KeepsAllParsedEntries()
    {
        var importer = new InMemoryLogImporter(InfoLine, WarnLine, ErrorLine);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(3));
            Assert.That(importer.SkippedCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void Import_OverriddenValidationHook_DropsRejectedEntries()
    {
        var importer = new RecordingImporter(
            [InfoLine, ErrorLine],
            entry => entry.Level != LogLevel.Error);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0].Level, Is.EqualTo(LogLevel.Info));
            Assert.That(importer.SkippedCount, Is.EqualTo(1));
        });
    }

    private sealed class RecordingImporter : LogImporterBase
    {
        private readonly IReadOnlyList<string> _lines;
        private readonly Func<LogEntry, bool>? _validate;
        private readonly LogLineParser _parser = new();

        public RecordingImporter(IReadOnlyList<string> lines, Func<LogEntry, bool>? validate = null)
        {
            _lines = lines;
            _validate = validate;
        }

        public List<string> Calls { get; } = [];

        protected override void Open() => Calls.Add("Open");

        protected override IEnumerable<string> ReadRaw()
        {
            Calls.Add("ReadRaw");
            return _lines;
        }

        protected override bool TryParse(string raw, out LogEntry entry)
        {
            Calls.Add("TryParse");
            return _parser.TryParse(raw, out entry);
        }

        protected override bool Validate(LogEntry entry)
        {
            Calls.Add("Validate");
            return _validate?.Invoke(entry) ?? true;
        }

        protected override void Close() => Calls.Add("Close");

        protected override void Release() => Calls.Add("Release");
    }
}
