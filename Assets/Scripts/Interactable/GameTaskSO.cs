using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameTaskState
{
    Waiting,
    Executing,
    Completed,
    End
}

[CreateAssetMenu()]
public class GameTaskSO : ScriptableObject
{
    [Serializable]
    public class ItemRequirement
    {
        public ItemSO item;
        public int count = 1;
    }

    public GameTaskState state;

    public string[] diague;

    public ItemSO startReward;
    public ItemSO endReward;

    [Header("Submit requirements")]
    public List<ItemRequirement> requirements = new List<ItemRequirement>();

    public void Start()
    {
        state = GameTaskState.Executing;
    }

    public bool IsRequirementMet()
    {
        if (requirements == null || InventoryManager.Instance == null) return false;

        foreach (ItemRequirement requirement in requirements)
        {
            if (requirement == null || requirement.item == null || requirement.count <= 0) continue;
            if (InventoryManager.Instance.CountItem(requirement.item) < requirement.count) return false;
        }
        return true;
    }

    public void ConsumeRequirements()
    {
        if (requirements == null || InventoryManager.Instance == null) return;

        foreach (ItemRequirement requirement in requirements)
        {
            if (requirement == null || requirement.item == null || requirement.count <= 0) continue;
            InventoryManager.Instance.RemoveItems(requirement.item, requirement.count);
        }
    }

    public void End()
    {
        state = GameTaskState.End;
    }
}
