using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance!=null && Instance != this)
        {
            Destroy(gameObject);return;
        }
        Instance = this;
    }

    public List<ItemSO> itemList;

    public void AddItem(ItemSO item)
    {
        itemList.Add(item);
        InventoryUI.Instance.AddItem(item);

        MessageUI.Instance.Show("你获得了一个：" + item.name);
    }
    public void RemoveItem(ItemSO itemSO)
    {
        itemList.Remove(itemSO);
    }

    public int CountItem(ItemSO itemSO)
    {
        int count = 0;
        foreach (ItemSO item in itemList)
        {
            if (item == itemSO) count++;
        }
        return count;
    }

    // Used by quest hand-ins: removes the matching inventory entries and their UI slots.
    public bool RemoveItems(ItemSO itemSO, int count)
    {
        if (count <= 0 || CountItem(itemSO) < count) return false;

        for (int i = 0; i < count; i++)
        {
            itemList.Remove(itemSO);
        }
        InventoryUI.Instance.RemoveItem(itemSO, count);
        return true;
    }

}
