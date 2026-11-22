using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DemoUIBuilder
{
    private const string ScenePath = "Assets/Scenes/01-GameScene.unity";
    private const string FontPath = "Assets/Fonts/ICE SDF.asset";

    private static TMP_FontAsset fontAsset;

    [MenuItem("Tools/Build Demo UI")]
    public static void Build()
    {
        fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset == null)
        {
            Debug.LogWarning("Font asset not found: " + FontPath);
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the active scene.");
            return;
        }

        RemoveIfExists(canvas.transform, "HUD");
        RemoveIfExists(canvas.transform, "MainMenuUI");
        RemoveIfExists(canvas.transform, "PauseUI");
        RemoveIfExists(canvas.transform, "FailUI");

        BuildHUD(canvas.transform);
        BuildMainMenu(canvas.transform);
        BuildPause(canvas.transform);
        BuildFail(canvas.transform);

        SetupManager();
        SetupPlayerWeapons();
        AddSceneToBuildSettings();

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Demo UI built and scene saved.");
    }

    private static void SetupManager()
    {
        GameObject manager = GameObject.Find("Manager");
        if (manager == null)
        {
            Debug.LogWarning("Manager object not found.");
            return;
        }
        if (manager.GetComponent<GameFlowManager>() == null)
        {
            manager.AddComponent<GameFlowManager>();
        }
    }

    private static void SetupPlayerWeapons()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (player == null) return;

        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        if (playerAttack == null) return;

        playerAttack.scytheItem = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/DataSO/Weapon_Scythe.asset");
        playerAttack.javelinItem = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/DataSO/Weapon_Javelin.asset");
        EditorUtility.SetDirty(playerAttack);
    }

    private static void AddSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    private static void BuildHUD(Transform canvas)
    {
        GameObject root = CreateUI("HUD", canvas);
        root.AddComponent<HUDUI>();
        SetRect(Rect(root), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(380f, 64f));

        GameObject ui = CreateUI("UI", root.transform);
        Stretch(Rect(ui));

        TextMeshProUGUI label = CreateText(ui.transform, "HPLabel", "生命值", 16,
            new Color(0.9f, 0.9f, 0.9f), TextAlignmentOptions.MidlineLeft);
        SetRect(Rect(label.gameObject), new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(120f, 20f));

        GameObject barBg = CreateUI("HPBarBg", ui.transform);
        SetRect(Rect(barBg), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -24f), new Vector2(260f, 26f));
        AddImage(barBg, new Color(0.08f, 0.08f, 0.08f, 0.85f), false);

        GameObject fill = CreateUI("HPBarFill", barBg.transform);
        RectTransform fillRect = Rect(fill);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        AddImage(fill, new Color(0.82f, 0.18f, 0.18f, 1f), false);

        TextMeshProUGUI hpText = CreateText(ui.transform, "HPText", "100/100", 20,
            Color.white, TextAlignmentOptions.MidlineLeft);
        SetRect(Rect(hpText.gameObject), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(268f, -24f), new Vector2(100f, 26f));
    }

    private static void BuildMainMenu(Transform canvas)
    {
        GameObject root = CreatePanel("MainMenuUI", canvas, new Color(0f, 0f, 0f, 0.8f));
        root.AddComponent<MainMenuUI>();
        GameObject ui = CreateUI("UI", root.transform);
        Stretch(Rect(ui));

        TextMeshProUGUI title = CreateText(ui.transform, "Title", "冒险开始", 48,
            Color.white, TextAlignmentOptions.Center);
        SetRect(Rect(title.gameObject), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 90f));

        CreateButton(ui.transform, "StartButton", "开始冒险", new Vector2(0f, 10f), new Vector2(320f, 56f));
        CreateButton(ui.transform, "QuitButton", "离开", new Vector2(0f, -60f), new Vector2(320f, 56f));

        TextMeshProUGUI hint = CreateText(ui.transform, "Hint",
            "移动：WASD / 攻击：点击 / 交互：点击\n切换武器：1 / 2 / 属性：E / 背包：B / 菜单：Esc",
            18, new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.Center);
        SetRect(Rect(hint.gameObject), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(900f, 70f));
    }

    private static void BuildPause(Transform canvas)
    {
        GameObject root = CreatePanel("PauseUI", canvas, new Color(0f, 0f, 0f, 0.65f));
        root.AddComponent<PauseUI>();
        GameObject ui = CreateUI("UI", root.transform);
        Stretch(Rect(ui));

        TextMeshProUGUI title = CreateText(ui.transform, "Title", "菜单", 48,
            Color.white, TextAlignmentOptions.Center);
        SetRect(Rect(title.gameObject), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 90f));

        CreateButton(ui.transform, "ContinueButton", "继续", new Vector2(0f, 30f), new Vector2(320f, 56f));
        CreateButton(ui.transform, "RestartButton", "重新开始", new Vector2(0f, -40f), new Vector2(320f, 56f));
        CreateButton(ui.transform, "MenuButton", "回到主菜单", new Vector2(0f, -110f), new Vector2(320f, 56f));
    }

    private static void BuildFail(Transform canvas)
    {
        GameObject root = CreatePanel("FailUI", canvas, new Color(0.25f, 0f, 0f, 0.72f));
        root.AddComponent<FailUI>();
        GameObject ui = CreateUI("UI", root.transform);
        Stretch(Rect(ui));

        TextMeshProUGUI title = CreateText(ui.transform, "Title", "冒险结束", 48,
            Color.white, TextAlignmentOptions.Center);
        SetRect(Rect(title.gameObject), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 90f));

        CreateButton(ui.transform, "RestartButton", "重新开始", new Vector2(0f, 10f), new Vector2(320f, 56f));
        CreateButton(ui.transform, "MenuButton", "回到主菜单", new Vector2(0f, -60f), new Vector2(320f, 56f));
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject root = CreateUI(name, parent);
        Stretch(Rect(root));
        AddImage(root, color, true);
        return root;
    }

    private static GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    private static RectTransform Rect(GameObject go)
    {
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static Image AddImage(GameObject go, Color color, bool raycastTarget)
    {
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        float size, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = CreateUI(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null)
        {
            tmp.font = fontAsset;
        }
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        Stretch(Rect(go));
        return tmp;
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        GameObject go = CreateUI(name, parent);
        Image image = AddImage(go, new Color(0.16f, 0.2f, 0.28f, 0.95f), true);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        SetRect(Rect(go), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

        TextMeshProUGUI text = CreateText(go.transform, "Label", label, 24,
            Color.white, TextAlignmentOptions.Center);
        Stretch(Rect(text.gameObject));
    }

    private static void RemoveIfExists(Transform parent, string name)
    {
        Transform target = parent.Find(name);
        if (target != null)
        {
            Object.DestroyImmediate(target.gameObject);
        }
    }
}
