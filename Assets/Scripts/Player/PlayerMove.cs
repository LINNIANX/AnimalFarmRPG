using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private float gravity = -20f;

    [Header("Enemy Collision")]
    [SerializeField] private float searchRadius = 2.5f;
    [SerializeField] private float maxPushPerFrame = 0.5f;

    private readonly Collider[] overlapHits = new Collider[16];
    private BoxCollider bodyCollider;

    private CharacterController controller;
    private PlayerAttack playerAttack;
    private PlayerProperty playerProperty;

    private float verticalVelocity;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerProperty = GetComponent<PlayerProperty>();
        bodyCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInputBlocked()) return;

        MoveByWASD();

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleLeftClick();
        }
    }

    // Enemies still move through NavMeshAgent, whose position is applied after
    // Update, so the overlap has to be resolved here to be gone before rendering.
    private void LateUpdate()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;
        if (playerProperty != null && playerProperty.IsDead) return;

        ResolveEnemyOverlap();
    }

    // CharacterController only resolves overlaps during its own Move, so an enemy
    // walking into a standing player still has to be pushed out explicitly.
    private void ResolveEnemyOverlap()
    {
        if (controller == null || !controller.enabled) return;
        if (bodyCollider == null) return;

        int count = Physics.OverlapSphereNonAlloc(transform.position, searchRadius,
            overlapHits, ~0, QueryTriggerInteraction.Ignore);

        Vector3 push = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Collider other = overlapHits[i];
            if (other == null || !other.enabled || other.isTrigger || other == bodyCollider) continue;
            if (other.GetComponentInParent<Enemy>() == null) continue;

            Vector3 direction;
            float distance;
            if (!Physics.ComputePenetration(bodyCollider, transform.position, transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out direction, out distance))
            {
                continue;
            }

            // Keep the correction horizontal so the controller never gets lifted off the ground.
            direction.y = 0f;
            float length = direction.magnitude;
            if (length < 0.0001f) continue;

            push += direction / length * distance;
        }

        if (push.sqrMagnitude > 0.000001f)
        {
            controller.Move(Vector3.ClampMagnitude(push, maxPushPerFrame));
        }
    }

    private bool IsInputBlocked()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return true;
        if (playerProperty != null && playerProperty.IsDead) return true;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowing) return true;
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsShowing) return true;
        if (PlayerPropertyUI.Instance != null && PlayerPropertyUI.Instance.IsShowing) return true;
        return false;
    }

    private void MoveByWASD()
    {
        if (controller == null || !controller.enabled) return;

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        // This game has no jump; gravity only keeps the controller stuck to the ground.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = Vector3.up * verticalVelocity;

        if (input.sqrMagnitude >= 0.01f)
        {
            Transform cam = Camera.main.transform;
            Vector3 forward = cam.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = cam.right;
            right.y = 0;
            right.Normalize();

            Vector3 dir = (forward * input.z + right * input.x).normalized;
            velocity += dir * moveSpeed;

            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            InteractableObject interactable = hit.collider.GetComponentInParent<InteractableObject>();
            if (interactable != null)
            {
                interactable.TryInteract(transform);
                return;
            }
        }

        playerAttack.Attack();
    }
}
