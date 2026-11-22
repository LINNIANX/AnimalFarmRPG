using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    // Orbit pivot, measured from the player.
    public Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    public float distance = 9.02f;

    [Header("Orbit")]
    // Right-drag rotates; the left button stays free for the game's click-to-attack.
    public float yaw = 0f;
    public float pitch = 31.4f;
    public float minPitch = 5f;
    public float maxPitch = 75f;
    public float mouseSensitivity = 2f;

    [Header("Lens")]
    [Range(20f, 90f)]
    public float fieldOfView = 60f;

    private Camera cam;
    private Transform playerTransform;

    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        ApplyFieldOfView();

        GameObject player = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (player != null) playerTransform = player.transform;
        FollowPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;

        RotateByMouse();
    }

    private void RotateByMouse()
    {
        if (!Input.GetMouseButton(1)) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        // Pushing the mouse up looks up, which means lowering the camera towards the pivot.
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    // Lets the inspector slider preview the lens immediately, in edit and play mode alike.
    private void OnValidate()
    {
        ApplyFieldOfView();
    }

    private void ApplyFieldOfView()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null) cam.fieldOfView = fieldOfView;
    }

    // LateUpdate so the camera reads the player's position after the player has moved.
    private void LateUpdate()
    {
        FollowPlayer();

        if (shakeTimer <= 0f) return;

        shakeTimer -= Time.unscaledDeltaTime;
        float decay = Mathf.Clamp01(shakeTimer / shakeDuration);
        transform.position += Random.insideUnitSphere * (shakeMagnitude * decay);
    }

    private void FollowPlayer()
    {
        if (playerTransform == null) return;

        Vector3 pivot = playerTransform.position + lookOffset;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = pivot + orbit * (Vector3.back * distance);
        transform.LookAt(pivot);
    }

    public static void ShakeCamera(float duration = 0.15f, float magnitude = 0.12f)
    {
        if (Camera.main == null) return;

        CameraController controller = Camera.main.GetComponent<CameraController>();
        if (controller != null)
        {
            controller.Shake(duration, magnitude);
        }
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.12f)
    {
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeTimer = shakeDuration;
        shakeMagnitude = magnitude;
    }
}
