using ConsoleApp.Solvers;
using NUnit.Framework;

namespace Tests;

public class RomanListSteganographyTests
{
    [TestCase("XXXIX III XCVII I CV XXVI XXIV XIX XCIX XV XXXV IV LI LVIII CXI CXII XXXI XXIX\nbefore his aunt was back outside the door are you up yet she demanded nearly said harry well get a move on i want you", "of books thousands")]
    [TestCase("I III I\nabc", "aca")]
    [TestCase("I II III\r\na b\r\n", "a b")]
    [TestCase("I\na\nb\n", "ab")]
    [TestCase("  I  III  \nabc", "ac")]
    public void CanonicalRomanListsAreSolved(string question, string expected)
    {
        Assert.That(SolverRegistry.Default.TrySolve("steganography", question, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo(expected));
    }

    [TestCase("I IV\nabc")]
    [TestCase("I IIII\nabcde")]
    [TestCase("I IL\nabcde")]
    [TestCase("I 2\nabc")]
    [TestCase("1 II\nabc")]
    [TestCase("I -I\nabc")]
    [TestCase("I NaN\nabc")]
    [TestCase("I iii\nabc")]
    [TestCase("I II\nabc\ndef")]
    [TestCase("I II\n")]
    [TestCase("I II\n😀")]
    public void InvalidRomanListsHaveNoAnswer(string question)
    {
        Assert.That(SolverRegistry.Default.TrySolve("steganography", question, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void CanonicalRomanBoundaryIsSupported()
    {
        string text = new string('a', 3998) + "z";
        Assert.That(SolverRegistry.Default.TrySolve("steganography", "I MMMCMXCIX\n" + text, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo("az"));
        Assert.That(SolverRegistry.Default.TrySolve("steganography", "I MMMM\n" + text + "a", out answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void RomanListResourceBoundsAreEnforced()
    {
        // Overlong individual Roman tokens and the overall input are rejected before costly parsing.
        foreach (string input in new[] { "I " + new string('M', 1000) + "\na", new string('I', 65536) + " II\na" })
        {
            Assert.That(SolverRegistry.Default.TrySolve("steganography", input, out var answer), Is.False);
            Assert.That(answer, Is.Null);
        }
    }
}
