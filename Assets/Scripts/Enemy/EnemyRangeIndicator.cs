using UnityEngine;

// Draws a dashed ring on the ground showing where the enemy's attack can reach.
// The radius matches the damage check in Enemy.AttackRoutine (attackRange * 1.3),
// so what the player sees is exactly where they get hit.
[RequireComponent(typeof(Enemy))]
public class EnemyRangeIndicator : MonoBehaviour
{
    [SerializeField] private bool alwaysVisible;
    [SerializeField] private Color ringColor = new Color(0.85f, 0.12f, 0.12f, 0.85f);

    private Enemy enemy;
    private Renderer ringRenderer;
    private Transform player;

    private static Material ringMaterial;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        BuildRing();
    }

    private void Start()
    {
        GameObject found = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (found != null) player = found.transform;
    }

    private void Update()
    {
        if (ringRenderer == null) return;

        if (alwaysVisible)
        {
            ringRenderer.enabled = true;
            return;
        }

        ringRenderer.enabled = player != null &&
            Vector3.Distance(transform.position, player.position) <= enemy.detectRange;
    }

    private void BuildRing()
    {
        float diameter = enemy.attackRange * 1.3f * 2f;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "AttackRangeRing";
        Destroy(quad.GetComponent<Collider>());

        quad.transform.SetParent(transform, false);
        // The enemy root is scaled, so divide it out to keep the ring in metres.
        float parentScale = Mathf.Abs(transform.lossyScale.x);
        if (parentScale < 0.0001f) parentScale = 1f;
        quad.transform.localScale = new Vector3(diameter / parentScale, diameter / parentScale, 1f);
        quad.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        ringRenderer = quad.GetComponent<MeshRenderer>();
        ringRenderer.sharedMaterial = GetRingMaterial();
        ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ringRenderer.receiveShadows = false;
    }

    private Material GetRingMaterial()
    {
        if (ringMaterial == null)
        {
            ringMaterial = new Material(Shader.Find("Sprites/Default"));
            ringMaterial.mainTexture = BuildRingTexture(256, 6f, 44);
            ringMaterial.color = ringColor;
        }

        return ringMaterial;
    }

    // White dashes on transparent; the material colour tints them.
    private static Texture2D BuildRingTexture(int size, float widthPixels, int dashCount)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        float center = size * 0.5f;
        float outer = center - 1f;
        float inner = outer - widthPixels;
        const float gapRatio = 0.45f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center;
                float dy = y + 0.5f - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance > outer || distance < inner) continue;

                float angle = Mathf.Atan2(dy, dx) / (2f * Mathf.PI) + 0.5f;
                float phase = angle * dashCount;
                if (phase - Mathf.Floor(phase) < gapRatio) continue;

                pixels[y * size + x] = new Color32(255, 255, 255, 255);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }
}
