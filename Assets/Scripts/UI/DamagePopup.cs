using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float lifetime = 0.9f;
    public float riseSpeed = 1.5f;
    public float spreadRadius = 0.2f;

    private TextMeshPro text;
    private Color baseColor;
    private float elapsed;

    public void Initialize(int damage, Color color)
    {
        text = GetComponent<TextMeshPro>();
        if (text != null)
        {
            text.text = damage.ToString();
            text.color = color;
        }

        baseColor = color;
        transform.position += new Vector3(
            Random.Range(-spreadRadius, spreadRadius),
            0f,
            Random.Range(-spreadRadius, spreadRadius));
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        if (text == null) return;

        Color color = baseColor;
        color.a = Mathf.Clamp01(1f - elapsed / lifetime);
        text.color = color;

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
