using ConsoleApp.Solvers;
using NUnit.Framework;

namespace Tests;

public class MultiplicatorCypherTests
{
    private const string Formula = " formula: (multiplicator * (charIndex + 1)) % (|ABC| + 1) - 1";
    private static string Question(string key, string message = "a z9'") =>
        "#prime multiplicator=" + key + Formula + "#" + message + "#";

    [TestCase("1", "a z9'")]
    [TestCase("2", "tsmr ")]
    [TestCase("38", " amcb")]
    [TestCase("2147483647", "d8z04")]
    public void MultiplicationSupportsInvertibleKeysWithoutIntermediateOverflow(string key, string expected)
    {
        // MaxValue * 38 overflows int; reduction before multiplication must preserve the answer.
        Assert.That(SolverRegistry.Default.TrySolve("cypher", Question(key), out var answer), Is.True);
        Assert.That(answer, Is.EqualTo(expected));
    }

    [Test]
    public void RealTaskIsDecoded()
    {
        string question = Question("1103", "k'e1k10ps1i01i9c1ioepc1ny9ep'yn1si9oe1ei1spoo1yi1cpzpzvpc");
        Assert.That(SolverRegistry.Default.TrySolve("cypher", question, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo("and a few of our older students would do well to remember"));
    }

    [TestCase("0")]
    [TestCase("3")]
    [TestCase("13")]
    [TestCase("39")]
    [TestCase("-1")]
    [TestCase("2147483648")]
    [TestCase("NaN")]
    [TestCase("")]
    public void InvalidOrNoninvertibleKeysHaveNoAnswer(string key)
    {
        Assert.That(SolverRegistry.Default.TrySolve("cypher", Question(key), out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void UnknownFormulaHasNoAnswer()
    {
        foreach (string question in new[]
        {
            Question("1103").Replace("charIndex + 1", "charIndex + 2"),
            Question("1103").Replace(Formula, " formula: arbitrary code"),
            Question(new string('9', 65000)),
            Question("1103", "😀"),
            Question("1103", "")
        })
        {
            Assert.That(SolverRegistry.Default.TrySolve("cypher", question, out var answer), Is.False);
            Assert.That(answer, Is.Null);
        }
    }
}
