using System;
using ConsoleApp;
using Challenge.DataContracts;
using NUnit.Framework;

namespace Tests;

public class MatrixDeterminantTests
{
    // Первый случай — реальная задача с платформы
    [TestCase(@"-9 & 1 & -9 \\ 0 & -8 & -1 \\ -1 & -1 & -10", "-638")]
    [TestCase("5", "5")]
    [TestCase(@"1 & 2 \\ 3 & 4", "-2")]
    [TestCase(@"1 & 0 \\ 0 & 1", "1")]
    [TestCase(@"0 & 1 \\ 1 & 0", "-1")]
    [TestCase(@"1 & 2 \\ 2 & 4", "0")]
    [TestCase(@"0 & 2 & 1 \\ 3 & 0 & 4 \\ 5 & 6 & 0", "58")]
    [TestCase(@"1 & 2 & 3 \\ 4 & 5 & 6 \\ 7 & 8 & 9", "0")]
    [TestCase(@"2 & 0 & 0 & 0 \\ 0 & 3 & 0 & 0 \\ 0 & 0 & 4 & 0 \\ 0 & 0 & 0 & 5", "120")]
    [TestCase(@"0 & 0 & 1 & 0 \\ 0 & 0 & 0 & 1 \\ 1 & 0 & 0 & 0 \\ 0 & 1 & 0 & 0", "1")]
    [TestCase(@"1000000000 & 1 \\ 1 & 1000000000", "999999999999999999")]
    public void Evaluate_ReturnsExactDeterminant(string matrix, string expected)
    {
        Assert.That(MatrixDeterminant.Evaluate(matrix).ToString(), Is.EqualTo(expected));
    }

    [TestCase(@"1 & 2 \\ 3")]
    [TestCase(@"1 & 2 & 3 \\ 4 & 5 & 6")]
    [TestCase(@"a & b \\ c & d")]
    public void Evaluate_InvalidMatrix_Throws(string matrix)
    {
        Assert.Throws<FormatException>(() => MatrixDeterminant.Evaluate(matrix));
    }

    [Test]
    public void Solve_DeterminantTask_ReturnsDeterminant()
    {
        var task = new TaskResponse { TypeId = "determinant", Question = @"-9 & 1 & -9 \\ 0 & -8 & -1 \\ -1 & -1 & -10" };

        Assert.That(SolveTask(task), Is.EqualTo("-638"));
    }
    private static string SolveTask(TaskResponse task)
    {
        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        return answer;
    }
}
