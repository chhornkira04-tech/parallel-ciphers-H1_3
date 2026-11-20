using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp;

/// <summary>Извлекает символы по римской позиции в строках либо списку позиций в тексте.</summary>
public static class Steganography
{
    // Atomic-number order verified against the IUPAC periodic table:
    // https://iupac.org/wp-content/uploads/2018/12/IUPAC_Periodic_Table_A3-01Dec18.pdf
    private static readonly string[] Elements = (
        "H He Li Be B C N O F Ne Na Mg Al Si P S Cl Ar K Ca " +
        "Sc Ti V Cr Mn Fe Co Ni Cu Zn Ga Ge As Se Br Kr Rb Sr Y Zr " +
        "Nb Mo Tc Ru Rh Pd Ag Cd In Sn Sb Te I Xe Cs Ba La Ce Pr Nd " +
        "Pm Sm Eu Gd Tb Dy Ho Er Tm Yb Lu Hf Ta W Re Os Ir Pt Au Hg " +
        "Tl Pb Bi Po At Rn Fr Ra Ac Th Pa U Np Pu Am Cm Bk Cf Es Fm " +
        "Md No Lr Rf Db Sg Bh Hs Mt Ds Rg Cn Nh Fl Mc Lv Ts Og").Split(' ');

    public static string Decode(string question)
    {
        ArgumentNullException.ThrowIfNull(question);
        if (question.Length > 65536) throw new NotSupportedException("Слишком длинный текст");
        var normalized = question.Replace("\r\n", "\n");
        if (normalized.Contains('\r')) throw new FormatException("Некорректный перевод строки");
        // Платформа завершает некоторые тексты переводом строки.
        var lines = normalized.TrimEnd('\n').Split('\n');
        if (lines.Length < 2 || lines[0].Length == 0)
            throw new FormatException("Нет позиции или текста");
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Length == 0) throw new FormatException("Пустая строка текста");
            foreach (char c in lines[i])
                if (!(c is >= 'a' and <= 'z' or >= '0' and <= '9' or '\'' or ' '))
                    throw new FormatException("Символ вне алфавита платформы");
        }
        var result = new StringBuilder();
        string header = lines[0].Trim();
        var positions = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool decimalPositions = header.Length > 0 && header[0] is >= '0' and <= '9';
        bool elementPositions = header.StartsWith("[", StringComparison.Ordinal);
        if (decimalPositions || elementPositions || positions.Length > 1)
        {
            if (lines.Length != 2) throw new FormatException("Для списка позиций нужна одна строка текста");
            foreach (var token in positions)
            {
                int position;
                if (elementPositions) position = FromElement(token);
                else if (decimalPositions)
                {
                    if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out position))
                        throw new FormatException("Некорректная десятичная позиция");
                }
                else position = FromRoman(token);
                if (position < 1 || position > lines[1].Length)
                    throw new FormatException("Недопустимая позиция");
                result.Append(lines[1][position - 1]);
            }
        }
        else
        {
            int position = FromRoman(header);
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length < position) throw new FormatException("Строка короче позиции");
                result.Append(lines[i][position - 1]);
            }
        }
        return result.ToString().Trim();
    }

    private static int FromElement(string token)
    {
        if (token.Length is < 3 or > 4 || token[0] != '[' || token[^1] != ']')
            throw new FormatException("Ожидался символ химического элемента в скобках");
        var symbol = token[1..^1];
        // At most 118 exact, case-sensitive comparisons per position; no permissive
        // parsing of obsolete symbols, atomic masses or arbitrary bracket expressions.
        int index = Array.IndexOf(Elements, symbol);
        if (index < 0) throw new FormatException("Неизвестный химический элемент");
        return index + 1;
    }

    private static int FromRoman(string roman)
    {
        // Канонические римские числа 1..3999; заголовок ограничен до разбора.
        if (roman.Length is < 1 or > 15 || !Regex.IsMatch(roman,
                @"\AM{0,3}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})\z",
                RegexOptions.CultureInvariant | RegexOptions.NonBacktracking))
            throw new FormatException("Некорректное римское число");
        var values = new Dictionary<char, int>
        { ['I'] = 1, ['V'] = 5, ['X'] = 10, ['L'] = 50, ['C'] = 100, ['D'] = 500, ['M'] = 1000 };
        int total = 0;
        for (int i = 0; i < roman.Length; i++)
        {
            int value = values[roman[i]];
            int next = i + 1 < roman.Length ? values[roman[i + 1]] : 0;
            total += value < next ? -value : value;
        }
        return total;
    }
}
