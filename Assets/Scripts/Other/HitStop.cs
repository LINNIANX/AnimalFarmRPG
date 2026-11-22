using System.Collections;
using UnityEngine;

public static class HitStop
{
    private static bool isRunning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        isRunning = false;
    }

    // A scene reload can interrupt the running coroutine, so the flag must be
    // cleared explicitly or hit stop would stay disabled for the new scene.
    public static void Reset()
    {
        isRunning = false;
    }

    public static void Play(float duration = 0.06f, float timeScale = 0.05f)
    {
        if (isRunning) return;
        if (GameFlowManager.Instance == null) return;
        if (GameFlowManager.Instance.IsPaused) return;

        GameFlowManager.Instance.StartCoroutine(Run(duration, timeScale));
    }

    private static IEnumerator Run(float duration, float timeScale)
    {
        isRunning = true;
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused ? 0f : 1f;
        isRunning = false;
    }
}
