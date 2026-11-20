using System;
using ConsoleApp;
using ConsoleApp.Solvers;
using Challenge.DataContracts;
using NUnit.Framework;

namespace Tests;

public class CypherTests
{
    [TestCase("#Caesar's code=-20#'a9wrzwqvr9wsvr zwr5s4wr'64wczw9wr65uwr w94rzsvr' s9 wv#", "sure he'd read the name somewhere once term had started")]
    [TestCase("#Caesar's code=-2#sl c'p' jc9fc8q9rfc9qjwrfcpgl9efmqr9f'ppw9jmmicb9mtcp#", "unbearable he's the slytherin ghost harry looked over")]
    [TestCase("#Caesar's code=0#a z9'#", "a z9'")]
    [TestCase("#Caesar's code=2147483647#a z9'#", "rqeop")]
    [TestCase("#Caesar's code=-2147483648#a z9'#", "wvjtu")]
    public void CaesarIsSolvedThroughRegistry(string input, string expected)
    {
        Assert.That(SolverRegistry.Default.TrySolve("cypher", input, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("#reversed##")]
    [TestCase("#reversed#abc#ignored")]
    [TestCase("ignored#reversed#abc#")]
    [TestCase("#reversed#😀#")]
    [TestCase("#Caesar's code=2147483648#abc#")]
    [TestCase("#Caesar's code=-2147483649#abc#")]
    [TestCase("#Caesar's code=NaN#abc#")]
    [TestCase("#Caesar's code=#abc#")]
    public void InvalidCypherHasNoAnswer(string input)
    {
        Assert.That(SolverRegistry.Default.TrySolve("cypher", input, out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    [Test]
    public void CypherLengthIsBounded()
    {
        string input = "#reversed#" + new string('a', 65525) + "#";
        Assert.That(input.Length, Is.EqualTo(65536));
        Assert.That(Cypher.Decode(input).Length, Is.EqualTo(65525));
        Assert.Throws<NotSupportedException>(() => Cypher.Decode(input + "#"));
        Assert.That(SolverRegistry.Default.TrySolve("cypher", input + "#", out var answer), Is.False);
        Assert.That(answer, Is.Null);
    }

    // Реальные задачи с платформы
    [TestCase("#reversed#riap a ekil#", "like a pair")]
    [TestCase("#reversed#sa no yats ot#", "to stay on as")]
    [TestCase("#reversed#hey tseb s'ti llew#", "well it's best yeh")]
    [TestCase("#reversed#gnitarbelec erew ffupelffuh#", "hufflepuff were celebrating")]
    [TestCase("#reversed#noitcelfer a tsuj saw#", "was just a reflection")]
    public void Decode_Reversed(string question, string expected)
    {
        Assert.That(Cypher.Decode(question), Is.EqualTo(expected));
    }

    [Test]
    public void Decode_UnknownCypher_Throws()
    {
        Assert.Throws<NotSupportedException>(() => Cypher.Decode("#caesar#abc#"));
    }

    [Test]
    public void Decode_NoCypherDescription_Throws()
    {
        Assert.Throws<FormatException>(() => Cypher.Decode("abc"));
    }

    [Test]
    public void TrySolve_UnknownCypher_ReturnsFalse()
    {
        var task = new TaskResponse { TypeId = "cypher", Question = "#caesar#abc#" };

        Assert.That(Solver.TrySolve(task, out _), Is.False);
    }

    [Test]
    public void TrySolve_Reversed_ReturnsDecodedText()
    {
        var task = new TaskResponse { TypeId = "cypher", Question = "#reversed#riap a ekil#" };

        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo("like a pair"));
    }

    [Test]
    public void TrySolve_StarterTask_Returns42()
    {
        var task = new TaskResponse { TypeId = "starter", Question = "Главный вопрос жизни, вселенной и всего такого." };

        Assert.That(Solver.TrySolve(task, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo("42"));
    }
}
