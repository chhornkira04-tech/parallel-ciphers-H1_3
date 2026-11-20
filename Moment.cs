using System;
using System.Globalization;

namespace ConsoleApp;

/// <summary>
/// Преобразует дату и время UTC из формата "HH:mm:ss dd.MM.yyyy"
/// в формат "d MMMM HH:mm" с русским названием месяца.
/// Секунды отбрасываются, часовой пояс не меняется.
/// </summary>
public static class Moment
{
    private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");
    private const string InputFormat = "HH:mm:ss dd.MM.yyyy";

    public static string Evaluate(string momentText)
    {
        if (momentText is null)
            throw new ArgumentNullException(nameof(momentText));

        if (!DateTime.TryParseExact(
                momentText.Trim(),
                InputFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var moment))
        {
            throw new FormatException($"неверный формат даты и времени: \"{momentText}\"");
        }

        return moment.ToString("d MMMM HH:mm", RussianCulture);
    }
}
