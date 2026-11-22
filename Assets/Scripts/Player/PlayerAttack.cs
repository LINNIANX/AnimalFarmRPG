using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Weapon weapon;
    public Sprite weaponIcon;
    public ItemSO scytheItem;
    public ItemSO javelinItem;
    public float attackCooldown = 0.5f;

    private float attackTimer;
    private int currentWeaponIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (weapon == null && scytheItem != null)
        {
            LoadWeapon(scytheItem);
        }
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer -= Time.deltaTime;

        if (IsInputBlocked()) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SwitchWeapon(scytheItem, 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SwitchWeapon(javelinItem, 1);
        }
    }

    private bool IsInputBlocked()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return true;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowing) return true;
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsShowing) return true;
        if (PlayerPropertyUI.Instance != null && PlayerPropertyUI.Instance.IsShowing) return true;

        PlayerProperty pp = GetComponent<PlayerProperty>();
        if (pp != null && pp.IsDead) return true;
        return false;
    }

    public void Attack()
    {
        if (IsInputBlocked()) return;
        if (attackTimer > 0 || weapon == null) return;

        weapon.Attack();
        attackTimer = attackCooldown;
    }

    private void SwitchWeapon(ItemSO item, int index)
    {
        if (item == null || currentWeaponIndex == index) return;

        LoadWeapon(item);

        if (MessageUI.Instance != null)
        {
            MessageUI.Instance.Show("切换武器：" + item.name);
        }
    }

    public void LoadWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }
    public void LoadWeapon(ItemSO itemSO)
    {
        if (itemSO == null) return;

        // Remove every weapon currently attached to the player (including the
        // scene's pre-placed weapon instances) so only one is ever visible.
        foreach (Weapon existing in GetComponentsInChildren<Weapon>(true))
        {
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }
        }
        weapon = null;

        string prefabName = itemSO.prefab.name;
        Transform weaponParent = transform.Find(prefabName + "Position");
        if (weaponParent == null)
        {
            Debug.LogError("Weapon position not found: " + prefabName + "Position");
            return;
        }

        GameObject weaponGo = GameObject.Instantiate(itemSO.prefab, weaponParent);
        weaponGo.transform.localPosition = Vector3.zero;
        weaponGo.transform.localRotation = Quaternion.identity;

        this.weapon = weaponGo.GetComponent<Weapon>();
        this.weaponIcon = itemSO.icon;

        if (itemSO == scytheItem) currentWeaponIndex = 0;
        else if (itemSO == javelinItem) currentWeaponIndex = 1;

        if (PlayerPropertyUI.Instance != null)
        {
            PlayerPropertyUI.Instance.UpdatePlayerPropertyUI();
        }
    }
    public void UnloadWeapon()
    {
        weapon = null;
    }

}
