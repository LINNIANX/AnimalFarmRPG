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

    [Header("Audio")]
    [SerializeField] private AudioClip hurtClip;

    [Header("Feedback")]
    [SerializeField] private float hitFlashTime = 0.1f;
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private Color damagePopupColor = new Color(1f, 0.35f, 0.35f);

    private Material[] flashMaterials;
    private Coroutine flashRoutine;

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
        CacheFlashMaterials();
    }

    private void CacheFlashMaterials()
    {
        Renderer body = GetComponent<Renderer>();
        if (body == null) return;

        List<Material> cached = new List<Material>();
        foreach (Material material in body.materials)
        {
            if (material == null || !material.HasProperty("_EmissionColor")) continue;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            cached.Add(material);
        }
        flashMaterials = cached.ToArray();
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
        TakeDamage(2, false);
    }

    public void TakeDamage(int damage, bool showFeedback = true)
    {
        if (IsDead) return;

        hpValue = Mathf.Max(0, hpValue - damage);
        OnHPChanged?.Invoke(hpValue);

        if (showFeedback && damage > 0)
        {
            if (hurtClip != null) AudioSource.PlayClipAtPoint(hurtClip, transform.position);
            PlayHitFlash();
            SpawnDamagePopup(damage);
        }

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
    private void SpawnDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        GameObject popupObject = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 1.7f, Quaternion.identity);
        DamagePopup popup = popupObject.GetComponent<DamagePopup>();
        if (popup != null) popup.Initialize(damage, damagePopupColor);
    }

    private void PlayHitFlash()
    {
        if (flashMaterials == null || flashMaterials.Length == 0) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        SetEmission(Color.white * 2f);
        yield return new WaitForSecondsRealtime(hitFlashTime);
        SetEmission(Color.black);
        flashRoutine = null;
    }

    private void SetEmission(Color color)
    {
        foreach (Material material in flashMaterials)
        {
            if (material == null) continue;
            // Re-enable every time: a fresh material instance can reset the keyword.
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
        }
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
