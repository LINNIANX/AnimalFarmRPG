using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class EnemySetupBuilder
{
    private const string EnemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
    private const string WraithModelPath = "Assets/敌人/Wraith.fbx";
    private const string ControllerPath = "Assets/敌人/ZombieDemo.controller";
    private const string BloodPrefabPath = "Assets/Effects/Blood/Blood_particle.prefab";
    private const string HitClipPath = "Assets/Audio/Combat/EnemyHit.ogg";
    private const string AttackClipPath = "Assets/Audio/Combat/EnemyAttack.ogg";
    private const string DieClipPath = "Assets/Audio/Combat/EnemyDie.ogg";
    private const string DamagePopupPath = "Assets/Prefabs/DamagePopup.prefab";
    private const string FontPath = "Assets/Fonts/ICE SDF.asset";

    private const float TargetHeight = 1.8f;
    private const float ModelYaw = 0f;

    [MenuItem("Tools/Build Enemy Prefab")]
    public static void Build()
    {
        EnsureDamagePopupPrefab();

        GameObject wraithAsset = AssetDatabase.LoadAssetAtPath<GameObject>(WraithModelPath);
        if (wraithAsset == null)
        {
            Debug.LogError("Wraith model not found: " + WraithModelPath);
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
        try
        {
            contents.transform.localPosition = Vector3.zero;
            contents.transform.localRotation = Quaternion.identity;
            contents.transform.localScale = Vector3.one;

            RemoveCapsuleVisuals(contents);

            Transform oldModel = contents.transform.Find("Model");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject, true);

            GameObject modelRoot = new GameObject("Model");
            modelRoot.transform.SetParent(contents.transform, false);

            GameObject wraith = (GameObject)PrefabUtility.InstantiatePrefab(wraithAsset, modelRoot.transform);
            wraith.transform.localPosition = Vector3.zero;
            wraith.transform.localRotation = Quaternion.Euler(0f, ModelYaw, 0f);

            float sourceHeight = MeasureHeight(wraith);
            if (sourceHeight > 0.01f)
            {
                wraith.transform.localScale *= TargetHeight / sourceHeight;
            }

            AlignFeet(wraith, contents.transform);

            Animator animator = ConfigureAnimator(wraith);
            ConfigureAgentAndCollider(contents);
            Enemy enemy = ConfigureEnemy(contents, animator);

            PrefabUtility.SaveAsPrefabAsset(contents, EnemyPrefabPath);
            Debug.Log(string.Format("Enemy prefab rebuilt: height {0:F2}m -> {1:F2}m, animator {2}, audio clips {3}.",
                sourceHeight, TargetHeight, animator != null ? "ok" : "MISSING",
                enemy != null && enemy.hitClip != null ? "ok" : "MISSING"));
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
            AssetDatabase.SaveAssets();
        }
    }

    private static void RemoveCapsuleVisuals(GameObject root)
    {
        foreach (Component component in root.GetComponents<Component>())
        {
            if (component is MeshFilter || component is MeshRenderer)
            {
                Object.DestroyImmediate(component, true);
            }
        }

        Transform cube = root.transform.Find("Cube");
        if (cube != null) Object.DestroyImmediate(cube.gameObject, true);
    }

    private static Animator ConfigureAnimator(GameObject wraith)
    {
        Animator animator = wraith.GetComponent<Animator>();
        if (animator == null) animator = wraith.AddComponent<Animator>();

        animator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        animator.applyRootMotion = false;

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(WraithModelPath))
        {
            Avatar avatar = asset as Avatar;
            if (avatar == null) continue;

            animator.avatar = avatar;
            break;
        }

        if (animator.avatar == null || !animator.avatar.isValid)
        {
            Debug.LogWarning("Wraith has no valid Humanoid avatar; animations may not play.");
        }

        return animator;
    }

    private static void ConfigureAgentAndCollider(GameObject root)
    {
        NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.radius = 0.35f;
            agent.height = TargetHeight;
            agent.baseOffset = 0f;
        }

        CapsuleCollider body = root.GetComponent<CapsuleCollider>();
        if (body != null)
        {
            body.center = new Vector3(0f, TargetHeight * 0.5f, 0f);
            body.height = TargetHeight;
            body.radius = 0.35f;
        }
    }

    private static Enemy ConfigureEnemy(GameObject root, Animator animator)
    {
        Enemy enemy = root.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy component missing on " + EnemyPrefabPath);
            return null;
        }

        AudioSource source = root.GetComponent<AudioSource>();
        if (source == null) source = root.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.minDistance = 2f;
        source.maxDistance = 25f;

        enemy.animator = animator;
        enemy.audioSource = source;
        enemy.hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HitClipPath);
        enemy.attackClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AttackClipPath);
        enemy.dieClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DieClipPath);

        GameObject blood = AssetDatabase.LoadAssetAtPath<GameObject>(BloodPrefabPath);
        enemy.hitEffectPrefab = blood != null ? blood.GetComponent<ParticleSystem>() : null;
        enemy.damagePopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DamagePopupPath);

        return enemy;
    }

    private static void EnsureDamagePopupPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(DamagePopupPath) != null) return;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        GameObject popup = new GameObject("DamagePopup");
        TextMeshPro text = popup.AddComponent<TextMeshPro>();
        if (font != null)
        {
            text.font = font;
            text.fontSharedMaterial = font.material;
        }
        text.fontSize = 5f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.85f, 0.2f);
        text.text = "0";
        popup.AddComponent<DamagePopup>();

        PrefabUtility.SaveAsPrefabAsset(popup, DamagePopupPath);
        Object.DestroyImmediate(popup);
    }

    private static float MeasureHeight(GameObject model)
    {
        Bounds bounds;
        if (TryGetBounds(model, out bounds)) return bounds.size.y;
        return 0f;
    }

    private static void AlignFeet(GameObject model, Transform root)
    {
        Bounds bounds;
        if (!TryGetBounds(model, out bounds)) return;

        Vector3 rootPosition = root.position;
        model.transform.localPosition += new Vector3(
            rootPosition.x - bounds.center.x,
            rootPosition.y - bounds.min.y,
            rootPosition.z - bounds.center.z);
    }

    private static bool TryGetBounds(GameObject model, out Bounds bounds)
    {
        bounds = new Bounds(model.transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is MeshRenderer && renderer.GetComponent<MeshFilter>() == null) continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds && bounds.size.y > 0.0001f;
    }
}
