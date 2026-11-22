using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public float interactDistance = 2.5f;

    public bool TryInteract(Transform player)
    {
        if (player == null) return false;
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return false;

        Vector3 self = transform.position;
        Vector3 target = player.position;
        self.y = 0;
        target.y = 0;

        if ((self - target).sqrMagnitude > interactDistance * interactDistance)
        {
            if (MessageUI.Instance != null)
            {
                MessageUI.Instance.Show("靠近后才能交互");
            }
            return false;
        }

        Interact();
        return true;
    }

    protected virtual void Interact()
    {
        print("Interacting with Interactable Object.");
    }
}
