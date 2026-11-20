using System;
using System.Net.Http;
using Challenge;
using Challenge.DataContracts;
using ConsoleApp;
using ConsoleApp.Infra;
using Task = System.Threading.Tasks.Task;


// Данное приложение можно запускать под Windows, Linux, Mac.
// Для запуска приложения необходимо скачать и установить .NET 10.
// Скачать можно тут: https://dotnet.microsoft.com/download/dotnet


var teamSecret = SecretProvider.Get();
if (string.IsNullOrEmpty(teamSecret))
{
    Console.WriteLine("Задай секрет своей команды, чтобы можно было делать запросы от ее имени");
    Console.ReadLine();
    return;
}

var challengeClient = new ChallengeClient(teamSecret);

const string challengeId = "projects-course";
Console.WriteLine($"Нажми ВВОД, чтобы получить информацию о соревновании {challengeId}");
Console.ReadLine();
Console.WriteLine("Ожидание...");
var challenge = await challengeClient.GetChallengeAsync(challengeId);
Console.WriteLine(challenge.Description);
Console.WriteLine();
Console.WriteLine("----------------");
Console.WriteLine();

// Тип задач можно передать аргументом: dotnet run --project ConsoleApp -- determinant
var taskType = args.Length > 0 ? args[0] : "moment";

var utcNow = DateTime.UtcNow;
string currentRound = null;
foreach (var round in challenge.Rounds)
{
    if (round.StartTimestamp < utcNow && utcNow < round.EndTimestamp)
        currentRound = round.Id;
}

Console.WriteLine($"Нажми ВВОД, чтобы получить первые N взятых командой задач типа {taskType} в раунде {currentRound}");
Console.ReadLine();
Console.WriteLine("Ожидание...");
var firstTasks = await challengeClient.GetTasksAsync(currentRound, taskType, TaskStatus.Pending, 0, 50);
for (int i = 0; i < firstTasks.Count; i++)
{
    var task = firstTasks[i];
    Console.WriteLine($"  Задание {i + 1}, статус {task.Status}");
    Console.WriteLine($"  Формулировка: {task.UserHint}");
    Console.WriteLine($"                {task.Question}");
    Console.WriteLine();
}
Console.WriteLine("----------------");
Console.WriteLine();

Console.WriteLine($"ВНИМАНИЕ: взятие задачи типа {taskType} запускает таймер на сдачу.");
Console.WriteLine($"Сколько задач взять и решить в раунде {currentRound}? Введи число и нажми ВВОД (пусто или 0 — выход)");
if (!int.TryParse(Console.ReadLine()?.Trim(), out var tasksToSolve) || tasksToSolve <= 0)
    return;

// Решатель отвечает сам. Если он не смог решить задачу, или ответ оказался неверным,
// программа останавливается и показывает задачу, чтобы ответить вручную
int solved = 0;
for (int n = 1; n <= tasksToSolve; n++)
{
    Console.WriteLine($"=== Задача {n} из {tasksToSolve} ===");
    TaskResponse task;
    try
    {
        task = await challengeClient.AskNewTaskAsync(currentRound, taskType);
    }
    catch (ErrorResponseException e)
    {
        Console.WriteLine($"Не удалось взять задачу: {e.StatusCode} {e.ErrorName} {e.Message}");
        break;
    }
    catch (Exception e) when (e is HttpRequestException or System.Threading.Tasks.TaskCanceledException)
    {
        Console.WriteLine($"Сетевая ошибка при взятии задачи: {e.Message}");
        Console.WriteLine("Возможно, задача все же взята. Проверь список взятых задач.");
        break;
    }
    Console.WriteLine($"  Id:           {task.Id}");
    Console.WriteLine($"  Формулировка: {task.UserHint}");
    Console.WriteLine($"                {task.Question}");

    string answer;
    if (Solver.TrySolve(task, out answer))
    {
        Console.WriteLine($"  Ответ решателя: {answer}");
    }
    else
    {
        Console.WriteLine("  Решатель не смог решить задачу. Введи ответ сам и нажми ВВОД (пусто — выход без отправки)");
        answer = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(answer))
        {
            Console.WriteLine($"Задача {task.Id} осталась взятой и не сданной, таймер идет.");
            break;
        }
    }

    TaskResponse updatedTask;
    try
    {
        //Console.WriteLine($"Нажми ВВОД, чтобы ответить на полученную задачу самым правильным ответом: {answer}");
        //Console.ReadLine();
        updatedTask = await challengeClient.CheckTaskAnswerAsync(task.Id, answer);
    }
    catch (ErrorResponseException e)
    {
        Console.WriteLine($"Не удалось отправить ответ: {e.StatusCode} {e.ErrorName} {e.Message}");
        Console.WriteLine($"Задача {task.Id} осталась не сданной.");
        break;
    }
    catch (Exception e) when (e is HttpRequestException or System.Threading.Tasks.TaskCanceledException)
    {
        Console.WriteLine($"Сетевая ошибка при отправке ответа {answer}: {e.Message}");
        Console.WriteLine($"Ответ мог дойти до сервера. Проверь статус задачи {task.Id} перед повторной отправкой.");
        break;
    }
    Console.WriteLine($"  Статус: {updatedTask.Status}, очки: {updatedTask.Points}, ответ команды: {updatedTask.TeamAnswer}");
    if (updatedTask.Status != TaskStatus.Success)
    {
        Console.WriteLine("Ответ не принят. Останавливаюсь, чтобы ты разобрался с этой задачей:");
        Console.WriteLine($"  {updatedTask.UserHint}");
        Console.WriteLine($"  {updatedTask.Question}");
        break;
    }
    solved++;
    Console.WriteLine();
}
Console.WriteLine($"Решено задач: {solved}");
