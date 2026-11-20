using System;
using System.IO;
using System.Text.Json;

namespace ConsoleApp.Infra;

/// <summary>
/// Секрет команды берется из переменной окружения TEAM_SECRET или из файла secrets.json
/// вида {"teamSecret": "..."}. Файл лежит в .gitignore и не должен попадать в коммиты.
/// </summary>
public static class SecretProvider
{
    public const string EnvName = "TEAM_SECRET";
    public const string FileName = "secrets.json";

    public static string Get(Func<string, string> getEnv = null, string[] searchDirs = null)
    {
        getEnv ??= Environment.GetEnvironmentVariable;
        var fromEnv = getEnv(EnvName)?.Trim();
        if (!string.IsNullOrEmpty(fromEnv))
            return fromEnv;

        searchDirs ??= new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var start in searchDirs)
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                var path = Path.Combine(dir.FullName, FileName);
                if (!File.Exists(path))
                    continue;
                var secret = ReadFile(path);
                if (!string.IsNullOrEmpty(secret))
                    return secret;
            }
        }
        return null;
    }

    private static string ReadFile(string path)
    {
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            return json.RootElement.TryGetProperty("teamSecret", out var value) ? value.GetString()?.Trim() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
