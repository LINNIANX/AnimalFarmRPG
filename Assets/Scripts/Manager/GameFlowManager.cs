using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    // Set before reloading the scene to decide what the next load shows.
    public static bool ShowMainMenuOnLoad = true;

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    private PlayerProperty playerProperty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ShowMainMenuOnLoad = true;
    }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        HitStop.Reset();
        // Runs before PlayerProperty.Awake re-subscribes, so stale subscriptions
        // from the previous load (e.g. a running GameTaskSO) are dropped.
        EventCenter.Clear();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (player != null)
        {
            playerProperty = player.GetComponent<PlayerProperty>();
            if (playerProperty != null)
            {
                playerProperty.OnPlayerDied += GameOver;
            }
        }

        if (ShowMainMenuOnLoad)
        {
            ShowMainMenuOnLoad = false;
            ShowMainMenu();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void OnDestroy()
    {
        if (playerProperty != null)
        {
            playerProperty.OnPlayerDied -= GameOver;
        }
    }

    private void Update()
    {
        if (IsGameOver) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (MainMenuUI.Instance != null && MainMenuUI.Instance.IsOpen) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowing) return;

        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void ShowMainMenu()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (MainMenuUI.Instance != null) MainMenuUI.Instance.Show();
        if (HUDUI.Instance != null) HUDUI.Instance.Hide();
    }

    public void StartGame()
    {
        if (MainMenuUI.Instance != null) MainMenuUI.Instance.Hide();
        IsPaused = false;
        Time.timeScale = 1f;
        if (HUDUI.Instance != null) HUDUI.Instance.Show();
    }

    public void Pause()
    {
        if (IsGameOver) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (PauseUI.Instance != null) PauseUI.Instance.Show();
    }

    public void Resume()
    {
        if (IsGameOver) return;
        IsPaused = false;
        Time.timeScale = 1f;
        if (PauseUI.Instance != null) PauseUI.Instance.Hide();
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsPaused = true;
        Time.timeScale = 0f;
        if (PauseUI.Instance != null) PauseUI.Instance.Hide();
        if (FailUI.Instance != null) FailUI.Instance.Show();
    }

    public void Restart()
    {
        ShowMainMenuOnLoad = false;
        Time.timeScale = 1f;
        EventCenter.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenuOnLoad = true;
        Time.timeScale = 1f;
        EventCenter.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
