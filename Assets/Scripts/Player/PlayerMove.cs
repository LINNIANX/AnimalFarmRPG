using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
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
