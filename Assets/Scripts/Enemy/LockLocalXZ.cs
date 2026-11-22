using UnityEngine;

// The cartoon-animal clips carry baked root-motion curves that slide the whole model
// away from its collider (up to ~3 m on the attack clip). Keep the vertical hop and
// the squash, but pin the horizontal offset back to the authored pose.
public class LockLocalXZ : MonoBehaviour
{
    private float baseX;
    private float baseZ;

    private void Awake()
    {
        baseX = transform.localPosition.x;
        baseZ = transform.localPosition.z;
    }

    private void LateUpdate()
    {
        Vector3 position = transform.localPosition;
        position.x = baseX;
        position.z = baseZ;
        transform.localPosition = position;
    }
}
