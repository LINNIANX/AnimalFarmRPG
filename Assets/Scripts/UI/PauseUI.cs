using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance { get; private set; }

    private GameObject uiGameObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiGameObject = gameObject;
        transform.Find("UI/ContinueButton").GetComponent<Button>().onClick.AddListener(OnContinueClick);
        transform.Find("UI/RestartButton").GetComponent<Button>().onClick.AddListener(OnRestartClick);
        transform.Find("UI/MenuButton").GetComponent<Button>().onClick.AddListener(OnMenuClick);
        uiGameObject.SetActive(false);
    }

    public void Show()
    {
        uiGameObject.SetActive(true);
    }

    public void Hide()
    {
        uiGameObject.SetActive(false);
    }

    private void OnContinueClick()
    {
        GameFlowManager.Instance.Resume();
    }

    private void OnRestartClick()
    {
        GameFlowManager.Instance.Restart();
    }

    private void OnMenuClick()
    {
        GameFlowManager.Instance.ReturnToMainMenu();
    }
}
