using System;
using System.IO;
using System.Linq;
using ConsoleApp;
using ConsoleApp.Solvers;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Tests;
public class OverflowSafetyTests
{
    // Direct implementation intentionally bypasses TaskSolver: the registry must still protect it.
    private sealed class FutureSolver : ITaskSolver
    {
        public string TypeId => "future";
        public bool TrySolve(string question, out string answer)
        {
            answer = "must not escape";
            var value = int.Parse(question);
            answer = (value + 1).ToString();
            return true;
        }
    }
    private sealed class InvalidAnswerSolver : ITaskSolver
    {
        public string TypeId => "invalid";
        public bool TrySolve(string question, out string answer) { answer = question; return true; }
    }
    [Test]
    public void FutureSolverOverflowIsRejected()
    {
        var registry = new SolverRegistry(new[] { new FutureSolver() });
        Assert.That(registry.TrySolve("future", int.MaxValue.ToString(), out var answer), Is.False);
        Assert.That(answer, Is.Null);
        Assert.That(registry.TrySolve("future", "1", out answer), Is.True);
        Assert.That(answer, Is.EqualTo("2"));
    }
    [TestCase("NaN")]
    [TestCase("Infinity")]
    [TestCase("-Infinity")]
    [TestCase("1e999")]
    [TestCase("")]
    public void InvalidAnswersAreRejected(string input)
    {
        var registry = new SolverRegistry(new[] { new InvalidAnswerSolver() });
        Assert.That(registry.TrySolve("invalid", input, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }
    [TestCase("2147483647+1")]
    [TestCase("50000*50000")]
    [TestCase("-2147483647-2")]
    [TestCase("(-2147483647-1)/-1")]
    [TestCase("-(-2147483647-1)")]
    [TestCase("(2147483647+1)-1")]
    [TestCase("1/0")]
    public void MathFailureDoesNotProduceAnswer(string input)
    {
        Assert.That(SolverRegistry.Default.TrySolve("math", input, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }
    [TestCase("1e999*x+1")]
    [TestCase("1e308*1e308*x")]
    [TestCase("x^129")]
    public void PolynomialFailureDoesNotProduceAnswer(string input) =>
        Assert.That(SolverRegistry.Default.TrySolve("polynomial-root", input, out _), Is.False);

    [TestCase("math")]
    [TestCase("polynomial-root")]
    public void ExcessiveNestingIsRejected(string type)
    {
        var input = new string('(', 200) + "1" + new string(')', 200);
        Assert.That(SolverRegistry.Default.TrySolve(type, input, out _), Is.False);
    }
    [Test]
    public void InvalidMatrixIsRejected() =>
        Assert.That(SolverRegistry.Default.TrySolve("determinant", "1 & 2", out _), Is.False);

    [Test]
    public void ProductionSourcesCannotDisableOverflowChecks()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "Directory.Build.props"))) root = root.Parent;
        Assert.That(root, Is.Not.Null, "Repository root not found");
        var violations = Directory.EnumerateFiles(root.FullName, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "obj" or "bin"))
            .Where(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot()
                .DescendantTokens().Any(token => token.RawKind == (int)SyntaxKind.UncheckedKeyword))
            .ToArray();
        Assert.That(violations, Is.Empty, "Explicit unchecked disables the common overflow protection");
    }
}
