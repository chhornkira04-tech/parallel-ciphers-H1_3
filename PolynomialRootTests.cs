using System.Globalization;
using ConsoleApp;
using Challenge.DataContracts;
using NUnit.Framework;

namespace Tests;

public class PolynomialRootTests
{
    // Корень подходит, если многочлен в нем обращается в 0 с точностью 0.001
    [TestCase("5.5*x + (-1.6)")]
    [TestCase("(-2)*x^2 + (-10.6)*x + 0.30000000000000004")]
    [TestCase("x^2 - 4")]
    [TestCase("x^2 - 2*x + 1")]
    [TestCase("x^3 - 6*x^2 + 11*x - 6")]
    [TestCase("x^3 + 1")]
    [TestCase("(x - 1)*(x - 2)*(x + 300)")]
    [TestCase("x^4 - 10*x^2 + 9")]
    [TestCase("2.5*x^5 + (-1)*x + 0.5")]
    [TestCase("x^2 + 0.0001")]
    [TestCase("1e-05*x + 1e-06")]
    [TestCase("0.0")]
    [TestCase("x")]
    public void Solve_ReturnsRoot(string polynomial)
    {
        var answer = PolynomialRoot.Solve(polynomial);

        Assert.That(answer, Is.Not.EqualTo(PolynomialRoot.NoRoots));
        var root = double.Parse(answer, CultureInfo.InvariantCulture);
        Assert.That(PolynomialRoot.Evaluate(PolynomialRoot.Parse(polynomial), root), Is.EqualTo(0).Within(0.001));
    }

    [TestCase("0.7999999999999999*x^2 + (-4.8)*x + 14.3")]
    [TestCase("4.699999999999999*x^2 + (-4.199999999999999)*x + 1.5")]
    [TestCase("x^2 + 1")]
    [TestCase("x^4 + x^2 + 1")]
    [TestCase("5")]
    [TestCase("(-3)*x^2 + (-1)")]
    public void Solve_NoRealRoots_ReturnsNoRoots(string polynomial)
    {
        Assert.That(PolynomialRoot.Solve(polynomial), Is.EqualTo("no roots"));
    }

    [Test]
    public void Solve_LinearEquation_IsAccurate()
    {
        Assert.That(PolynomialRoot.Solve("5.5*x + (-1.6)"), Is.EqualTo("0.2909090909"));
    }

    [Test]
    public void Solve_ThroughSolver_Works()
    {
        var task = new TaskResponse { TypeId = "polynomial-root", Question = "2*x + (-3)" };

        Assert.That(SolveTask(task), Is.EqualTo("1.5"));
    }

    [TestCase("x ^ -1")]
    [TestCase("x ^ 0.5")]
    [TestCase("x + ")]
    [TestCase("y + 1")]
    public void Solve_InvalidInput_Throws(string polynomial)
    {
        Assert.Throws<System.FormatException>(() => PolynomialRoot.Solve(polynomial));
    }
    private static string SolveTask(TaskResponse task)
    {
        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        return answer;
    }
}
