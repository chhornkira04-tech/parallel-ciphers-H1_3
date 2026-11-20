using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp.Infra;

public sealed class RunLogEntry
{
    public DateTime Time { get; set; } = DateTime.UtcNow;

    /// <summary>taken, collected, dry, result, unsolved, error</summary>
    public string Event { get; set; }

    public string TaskId { get; set; }
    public string Type { get; set; }
    public string Round { get; set; }
    public string Question { get; set; }
    public string Hint { get; set; }
    public string Answer { get; set; }
    public string Status { get; set; }
    public int? Points { get; set; }
    public long? SolveMs { get; set; }
    public string Message { get; set; }
}

public interface IRunLog
{
    void Write(RunLogEntry entry);
}

public sealed class NullRunLog : IRunLog
{
    public void Write(RunLogEntry entry)
    {
    }
}

/// <summary>
/// Пишет по одной JSON-строке на событие в файл runs-ГГГГММДД.jsonl.
/// Вопросы сохраняются у всех задач, в том числе у неверных: из них потом делаются тесты.
/// </summary>
public sealed class JsonlRunLog : IRunLog
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string dir;
    private readonly object sync = new();

    public JsonlRunLog(string dir)
    {
        this.dir = dir;
    }

    public void Write(RunLogEntry entry)
    {
        var line = JsonSerializer.Serialize(entry, Options);
        lock (sync)
        {
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, $"runs-{entry.Time:yyyyMMdd}.jsonl"), line + Environment.NewLine);
        }
    }
}
