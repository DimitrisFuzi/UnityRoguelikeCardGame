using System.Collections;
using System.Threading.Tasks;

/// <summary>
/// Bridges async Tasks into Unity coroutines.
/// Usage: StartCoroutine(MyTask().AsCoroutine());
/// </summary>
public static class TaskExtensions
{
    public static IEnumerator AsCoroutine(this Task task)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.Exception != null)
            throw task.Exception;
    }
}
