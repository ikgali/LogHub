namespace LogHub.Reading;

public interface ILogReader
{
    IEnumerable<string> ReadLines();
}
