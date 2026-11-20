using System.Collections.Generic;
using System.Threading.Tasks;
using Challenge;
using Challenge.DataContracts;
using TaskStatus = Challenge.DataContracts.TaskStatus;

namespace ConsoleApp.Infra;

/// <summary>
/// Нужные прогону вызовы API. Через интерфейс прогон проверяется тестами без сети.
/// </summary>
public interface ITaskApi
{
    Task<ChallengeResponse> GetChallengeAsync(string challengeId);
    Task<TaskResponse> AskNewTaskAsync(string round, string taskType);
    Task<TaskResponse> CheckTaskAnswerAsync(string taskId, string answer);
    Task<TaskResponse> GetTaskAsync(string taskId);
    Task<List<TaskResponse>> GetPendingAsync(string round, string taskType);
}

public sealed class ChallengeApi : ITaskApi
{
    private readonly ChallengeClient client;

    public ChallengeApi(ChallengeClient client)
    {
        this.client = client;
    }

    public Task<ChallengeResponse> GetChallengeAsync(string challengeId) => client.GetChallengeAsync(challengeId);

    public Task<TaskResponse> AskNewTaskAsync(string round, string taskType) => client.AskNewTaskAsync(round, taskType);

    public Task<TaskResponse> CheckTaskAnswerAsync(string taskId, string answer) => client.CheckTaskAnswerAsync(taskId, answer);

    public Task<TaskResponse> GetTaskAsync(string taskId) => client.GetTask(taskId);

    public Task<List<TaskResponse>> GetPendingAsync(string round, string taskType) =>
        client.GetTasksAsync(round, taskType, TaskStatus.Pending, 0, 50);
}
