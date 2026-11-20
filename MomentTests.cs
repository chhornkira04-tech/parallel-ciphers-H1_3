using System;
using Challenge.DataContracts;
using ConsoleApp;
using NUnit.Framework;

namespace Tests;

public class MomentTests
{
    [TestCase("15:52:13 25.10.2026", "25 октября 15:52")]
    [TestCase("00:00:00 01.01.2026", "1 января 00:00")]
    [TestCase("23:59:59 31.12.2026", "31 декабря 23:59")]
    [TestCase("09:08:00 04.02.2026", "4 февраля 09:08")]
    [TestCase(" 15:52:13 25.10.2026 ", "25 октября 15:52")]
    public void Evaluate_FormatsUtcMoment(string input, string expected)
    {
        Assert.That(Moment.Evaluate(input), Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("15:52 25.10.2026")]
    [TestCase("25.10.2026 15:52:13")]
    [TestCase("15:52:13 32.10.2026")]
    [TestCase("15:52:13 25.13.2026")]
    [TestCase("not a date")]
    public void Evaluate_InvalidInput_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => Moment.Evaluate(input));
    }

    [Test]
    public void Evaluate_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Moment.Evaluate(null));
    }

    [Test]
    public void Solve_MomentTask_ReturnsFormattedMoment()
    {
        var task = new TaskResponse
        {
            TypeId = "moment",
            Question = "15:52:13 25.10.2026"
        };

        Assert.That(SolveTask(task), Is.EqualTo("25 октября 15:52"));
    }

    [Test]
    public void TrySolve_InvalidMoment_ReturnsFalse()
    {
        var task = new TaskResponse { TypeId = "moment", Question = "invalid" };

        Assert.That(Solver.TrySolve(task, out _), Is.False);
    }
    private static string SolveTask(TaskResponse task)
    {
        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        return answer;
    }
}
