using ConsoleApp.Solvers;
using NUnit.Framework;

namespace Tests;

public class ElementSteganographyTests
{
    [Test]
    public void RealElementIndexesAreDecoded()
    {
        const string question = "[At] [P] [Md] [Br] [In] [Zr] [Gd] [O] [Pu] [Ce] [Mo]\n" +
            "how said hermione nervously i think said ron were going to have to be chessmen he walked up to a black knight and";
        AssertAnswer(question, "locked door");
    }

    [TestCase("H", 1)]
    [TestCase("He", 2)]
    [TestCase("O", 8)]
    [TestCase("At", 85)]
    [TestCase("Md", 101)]
    [TestCase("Lr", 103)]
    [TestCase("Nh", 113)]
    [TestCase("Og", 118)]
    public void AtomicNumbersUseOneBasedPositions(string element, int number)
    {
        var text = new string('a', number - 1) + "z";
        AssertAnswer("[" + element + "]\n" + text, "z");
    }

    [Test]
    public void SingleElementUsesOneTextLineAndListsPreserveSpaces()
    {
        AssertAnswer("[H] [Li] [He]\na b", "ab");
        AssertAnswer("[H] [He] [Li]\r\na b\r\n", "a b");
    }

    [TestCase("[Og]\nshort")]
    [TestCase("[h]\na")]
    [TestCase("[OG]\na")]
    [TestCase("[Uuo]\na")]
    [TestCase("[Xx]\na")]
    [TestCase("[0]\na")]
    [TestCase("[H\na")]
    [TestCase("H]\na")]
    [TestCase("[]\na")]
    [TestCase("[[H]]\na")]
    [TestCase("[H][He]\nabc")]
    [TestCase("[H] II\nabc")]
    [TestCase("[H] 2\nabc")]
    [TestCase("I [He]\nabc")]
    [TestCase("[H]\nabc\ndef")]
    [TestCase("[H]\n")]
    [TestCase("[H]\n😀")]
    public void InvalidElementPositionsFailWithoutAnswer(string question)
    {
        Assert.That(SolverRegistry.Default.TrySolve("steganography", question, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void ExcessiveElementInputFailsWithoutAnswer()
    {
        string question = "[H] " + new string('x', 65536) + "\na";
        Assert.That(SolverRegistry.Default.TrySolve("steganography", question, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    private static void AssertAnswer(string question, string expected)
    {
        Assert.That(SolverRegistry.Default.TrySolve("steganography", question, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo(expected));
    }
}
