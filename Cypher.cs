using System;
using System.Globalization;
using System.Linq;

namespace ConsoleApp;

/// <summary>Расшифровывает известные форматы над фиксированным алфавитом платформы.</summary>
public static class Cypher
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789' ";

    public static string Decode(string question)
    {
        ArgumentNullException.ThrowIfNull(question);
        if (question.Length > 65536) throw new NotSupportedException("Слишком длинное сообщение");
        var parts = question.Split('#');
        if (parts.Length != 4 || parts[0] != "" || parts[3] != "" || parts[2].Length == 0)
            throw new FormatException("Ожидалось #шифр#сообщение#");
        var name = parts[1].Trim();
        var message = parts[2];
        if (message.Any(c => Alphabet.IndexOf(c) < 0))
            throw new FormatException("Символ вне алфавита платформы");
        const string vigenerePrefix = "Vigenere's code=";
        if (parts[1].StartsWith(vigenerePrefix, StringComparison.Ordinal))
            return DecodeVigenere(message, parts[1][vigenerePrefix.Length..]);
        if (name == "reversed") return new string(message.Reverse().ToArray());
        const string multiplicatorPrefix = "prime multiplicator=";
        const string formula = " formula: (multiplicator * (charIndex + 1)) % (|ABC| + 1) - 1";
        if (name.StartsWith(multiplicatorPrefix, StringComparison.Ordinal))
        {
            if (!name.EndsWith(formula, StringComparison.Ordinal) ||
                name.Length <= multiplicatorPrefix.Length + formula.Length ||
                !int.TryParse(name.AsSpan(multiplicatorPrefix.Length,
                        name.Length - multiplicatorPrefix.Length - formula.Length),
                    NumberStyles.None, CultureInfo.InvariantCulture, out int multiplicator) || multiplicator < 1)
                throw new FormatException("Неизвестная формула или некорректный множитель");
            return DecodeMultiplication(message, multiplicator);
        }
        const string prefix = "Caesar's code=";
        if (!name.StartsWith(prefix, StringComparison.Ordinal))
            throw new NotSupportedException("Неизвестный шифр");
        if (!int.TryParse(name.AsSpan(prefix.Length), NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var key))
            throw new FormatException("Некорректный сдвиг");
        // Сначала уменьшаем ключ: даже int.MinValue не требует опасного унарного минуса.
        int shift = key % Alphabet.Length;
        var result = new char[message.Length];
        for (int i = 0; i < message.Length; i++)
        {
            int index = (Alphabet.IndexOf(message[i]) - shift + Alphabet.Length) % Alphabet.Length;
            result[i] = Alphabet[index];
        }
        return new string(result);
    }

    private static string DecodeVigenere(string message, string key)
    {
        // Bounded key schedule; keys longer than a message remain valid.
        if (key.Length is < 1 or > 4096 || key.Any(c => Alphabet.IndexOf(c) < 0))
            throw new FormatException("Недопустимый ключ Виженера");
        var result = new char[message.Length];
        for (int i = 0; i < message.Length; i++)
        {
            int shift = Alphabet.IndexOf(key[i % key.Length]);
            int encryptedIndex = Alphabet.IndexOf(message[i]);
            int originalIndex = (encryptedIndex - shift + Alphabet.Length) % Alphabet.Length;
            if ((originalIndex + shift) % Alphabet.Length != encryptedIndex)
                throw new ArithmeticException("Не удалось проверить расшифровку Виженера");
            result[i] = Alphabet[originalIndex];
        }
        return new string(result);
    }

    private static string DecodeMultiplication(string message, int multiplicator)
    {
        int modulus = Alphabet.Length + 1;
        // Остаток берём до умножения: int.MaxValue * (charIndex + 1) мог бы переполниться.
        int reduced = multiplicator % modulus;
        int inverse = 0;
        for (int candidate = 1; candidate < modulus; candidate++)
            if (reduced * candidate % modulus == 1) { inverse = candidate; break; }
        if (inverse == 0) throw new NotSupportedException("Множитель не имеет обратного по модулю алфавита");
        var result = new char[message.Length];
        for (int i = 0; i < message.Length; i++)
        {
            int encryptedIndex = Alphabet.IndexOf(message[i]);
            int originalIndex = inverse * (encryptedIndex + 1) % modulus - 1;
            if (originalIndex < 0 || originalIndex >= Alphabet.Length ||
                reduced * (originalIndex + 1) % modulus - 1 != encryptedIndex)
                throw new ArithmeticException("Не удалось проверить расшифровку");
            result[i] = Alphabet[originalIndex];
        }
        return new string(result);
    }
}
