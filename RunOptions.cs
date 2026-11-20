using System;
using System.Collections.Generic;
using System.Globalization;

namespace ConsoleApp.Infra;

public enum RunMode
{
    Solve,
    Pending,
    Help
}

/// <summary>
/// Параметры запуска: dotnet run --project ConsoleApp -- &lt;тип&gt; &lt;сколько&gt; [флаги]
/// </summary>
public sealed class RunOptions
{
    public const string Usage = @"Использование:
  dotnet run --project ConsoleApp -- <тип> [сколько]     взять и решить задачи (по умолчанию одну)
  dotnet run --project ConsoleApp -- --pending [тип]     показать взятые, но не сданные задачи

Флаги:
  --dry                 взять задачу и показать ответ, не отправляя его (таймер задачи все равно идет)
  --stop-on-fail        остановиться на первом неверном ответе (режим разбора)
  --max-fails <N>       остановиться после N неверных ответов подряд (по умолчанию 3, 0 — без ограничения)
  --collect             только взять задачи и записать вопросы в лог, ничего не отправляя
  --round <id>          раунд (по умолчанию текущий)
  --solve-timeout <с>   сколько секунд решатель может думать над задачей (по умолчанию 20)
  --log-dir <папка>     куда писать JSONL-логи (по умолчанию logs)
  --help                эта справка

Секрет команды: переменная окружения TEAM_SECRET или файл secrets.json {""teamSecret"": ""...""}.";

    public RunMode Mode { get; private set; } = RunMode.Solve;
    public string TypeId { get; private set; }
    public int Count { get; private set; } = 1;
    public bool Dry { get; private set; }
    public bool StopOnFail { get; private set; }
    public int MaxFails { get; private set; } = 3;
    public bool Collect { get; private set; }
    public string Round { get; private set; }
    public TimeSpan SolveTimeout { get; private set; } = TimeSpan.FromSeconds(20);
    public string LogDir { get; private set; } = "logs";

    public static RunOptions Parse(IReadOnlyList<string> args)
    {
        var options = new RunOptions();
        var positional = new List<string>();
        for (var i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--help" or "-h":
                    options.Mode = RunMode.Help;
                    return options;
                case "--dry":
                    options.Dry = true;
                    break;
                case "--stop-on-fail":
                    options.StopOnFail = true;
                    break;
                case "--collect":
                    options.Collect = true;
                    break;
                case "--pending":
                    options.Mode = RunMode.Pending;
                    break;
                case "--max-fails":
                    options.MaxFails = ParseInt(args, ref i, 0);
                    break;
                case "--solve-timeout":
                    options.SolveTimeout = TimeSpan.FromSeconds(ParseInt(args, ref i, 1));
                    break;
                case "--round":
                    options.Round = ParseValue(args, ref i);
                    break;
                case "--log-dir":
                    options.LogDir = ParseValue(args, ref i);
                    break;
                default:
                    if (args[i].StartsWith("--"))
                        throw new ArgumentException($"неизвестный флаг {args[i]}");
                    positional.Add(args[i]);
                    break;
            }
        }

        if (positional.Count > 0)
            options.TypeId = positional[0];
        if (positional.Count > 1)
        {
            if (!int.TryParse(positional[1], NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count < 1)
                throw new ArgumentException($"количество задач должно быть целым числом больше нуля, а не \"{positional[1]}\"");
            options.Count = count;
        }
        if (positional.Count > 2)
            throw new ArgumentException("лишние аргументы: " + string.Join(" ", positional.GetRange(2, positional.Count - 2)));

        if (options.Mode == RunMode.Solve && options.TypeId == null)
            throw new ArgumentException("не указан тип задач");
        return options;
    }

    private static string ParseValue(IReadOnlyList<string> args, ref int i)
    {
        if (i + 1 >= args.Count)
            throw new ArgumentException($"у флага {args[i]} нет значения");
        return args[++i];
    }

    private static int ParseInt(IReadOnlyList<string> args, ref int i, int min)
    {
        var name = args[i];
        var value = ParseValue(args, ref i);
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < min)
            throw new ArgumentException($"флаг {name} ждет целое число не меньше {min}, а не \"{value}\"");
        return number;
    }
}
