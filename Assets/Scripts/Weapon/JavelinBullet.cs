using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JavelinBullet : MonoBehaviour
{
    public int atkValue = 30;
    private Rigidbody rgd;
    private Collider col;

    private void Awake()
    {
        rgd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Physics can fire before Start on a freshly spawned bullet.
        if (rgd == null) rgd = GetComponent<Rigidbody>();
        if (col == null) col = GetComponent<Collider>();
        if (rgd == null || col == null) return;

        if (collision.collider.tag == Tag.PLAYER) { return;
        }

        rgd.isKinematic = true;
        col.enabled = false;

        Destroy(this.gameObject, 1f);

        if (collision.gameObject.tag == Tag.ENEMY)
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(atkValue); 
        }
    }

}
