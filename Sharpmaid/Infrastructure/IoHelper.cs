namespace Sharpmaid.Infrastructure;

public static class IoHelper
{
    public static string ReadFile(string path)
    {
        using var streamReader = new StreamReader(path);

        return streamReader.ReadToEnd();
    }

    public static void CreateFile(string path, List<string> lines)
    {
        using var streamWriter = new StreamWriter(path);
        foreach (var line in lines)
        {
            streamWriter.WriteLine(line);
        }
    }
}
