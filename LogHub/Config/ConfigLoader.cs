namespace LogHub.Config;

public static class ConfigLoader
{
    public static void LoadInto(string path, IConfigService target)
    {
        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                target.SetValue(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}
