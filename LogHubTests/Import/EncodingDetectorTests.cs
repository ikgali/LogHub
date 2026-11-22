using System.Text;
using LogHub.Import;

namespace LogHubTests.Import;

public class EncodingDetectorTests
{
    static EncodingDetectorTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Test]
    public void Detect_Utf8Bom_ReturnsUtf8()
    {
        var bytes = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes("Привет, лог"))
            .ToArray();

        var encoding = EncodingDetector.Detect(bytes);

        Assert.That(encoding.CodePage, Is.EqualTo(Encoding.UTF8.CodePage));
    }

    [Test]
    public void Detect_Utf8WithoutBom_ReturnsUtf8()
    {
        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes("Строка в UTF-8 без BOM");

        var encoding = EncodingDetector.Detect(bytes);

        Assert.That(encoding.CodePage, Is.EqualTo(Encoding.UTF8.CodePage));
    }

    [Test]
    public void Detect_Windows1251Bytes_ReturnsWindows1251()
    {
        var bytes = Encoding.GetEncoding(1251).GetBytes("Ошибка записи в файл журнала");

        var encoding = EncodingDetector.Detect(bytes);

        Assert.That(encoding.CodePage, Is.EqualTo(1251));
    }

    [Test]
    public void Detect_Koi8rBytes_ReturnsKoi8r()
    {
        var bytes = Encoding.GetEncoding(20866).GetBytes("Проверка кодировки КОИ-8");

        var encoding = EncodingDetector.Detect(bytes);

        Assert.That(encoding.CodePage, Is.EqualTo(20866));
    }

    [Test]
    public void Detect_GarbageBytes_DoesNotThrow_AndReturnsUtf8Fallback()
    {
        byte[] bytes = [0x80, 0x81, 0x82, 0x83, 0x84];

        Encoding? encoding = null;
        Assert.DoesNotThrow(() => encoding = EncodingDetector.Detect(bytes));

        Assert.That(encoding!.CodePage, Is.EqualTo(Encoding.UTF8.CodePage));
    }
}
