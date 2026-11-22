using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JavelinWeapon : Weapon
{
    public float bulletSpeed;
    public float liftSpeed = 1.6f;
    public float spinSpeed = 8f;
    public GameObject bulletPrefab;
    private GameObject bulletGo;

    private void Start()
    {
        SpawnBullet();
    }


    public override void Attack()
    {
        if (bulletGo != null)
        {
            bulletGo.transform.parent = null;
            Rigidbody body = bulletGo.GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;
            // Gravity pulls the throw down; the lift component cancels most of
            // that drop out to ~10 m so auto-aimed throws still land.
            body.velocity = transform.forward * bulletSpeed + Vector3.up * liftSpeed;
            body.angularVelocity = new Vector3(
                Random.Range(-spinSpeed, spinSpeed),
                Random.Range(-spinSpeed, spinSpeed),
                Random.Range(-spinSpeed, spinSpeed));
            bulletGo.GetComponent<Collider>().enabled = true;
            Destroy(bulletGo, 10);
            bulletGo = null;
            Invoke("SpawnBullet", 0.5f);
        }
        else
        {
            return;
        }
    }

    private void SpawnBullet()
    {
        bulletGo = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
        bulletGo.transform.parent = transform;
        bulletGo.GetComponent<Collider>().enabled = false;

        // Held in hand, so the physics step must not pull it out of the player's
        // hand before it is thrown.
        Rigidbody heldBody = bulletGo.GetComponent<Rigidbody>();
        if (heldBody != null)
        {
            heldBody.isKinematic = true;
            heldBody.useGravity = false;
        }

        // The bullet spawns inside the shooter's own colliders; without this it
        // collides with the player on the first physics step and never leaves.
        Collider bulletCollider = bulletGo.GetComponent<Collider>();
        foreach (Collider shooterCollider in transform.root.GetComponentsInChildren<Collider>(true))
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider);
        }
        if (tag == Tag.INTERACTABLE)
        {
            Destroy(bulletGo.GetComponent<JavelinBullet>());

            bulletGo.tag = Tag.INTERACTABLE;
            PickableObject po= bulletGo.AddComponent<PickableObject>();
            po.itemSO = GetComponent<PickableObject>().itemSO;
            Rigidbody rgd = bulletGo.GetComponent<Rigidbody>();
            rgd.isKinematic = false;
            rgd.useGravity = true;

            rgd.constraints = ~RigidbodyConstraints.FreezeAll;
            bulletGo.GetComponent<Collider>().enabled = true;
            bulletGo.transform.parent = null;
            Destroy(this.gameObject);
        }
    }
}
