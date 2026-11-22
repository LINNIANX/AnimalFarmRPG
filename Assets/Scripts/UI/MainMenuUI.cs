using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    private GameObject uiGameObject;

    public bool IsOpen => uiGameObject != null && uiGameObject.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiGameObject = gameObject;
        transform.Find("UI/StartButton").GetComponent<Button>().onClick.AddListener(OnStartButtonClick);
        transform.Find("UI/QuitButton").GetComponent<Button>().onClick.AddListener(OnQuitButtonClick);
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

    private void OnStartButtonClick()
    {
        GameFlowManager.Instance.StartGame();
    }

    private void OnQuitButtonClick()
    {
        GameFlowManager.Instance.QuitGame();
    }
}
