using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoleApp;

/// <summary>
/// Ищет действительный корень многочлена, заданного текстом вида "0.8*x^2 + (-4.8)*x + 14.3".
/// Поддерживаются + - * ^ (целая неотрицательная степень), скобки, унарный минус и числа в экспоненциальной записи.
/// </summary>
public static class PolynomialRoot
{
    public const string NoRoots = "no roots";
    private const double Tolerance = 0.001;
    private const double LeadingZero = 1e-12;

    public static string Solve(string polynomialText)
    {
        var coefficients = Parse(polynomialText);
        var root = FindAnyRoot(coefficients);
        if (root.HasValue && Math.Abs(Evaluate(coefficients, root.Value)) > Tolerance)
            throw new ArithmeticException("Корень не прошел проверку невязки");
        return root.HasValue
            ? root.Value.ToString("0.##########", CultureInfo.InvariantCulture)
            : NoRoots;
    }

    // coefficients[i] — коэффициент при x^i
    public static double[] Parse(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        if (text.Length > 4096) throw new NotSupportedException("Слишком длинный многочлен");
        var parser = new Parser(text);
        var result = parser.ParseSum();
        parser.ExpectEnd();
        return Trim(result);
    }

    public static double Evaluate(double[] coefficients, double x)
    {
        double value = 0;
        for (var i = coefficients.Length - 1; i >= 0; i--)
            value = Finite(value * x + coefficients[i]);
        return value;
    }

    public static double? FindAnyRoot(double[] coefficients)
    {
        var roots = FindRoots(Trim(coefficients));
        return roots.Count > 0 ? roots[0] : null;
    }

    private static double[] Trim(double[] c)
    {
        if (c == null || c.Length == 0) throw new ArgumentException("Нет коэффициентов");
        if (c.Length > 129) throw new NotSupportedException("Степень многочлена больше 128");
        foreach (var coefficient in c) Finite(coefficient);
        var length = c.Length;
        while (length > 1 && Math.Abs(c[length - 1]) < LeadingZero) length--;
        return c.Take(length).ToArray();
    }

    // Все действительные корни по возрастанию. Корни производной делят ось на промежутки монотонности,
    // на каждом из которых многочлен имеет не более одного корня.
    private static List<double> FindRoots(double[] c)
    {
        var degree = c.Length - 1;
        if (degree == 0)
            return Math.Abs(c[0]) <= Tolerance / 2 ? new List<double> { 0 } : new List<double>();
        if (degree == 1)
            return new List<double> { Finite(-c[0] / c[1]) };

        var derivative = Trim(Enumerable.Range(1, degree).Select(i => Finite(c[i] * i)).ToArray());
        var critical = FindRoots(derivative);

        var bound = Finite(1 + Enumerable.Range(0, degree).Max(i => Math.Abs(c[i] / c[degree])));
        var points = new List<double> { -bound };
        points.AddRange(critical.Where(p => p > -bound && p < bound));
        points.Add(bound);

        var roots = new List<double>();
        // Касание оси в точке экстремума (кратный или почти кратный корень)
        foreach (var p in critical)
            if (Math.Abs(Evaluate(c, p)) < Tolerance / 2)
                roots.Add(p);

        for (var i = 0; i + 1 < points.Count; i++)
        {
            double lo = points[i], hi = points[i + 1];
            double fLo = Evaluate(c, lo), fHi = Evaluate(c, hi);
            if (fLo == 0 || fHi == 0 || Math.Sign(fLo) == Math.Sign(fHi)) continue;
            for (var iteration = 0; iteration < 200; iteration++)
            {
                var mid = lo / 2 + hi / 2;
                var fMid = Evaluate(c, mid);
                if (Math.Sign(fMid) == Math.Sign(fLo)) { lo = mid; fLo = fMid; }
                else hi = mid;
            }
            roots.Add(lo / 2 + hi / 2);
        }

        roots.Sort();
        return roots;
    }

    private static double Finite(double value) => double.IsFinite(value)
        ? value : throw new ArithmeticException("Неконечное значение при вычислении многочлена");

    private class Parser
    {
        private readonly string text;
        private int pos;

        public Parser(string text)
        {
            this.text = text;
        }

        public double[] ParseSum()
        {
            var value = ParseProduct();
            while (true)
            {
                if (TryRead('+')) value = Add(value, ParseProduct(), 1);
                else if (TryRead('-')) value = Add(value, ParseProduct(), -1);
                else return value;
            }
        }

        private double[] ParseProduct()
        {
            var value = ParsePower();
            while (TryRead('*')) value = Multiply(value, ParsePower());
            return value;
        }

        private double[] ParsePower()
        {
            var baseValue = ParseUnary();
            if (!TryRead('^')) return baseValue;

            var exponent = ParseUnary();
            if (exponent.Length != 1 || exponent[0] < 0 || exponent[0] != Math.Floor(exponent[0]) || exponent[0] > 1000)
                throw Error("степень должна быть целым неотрицательным числом");

            var result = new[] { 1.0 };
            for (var i = 0; i < (int)exponent[0]; i++) result = Multiply(result, baseValue);
            return result;
        }

        private int depth;
        private double[] ParseUnary()
        {
            if (++depth > 128) throw new NotSupportedException("Слишком глубокое выражение");
            try { return ParseUnaryCore(); }
            finally { depth--; }
        }

        private double[] ParseUnaryCore()
        {
            if (TryRead('-')) return Array.ConvertAll(ParseUnary(), v => -v);
            if (TryRead('+')) return ParseUnary();
            return ParseAtom();
        }

        private double[] ParseAtom()
        {
            if (TryRead('('))
            {
                var inner = ParseSum();
                if (!TryRead(')')) throw Error("ожидалась ')'");
                return inner;
            }
            if (TryRead('x') || TryRead('X')) return new[] { 0.0, 1.0 };
            return new[] { ParseNumber() };
        }

        private double ParseNumber()
        {
            SkipSpaces();
            var start = pos;
            while (pos < text.Length && (char.IsDigit(text[pos]) || text[pos] == '.')) pos++;
            if (pos < text.Length && (text[pos] == 'e' || text[pos] == 'E'))
            {
                var exponentStart = pos++;
                if (pos < text.Length && (text[pos] == '+' || text[pos] == '-')) pos++;
                var digitsStart = pos;
                while (pos < text.Length && char.IsDigit(text[pos])) pos++;
                if (digitsStart == pos) pos = exponentStart;
            }
            if (!double.TryParse(text.AsSpan(start, pos - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                throw Error("ожидалось число");
            return Finite(number);
        }

        private static double[] Add(double[] a, double[] b, int sign)
        {
            var result = new double[Math.Max(a.Length, b.Length)];
            for (var i = 0; i < a.Length; i++) result[i] = Finite(result[i] + a[i]);
            for (var i = 0; i < b.Length; i++) result[i] = Finite(result[i] + sign * b[i]);
            return result;
        }

        private static double[] Multiply(double[] a, double[] b)
        {
            if (a.Length + b.Length - 2 > 128)
                throw new NotSupportedException("Степень многочлена больше 128");
            var result = new double[a.Length + b.Length - 1];
            for (var i = 0; i < a.Length; i++)
                for (var j = 0; j < b.Length; j++)
                    result[i + j] = Finite(result[i + j] + Finite(a[i] * b[j]));
            return result;
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
