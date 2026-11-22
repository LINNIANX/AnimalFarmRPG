using UnityEngine;

// World-space health bar above the enemy. Two billboarded quads (background + fill)
// so it needs no UI canvas or sprite assets.
[RequireComponent(typeof(Enemy))]
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private float width = 1.15f;
    [SerializeField] private float height = 0.13f;
    [SerializeField] private float heightAbove = 2.3f;
    [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.15f, 0.15f, 1f);

    private Enemy enemy;
    private Transform barRoot;
    private Transform fill;
    private Renderer[] renderers;
    private Camera cam;
    private Transform player;
    private int maxHp;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        maxHp = Mathf.Max(1, enemy.HP);
        Build();
    }

    private void Start()
    {
        GameObject found = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (found != null) player = found.transform;
    }

    private void LateUpdate()
    {
        if (barRoot == null || enemy == null) return;

        if (enemy.HP <= 0)
        {
            SetVisible(false);
            return;
        }

        float ratio = Mathf.Clamp01((float)enemy.HP / maxHp);
        fill.localScale = new Vector3(width * ratio, height, 1f);
        fill.localPosition = new Vector3(-width * (1f - ratio) * 0.5f, 0f, -0.01f);

        if (cam == null) cam = Camera.main;
        if (cam != null) barRoot.rotation = cam.transform.rotation;

        bool playerNear = player != null &&
            Vector3.Distance(transform.position, player.position) <= enemy.detectRange;
        SetVisible(ratio < 0.9999f || playerNear);
    }

    private void SetVisible(bool visible)
    {
        if (renderers == null) return;
        foreach (Renderer r in renderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    private void Build()
    {
        float parentScale = Mathf.Abs(transform.lossyScale.x);
        if (parentScale < 0.0001f) parentScale = 1f;

        GameObject root = new GameObject("HealthBar");
        root.transform.SetParent(transform, false);
        root.transform.localScale = Vector3.one / parentScale;
        root.transform.localPosition = new Vector3(0f, heightAbove / parentScale, 0f);
        barRoot = root.transform;

        GameObject bg = CreateQuad("Bg", barRoot, backgroundColor, 3000);
        bg.transform.localScale = new Vector3(width, height, 1f);
        bg.transform.localPosition = Vector3.zero;

        GameObject fillGo = CreateQuad("Fill", barRoot, fillColor, 3001);
        fill = fillGo.transform;
        fill.localScale = new Vector3(width, height, 1f);
        fill.localPosition = new Vector3(0f, 0f, -0.01f);

        renderers = new Renderer[] { bg.GetComponent<Renderer>(), fillGo.GetComponent<Renderer>() };
        SetVisible(false);
    }

    private static GameObject CreateQuad(string name, Transform parent, Color color, int renderQueue)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Destroy(quad.GetComponent<Collider>());

        quad.transform.SetParent(parent, false);

        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        material.renderQueue = renderQueue;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return quad;
    }
}
