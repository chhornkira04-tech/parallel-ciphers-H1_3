using System;
using ConsoleApp;
using Challenge.DataContracts;
using NUnit.Framework;

namespace Tests;

public class MathExpressionTests
{
    [TestCase("6+4*1+5-6", 9)]
    [TestCase("11+11", 22)]
    [TestCase("0-1*5+9", 4)]
    [TestCase("7", 7)]
    [TestCase("2+3*4", 14)]
    [TestCase("(2+3)*4", 20)]
    [TestCase("-5+2", -3)]
    [TestCase("2*-3", -6)]
    [TestCase("10-2-3", 5)]
    [TestCase("100/10/5", 2)]
    [TestCase(" 1 + 2 ", 3)]
    // Деление и остаток как в C#: усечение к нулю, остаток со знаком делимого
    [TestCase("7/2", 3)]
    [TestCase("-7/2", -3)]
    [TestCase("7%3", 1)]
    [TestCase("-7%3", -1)]
    [TestCase("7%-3", 1)]
    [TestCase("2+7%4*3", 11)]
    public void Evaluate_FollowsCSharpRules(string expression, int expected)
    {
        Assert.That(MathExpression.Evaluate(expression), Is.EqualTo(expected));
    }

    [TestCase("1/0")]
    [TestCase("1%0")]
    public void Evaluate_DivisionByZero_Throws(string expression)
    {
        Assert.Throws<DivideByZeroException>(() => MathExpression.Evaluate(expression));
    }

    [TestCase("")]
    [TestCase("1+")]
    [TestCase("(1+2")]
    [TestCase("1 2")]
    [TestCase("abc")]
    public void Evaluate_InvalidExpression_Throws(string expression)
    {
        Assert.Throws<FormatException>(() => MathExpression.Evaluate(expression));
    }

    [Test]
    public void Solve_MathTask_ReturnsValueOfExpression()
    {
        var task = new TaskResponse { TypeId = "math", Question = "6+4*1+5-6" };

        Assert.That(SolveTask(task), Is.EqualTo("9"));
    }

    [Test]
    public void TrySolve_UnknownType_ReturnsFalse()
    {
        var task = new TaskResponse { TypeId = "cypher", Question = "abc" };

        Assert.That(Solver.TrySolve(task, out _), Is.False);
    }

    [Test]
    public void TrySolve_UnparsableMathQuestion_ReturnsFalse()
    {
        var task = new TaskResponse { TypeId = "math", Question = "2 ^ 3" };

        Assert.That(Solver.TrySolve(task, out _), Is.False);
    }
    private static string SolveTask(TaskResponse task)
    {
        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        return answer;
    }
}
