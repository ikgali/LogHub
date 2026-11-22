using System.Text;

namespace LogHub.Import;

public static class EncodingDetector
{
    private static readonly char[] RussianLetters = ['а', 'е', 'ё', 'и', 'о', 'р', 'с', 'т', 'у'];
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static EncodingDetector()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding Detect(ReadOnlySpan<byte> bytes)
    {
        var bomEncoding = DetectByBom(bytes);
        if (bomEncoding is not null)
        {
            return bomEncoding;
        }

        if (IsValidUtf8(bytes))
        {
            return Encoding.UTF8;
        }

        var windows1251 = Encoding.GetEncoding(1251);
        var koi8r = Encoding.GetEncoding(20866);

        var windows1251Score = CountRussianLetters(windows1251.GetString(bytes));
        var koi8rScore = CountRussianLetters(koi8r.GetString(bytes));

        if (windows1251Score == 0 && koi8rScore == 0)
        {
            return Encoding.UTF8;
        }

        return windows1251Score >= koi8rScore ? windows1251 : koi8r;
    }

    private static Encoding? DetectByBom(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
        {
            return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
        }

        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
        {
            return new UTF32Encoding(bigEndian: false, byteOrderMark: true);
        }

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return Encoding.Unicode;
        }

        return null;
    }

    private static bool IsValidUtf8(ReadOnlySpan<byte> bytes)
    {
        try
        {
            _ = StrictUtf8.GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static int CountRussianLetters(string text)
    {
        var count = 0;
        foreach (var character in text)
        {
            if (Array.IndexOf(RussianLetters, char.ToLowerInvariant(character)) >= 0)
            {
                count++;
            }
        }

        return count;
    }
}
