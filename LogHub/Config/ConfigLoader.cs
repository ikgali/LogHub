using LogHub.Parsing;

namespace LogHub.Config;

public static class ConfigLoader
{
    public static LogHubConfig Load(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                values[parts[0].Trim()] = parts[1].Trim();
            }
        }

        var input = values.GetValueOrDefault("input", "data/sample-utf8.log");
        var output = values.GetValueOrDefault("output", "data/out.log");
        var minLevel = LogLevelNames.Parse(values.GetValueOrDefault("minLevel", "INFO"));
        var sinks = values.GetValueOrDefault("sink", "console")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new LogHubConfig(input, minLevel, sinks, output);
    }
}
