using Challenge.DataContracts;
using ConsoleApp.Solvers;

namespace ConsoleApp;

public class Solver
{
    /// <summary>
    /// Пытается решить задачу решателем из реестра. Возвращает false, если решателя для типа нет
    /// или вопрос не удалось решить: тогда ничего отправлять нельзя.
    /// </summary>
    public static bool TrySolve(TaskResponse taskResponse, out string answer)
    {
        answer = null;
        return taskResponse != null
               && SolverRegistry.Default.TrySolve(taskResponse.TypeId, taskResponse.Question, out answer);
    }
}
