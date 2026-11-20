using System;
using ConsoleApp;
using ConsoleApp.Solvers;
using Challenge.DataContracts;
using NUnit.Framework;

namespace Tests;

public class SteganographyTests
{
    [TestCase("20 36 44 92 30 47 28 38 82 113 114 45\nas fond of them as before she let harry watch television and gave him a bit of chocolate cake that tasted as though", "back through")]
    [TestCase("1 3 1\nabc", "aca")]
    [TestCase("1 2 3\r\na b\r\n", "a b")]
    [TestCase("I\na\nb\n", "ab")]
    [TestCase("85 76 41 87 69 106 8 108 51 71 93\nsauce and stacks of wizard crackers every few feet along the table these fantastic party favors were nothing like the", "anything so")]
    [TestCase("51 52 75 71 48 17 42 19 39 67 37 27\nsee sprouts they had a good time eating the every flavor beans harry got toast coconut baked bean strawberry curry", "floor hagrid")]
    [TestCase("70 27 86 11 30 13 51 110 61\nnoticed the night before and read hogwarts school of witchcraft and wizardry uniform first year students will require", "inferno a")]
    public void StageTwoFormatsAreSolvedThroughRegistry(string input, string expected)
    {
        Assert.That(SolverRegistry.Default.TrySolve("steganography", input, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("0\nabc")]
    [TestCase("4\nabc")]
    [TestCase("2147483648\nabc")]
    [TestCase("1 -1\nabc")]
    [TestCase("1 NaN\nabc")]
    [TestCase("1\nabc\ndef")]
    [TestCase("I\nabc\n\ndef")]
    [TestCase("IIII\nabcde")]
    [TestCase("I\n😀")]
    [TestCase("1\rabc")]
    [TestCase("1\n")]
    public void InvalidSteganographyHasNoAnswer(string input)
    {
        Assert.That(SolverRegistry.Default.TrySolve("steganography", input, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void SteganographyLengthIsBounded()
    {
        string input = "1\n" + new string('a', 65534);
        Assert.That(Steganography.Decode(input), Is.EqualTo("a"));
        Assert.Throws<NotSupportedException>(() => Steganography.Decode(input + "a"));
        Assert.That(SolverRegistry.Default.TrySolve("steganography", input + "a", out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    // Реальная задача с платформы
    private const string Sample =
        "XL\n" +
        "four hundred fifty points gryffindor common room he was going\n" +
        "parchment envelope addressed emerald green light harry had been trying\n" +
        "removed before he was going be able get past fluffy\n" +
        "ingredients collapsible brass scales examining precious little boats carried them\n" +
        "grumbled five minutes later they were all right said harry\n" +
        "dreadlocks was going be able get past fluffy he was\n" +
        "level hagrid said harry had been trying find how could\n" +
        "attacked before he was going be able get past fluffy\n" +
        "horns bulging lamp flickered darkness ahead he was going be\n" +
        "blowing an owl after all right said harry had been\n" +
        "echoes seemed be able get past fluffy he was going\n" +
        "mustache missing agrippa or not going be able get past\n";

    [Test]
    public void Decode_RealTask()
    {
        Assert.That(Steganography.Decode(Sample), Is.EqualTo("me all three"));
    }

    [TestCase("IV", 4)]
    [TestCase("IX", 9)]
    [TestCase("XIII", 13)]
    [TestCase("XXXVI", 36)]
    public void Decode_RomanNumeral(string roman, int position)
    {
        var line = new string('a', position - 1) + "z" + "bbb";

        Assert.That(Steganography.Decode($"{roman}\n{line}\n{line}\n"), Is.EqualTo("zz"));
    }

    [Test]
    public void Decode_ShortLine_Throws()
    {
        Assert.Throws<FormatException>(() => Steganography.Decode("XL\nabc\n"));
    }

    [Test]
    public void Decode_BadRoman_Throws()
    {
        Assert.Throws<FormatException>(() => Steganography.Decode("ZZ\nabc\n"));
    }

    [Test]
    public void TrySolve_Steganography()
    {
        var task = new TaskResponse { TypeId = "steganography", Question = Sample };

        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo("me all three"));
    }
}
