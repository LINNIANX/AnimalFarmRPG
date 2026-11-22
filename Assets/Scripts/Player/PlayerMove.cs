using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    [Header("Enemy Collision")]
    [SerializeField] private float searchRadius = 2.5f;
    [SerializeField] private float maxPushPerFrame = 0.5f;

    private readonly Collider[] overlapHits = new Collider[16];
    private Collider bodyCollider;

    private NavMeshAgent playerAgent;
    private PlayerAttack playerAttack;
    private PlayerProperty playerProperty;

    // Start is called before the first frame update
    void Start()
    {
        playerAgent = GetComponent<NavMeshAgent>();
        playerAttack = GetComponent<PlayerAttack>();
        playerProperty = GetComponent<PlayerProperty>();
        playerAgent.updateRotation = false;
        bodyCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInputBlocked()) return;

        MoveByWASD();
        ResolveEnemyOverlap();

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleLeftClick();
        }
    }

    // NavMeshAgent.Move ignores avoidance, so nothing stops the player walking into
    // an enemy's collider; this pushes the player back out by the exact penetration.
    private void ResolveEnemyOverlap()
    {
        if (playerAgent == null || !playerAgent.enabled || !playerAgent.isOnNavMesh) return;
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

            // Keep the correction horizontal so the agent never gets lifted off the ground.
            direction.y = 0f;
            float length = direction.magnitude;
            if (length < 0.0001f) continue;

            push += direction / length * distance;
        }

        if (push.sqrMagnitude > 0.000001f)
        {
            playerAgent.Move(Vector3.ClampMagnitude(push, maxPushPerFrame));
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
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude < 0.01f) return;

        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cam.right;
        right.y = 0;
        right.Normalize();

        Vector3 dir = (forward * input.z + right * input.x).normalized;
        playerAgent.Move(dir * playerAgent.speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(dir), 15f * Time.deltaTime);
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
