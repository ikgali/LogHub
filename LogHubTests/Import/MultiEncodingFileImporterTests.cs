using System.Text;
using LogHub.Import;

namespace LogHubTests.Import;

public class MultiEncodingFileImporterTests
{
    static MultiEncodingFileImporterTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Test]
    public void Import_Utf8Stream_ParsesCyrillicMessages()
    {
        const string text =
            "[2026-11-22 10:00:00] INFO Привет из UTF-8\n" +
            "[2026-11-22 10:00:01] WARN Вторая строка";
        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text);
        using var stream = new MemoryStream(bytes);
        var importer = new MultiEncodingFileImporter(stream);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries.Select(entry => entry.Message), Is.EqualTo(new[] { "Привет из UTF-8", "Вторая строка" }));
            Assert.That(importer.SkippedCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void Import_Windows1251Stream_PreservesCyrillicMessages()
    {
        const string text =
            "[2026-11-22 10:00:00] INFO Сообщение в Windows-1251\n" +
            "[2026-11-22 10:00:01] ERROR Ошибка кодировки";
        var bytes = Encoding.GetEncoding(1251).GetBytes(text);
        using var stream = new MemoryStream(bytes);
        var importer = new MultiEncodingFileImporter(stream);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries.Select(entry => entry.Message), Is.EqualTo(new[]
            {
                "Сообщение в Windows-1251", "Ошибка кодировки"
            }));
            Assert.That(importer.SkippedCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void Import_MessageWithReplacementCharacter_IsDroppedByValidationHook()
    {
        const string text =
            "[2026-11-22 10:00:00] INFO Хорошая строка\n" +
            "[2026-11-22 10:00:01] INFO Плохая \uFFFD строка";
        var bytes = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(text))
            .ToArray();
        using var stream = new MemoryStream(bytes);
        var importer = new MultiEncodingFileImporter(stream);

        var entries = importer.Import();

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0].Message, Is.EqualTo("Хорошая строка"));
            Assert.That(importer.SkippedCount, Is.EqualTo(1));
        });
    }
}
