using ConsoleApp;
using Challenge.DataContracts;
using NUnit.Framework;
namespace Tests;
public class SolverTests
{
    [Test]
    public void NullTaskIsRejected() => Assert.That(Solver.TrySolve(null, out _), Is.False);
    [Test]
    public void StarterReturns42()
    {
        Assert.That(Solver.TrySolve(new TaskResponse { TypeId = "starter", Question = "test" }, out var answer), Is.True);
        Assert.That(answer, Is.EqualTo("42"));
    }
}
