using System;

namespace ConsoleApp;

/// <summary>
/// Вычисляет целочисленные выражения с +, -, *, /, %, скобками и унарным минусом.
/// Арифметика int как в C#: деление усекает к нулю, остаток берет знак делимого.
/// </summary>
public static class MathExpression
{
    public static int Evaluate(string expression)
    {
        var parser = new Parser(expression);
        var result = parser.ParseExpression();
        parser.ExpectEnd();
        return result;
    }

    private class Parser
    {
        private readonly string text;
        private int pos;

        public Parser(string text)
        {
            this.text = text;
        }

        public int ParseExpression()
        {
            var value = ParseTerm();
            while (true)
            {
                if (TryRead('+')) value = checked(value + ParseTerm());
                else if (TryRead('-')) value = checked(value - ParseTerm());
                else return value;
            }
        }

        private int ParseTerm()
        {
            var value = ParseFactor();
            while (true)
            {
                if (TryRead('*')) value = checked(value * ParseFactor());
                else if (TryRead('/')) value = Divide(value, ParseFactor());
                else if (TryRead('%')) value = Remainder(value, ParseFactor());
                else return value;
            }
        }

        private int depth;
        private int ParseFactor()
        {
            if (++depth > 128) throw new NotSupportedException("Слишком глубокое выражение");
            try { return ParseFactorCore(); }
            finally { depth--; }
        }

        private int ParseFactorCore()
        {
            if (TryRead('-')) return checked(-ParseFactor());
            if (TryRead('+')) return ParseFactor();
            if (TryRead('('))
            {
                var inner = ParseExpression();
                if (!TryRead(')')) throw Error("ожидалась ')'");
                return inner;
            }
            return ParseNumber();
        }

        private int ParseNumber()
        {
            SkipSpaces();
            var start = pos;
            while (pos < text.Length && char.IsDigit(text[pos])) pos++;
            if (start == pos) throw Error("ожидалось число");
            return int.Parse(text.AsSpan(start, pos - start));
        }

        // Деление должно обнаруживать переполнение int.MinValue / -1.
        private static int Divide(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            return b == -1 ? checked(-a) : a / b;
        }

        private static int Remainder(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            return b == -1 ? 0 : a % b;
        }

        public void ExpectEnd()
        {
            SkipSpaces();
            if (pos != text.Length) throw Error("лишние символы");
        }

        private bool TryRead(char c)
        {
            SkipSpaces();
            if (pos < text.Length && text[pos] == c)
            {
                pos++;
                return true;
            }
            return false;
        }

        private void SkipSpaces()
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++;
        }

        private FormatException Error(string message) =>
            new FormatException($"{message} (позиция {pos}) в выражении \"{text}\"");
    }
}
