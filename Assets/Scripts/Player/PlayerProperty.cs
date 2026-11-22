using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProperty : MonoBehaviour
{
    public Dictionary<PropertyType, List<Property>> propertyDict;
    public int hpValue=100;
    public int energyValue=30;
    public int level = 1;
    public int currentExp = 0;

    public const int MaxEnergy = 100;

    public bool IsDead { get; private set; }

    public event Action<int> OnHPChanged;
    public event Action<int> OnEnergyChanged;
    public event Action OnPlayerDied;

    private float hungerTick;

    // Start is called before the first frame update
    void Awake()
    {
        propertyDict = new Dictionary<PropertyType, List<Property>>();

        propertyDict.Add(PropertyType.SpeedValue, new List<Property>());
        propertyDict.Add(PropertyType.AttackValue, new List<Property>());

        AddProperty(PropertyType.SpeedValue, 5);
        AddProperty(PropertyType.AttackValue, 20);

        EventCenter.OnEnemyDied += OnEnemyDied;
    }

    private void Update()
    {
        if (IsDead) return;
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;

        hungerTick += Time.deltaTime;
        if (hungerTick < 1f) return;
        hungerTick -= 1f;

        if (energyValue > 0)
        {
            energyValue--;
            OnEnergyChanged?.Invoke(energyValue);
            return;
        }

        // Starving: an empty stomach costs health instead.
        TakeDamage(2);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        hpValue = Mathf.Max(0, hpValue - damage);
        OnHPChanged?.Invoke(hpValue);

        if (hpValue <= 0)
        {
            IsDead = true;
            OnPlayerDied?.Invoke();
        }
    }

    public void UseDrug(ItemSO itemSO)
    {
        foreach(Property p in itemSO.propertyList)
        {
            AddProperty(p.propertyType, p.value);
        }
    }

    public void AddProperty(PropertyType pt,int value)
    {
        switch (pt)
        {
            case PropertyType.HPValue:
                hpValue = Mathf.Min(100, hpValue + value);
                OnHPChanged?.Invoke(hpValue);
                return;
            case PropertyType.EnergyValue:
                energyValue = Mathf.Clamp(energyValue + value, 0, MaxEnergy);
                OnEnergyChanged?.Invoke(energyValue);
                return;
        }

        List<Property> list;
        propertyDict.TryGetValue(pt, out list);
        list.Add(new Property(pt,value));
    }
    public void RemoveProperty(PropertyType pt, int value)
    {
        switch (pt)
        {
            case PropertyType.HPValue:
                hpValue = Mathf.Max(0, hpValue - value);
                OnHPChanged?.Invoke(hpValue);
                return;
            case PropertyType.EnergyValue:
                energyValue = Mathf.Max(0, energyValue - value);
                OnEnergyChanged?.Invoke(energyValue);
                return;
        }

        List<Property> list;
        propertyDict.TryGetValue(pt, out list);

        list.Remove(list.Find(x => x.value == value));
    }
    private void OnDestroy()
    {
        EventCenter.OnEnemyDied -= OnEnemyDied;
    }
    private void OnEnemyDied(Enemy enemy)
    {
        this.currentExp += enemy.exp;

        if (currentExp >= level * 30)
        {
            currentExp -= level * 30;
            level++;
        }
        PlayerPropertyUI.Instance.UpdatePlayerPropertyUI();
    }
}
