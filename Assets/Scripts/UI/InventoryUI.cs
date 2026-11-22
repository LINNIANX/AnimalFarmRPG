using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    private GameObject uiGameObject;
    private GameObject content;
    public GameObject itemPrefab;
    private bool isShow=false;

    public ItemDetailUI itemDetailUI;

    public bool IsShowing => uiGameObject != null && uiGameObject.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        { Destroy(gameObject); return; }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        uiGameObject = transform.Find("UI").gameObject;
        content = transform.Find("UI/ListBg/Scroll View/Viewport/Content").gameObject;
        Hide();
    }

    public void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isShow)
            {
                Hide();
                isShow = false;
            }
            else
            {
                Show();
                isShow = true;
            }
        }
    }


    public void Show()
    {
        uiGameObject.SetActive(true);
    }
    public void Hide()
    {
        uiGameObject.SetActive(false);
    }
    public void AddItem(ItemSO itemSO)
    {
        GameObject itemGo = GameObject.Instantiate(itemPrefab);
        itemGo.transform.SetParent(content.transform);
        ItemUI itemUI = itemGo.GetComponent<ItemUI>();
        
        itemUI.InitItem(itemSO);
    }
    public void RemoveItem(ItemSO itemSO, int count)
    {
        int removed = 0;
        foreach (Transform child in content.transform)
        {
            if (removed >= count) break;

            ItemUI itemUI = child.GetComponent<ItemUI>();
            if (itemUI == null || itemUI.itemSO != itemSO) continue;

            // Destroy is deferred to the end of the frame, so hide the slot now or the
            // next sibling pass would match the same one again.
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
            removed++;
        }
    }

    public void OnItemClick(ItemSO itemSO,ItemUI itemUI)
    {
        itemDetailUI.UpdateItemDetailUI(itemSO,itemUI);
    }

    public void OnItemUse(ItemSO itemSO,ItemUI itemUI)
    {
        Destroy(itemUI.gameObject);
        InventoryManager.Instance.RemoveItem(itemSO);

        GameObject.FindGameObjectWithTag(Tag.PLAYER).GetComponent<Player>().UseItem(itemSO);
    }
}
