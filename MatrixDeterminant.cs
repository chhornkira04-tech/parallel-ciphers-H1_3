using System;
using System.Linq;
using System.Numerics;

namespace ConsoleApp;

/// <summary>
/// Определитель целочисленной матрицы из текста вида "1 & 2 \\ 3 & 4":
/// строки разделены двойным обратным слэшем, столбцы амперсандом.
/// </summary>
public static class MatrixDeterminant
{
    public static BigInteger Evaluate(string matrixText)
    {
        return Determinant(Parse(matrixText));
    }

    public static BigInteger[][] Parse(string text)
    {
        var rows = text.Split(@"\\")
            .Select(row => row.Split('&')
                .Select(cell => BigInteger.Parse(cell.Trim()))
                .ToArray())
            .ToArray();

        if (rows.Any(row => row.Length != rows.Length))
            throw new FormatException($"матрица не квадратная: \"{text}\"");
        return rows;
    }

    // Алгоритм Барейса: все деления нацело, результат точный
    public static BigInteger Determinant(BigInteger[][] source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var n = source.Length;
        if (n == 0 || n > 128 || source.Any(row => row == null || row.Length != n))
            throw new ArgumentException("Ожидалась квадратная матрица размера 1..128", nameof(source));
        var a = source.Select(row => (BigInteger[])row.Clone()).ToArray();
        BigInteger sign = 1;
        BigInteger previousPivot = 1;

        for (var k = 0; k < n - 1; k++)
        {
            if (a[k][k].IsZero)
            {
                var swapWith = Array.FindIndex(a, k + 1, row => !row[k].IsZero);
                if (swapWith < 0) return BigInteger.Zero;
                (a[k], a[swapWith]) = (a[swapWith], a[k]);
                sign = -sign;
            }

            for (var i = k + 1; i < n; i++)
                for (var j = k + 1; j < n; j++)
                    a[i][j] = (a[i][j] * a[k][k] - a[i][k] * a[k][j]) / previousPivot;
            previousPivot = a[k][k];
        }

        return sign * a[n - 1][n - 1];
    }
}
