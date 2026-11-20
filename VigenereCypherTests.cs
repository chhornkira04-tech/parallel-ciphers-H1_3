using ConsoleApp;
using ConsoleApp.Solvers;
using NUnit.Framework;

namespace Tests;

public class VigenereCypherTests
{
    private static string Question(string key, string message) =>
        "#Vigenere's code=" + key + "#" + message + "#";

    [TestCase("kdguujlaumxs", "rhfw83wdt4xejrxtbqtn4lx58wn27pke541r3kktxx2m25 98", "he could say or think anything else the dormitory")]
    [TestCase("abc", "aceace", "abcabc")]
    [TestCase("a", "a z9'", "a z9'")]
    [TestCase(" ", "a", "b")]
    [TestCase("abc", "a", "a")]
    public void VigenereUsesThePlatformAlphabetAndRepeatsItsKey(string key, string encrypted, string expected)
    {
        Assert.That(SolverRegistry.Default.TrySolve("cypher", Question(key, encrypted), out var answer), Is.True);
        Assert.That(answer, Is.EqualTo(expected));
    }

    [Test]
    public void SingleLetterKeyIsEquivalentToCaesar()
    {
        const string message = "az09' abc";
        Assert.That(Cypher.Decode(Question("d", message)),
            Is.EqualTo(Cypher.Decode("#Caesar's code=3#" + message + "#")));
    }

    [TestCase("")]
    [TestCase("A")]
    [TestCase("abc😀")]
    [TestCase("abc\n")]
    [TestCase("abc#def")]
    public void InvalidKeysFailWithoutAnswer(string key)
    {
        Assert.That(SolverRegistry.Default.TrySolve("cypher", Question(key, "abc"), out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [TestCase("#Vigenere's code=abc##")]
    [TestCase("#Vigenere's code=abc#ABC#")]
    [TestCase("#Vigenere's code=abc#abc#trailing")]
    [TestCase("#Vigenere's code=abc#abc")]
    public void InvalidMessageStructureFailsWithoutAnswer(string input)
    {
        Assert.That(SolverRegistry.Default.TrySolve("cypher", input, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void KeyAndQuestionLengthsAreBounded()
    {
        Assert.That(Cypher.Decode(Question(new string('a', 4096), "a")), Is.EqualTo("a"));
        Assert.That(SolverRegistry.Default.TrySolve("cypher", Question(new string('a', 4097), "a"), out var answer), Is.False);
        Assert.That(answer, Is.Null);
        string prefix = "#Vigenere's code=a#";
        string largest = prefix + new string('b', 65536 - prefix.Length - 1) + "#";
        Assert.That(Cypher.Decode(largest).Length, Is.EqualTo(65536 - prefix.Length - 1));
        Assert.That(SolverRegistry.Default.TrySolve("cypher", largest + "#", out answer), Is.False);
        Assert.That(answer, Is.Null);
    }
}
