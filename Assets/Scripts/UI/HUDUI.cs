using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    public static HUDUI Instance { get; private set; }

    private GameObject uiGameObject;
    private RectTransform hpFill;
    private TextMeshProUGUI hpText;
    private RectTransform energyFill;
    private TextMeshProUGUI energyText;
    private PlayerProperty playerProperty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiGameObject = gameObject;
        hpFill = transform.Find("UI/HPBarBg/HPBarFill").GetComponent<RectTransform>();
        hpText = transform.Find("UI/HPText").GetComponent<TextMeshProUGUI>();

        Transform fill = transform.Find("UI/HungerBarBg/HungerBarFill");
        if (fill != null) energyFill = fill.GetComponent<RectTransform>();
        Transform label = transform.Find("UI/HungerText");
        if (label != null) energyText = label.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (player != null)
        {
            playerProperty = player.GetComponent<PlayerProperty>();
        }

        if (playerProperty != null)
        {
            playerProperty.OnHPChanged += UpdateHP;
            playerProperty.OnEnergyChanged += UpdateEnergy;
            UpdateHP(playerProperty.hpValue);
            UpdateEnergy(playerProperty.energyValue);
        }
    }

    private void OnDestroy()
    {
        if (playerProperty != null)
        {
            playerProperty.OnHPChanged -= UpdateHP;
            playerProperty.OnEnergyChanged -= UpdateEnergy;
        }
    }

    public void Show()
    {
        if (uiGameObject != null) uiGameObject.SetActive(true);
    }

    public void Hide()
    {
        if (uiGameObject != null) uiGameObject.SetActive(false);
    }

    private void UpdateHP(int hp)
    {
        if (hpFill != null)
        {
            hpFill.anchorMax = new Vector2(Mathf.Clamp01(hp / 100f), 1f);
        }
        if (hpText != null)
        {
            hpText.text = hp + "/100";
        }
    }

    private void UpdateEnergy(int energy)
    {
        if (energyFill != null)
        {
            energyFill.anchorMax = new Vector2(Mathf.Clamp01(energy / (float)PlayerProperty.MaxEnergy), 1f);
        }
        if (energyText != null)
        {
            energyText.text = energy + "/" + PlayerProperty.MaxEnergy;
        }
    }
}
