using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        NormalState,
        FightingState,
        MovingState,
        RestingState
    }

    private const string SpeedParam = "speed";
    private const string AngularSpeedParam = "angularspeed";
    private const string AttackParam = "attack";
    private const string DamageParam = "damage";
    private const string DeathParam = "death";

    private EnemyState state = EnemyState.NormalState;
    private EnemyState childState = EnemyState.RestingState;
    private NavMeshAgent enemyAgent;

    public float restTime = 2;
    private float restTimer = 0;

    public int HP = 100;

    public int exp = 20;

    public int attackDamage = 10;
    public float detectRange = 8f;
    public float attackRange = 1.8f;
    public float attackCooldown = 2f;
    public float attackWindup = 0.5f;

    [Header("Drops")]
    // Falls back to a random entry from ItemDB when left empty.
    public ItemSO dropItem;

    [Header("Presentation")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip hitClip;
    public AudioClip attackClip;
    public AudioClip dieClip;
    public ParticleSystem hitEffectPrefab;
    public GameObject damagePopupPrefab;
    public float hitFlashTime = 0.1f;
    public float knockbackDistance = 0.6f;
    public float knockbackTime = 0.12f;

    private float attackCooldownTimer = 0;
    private bool isAttacking;
    private bool isDead;

    private PlayerProperty playerProperty;
    private Transform playerTransform;

    private Material[] flashMaterials;

    private bool hasSpeedParam;
    private bool hasAngularSpeedParam;
    private bool hasAttackParam;
    private bool hasDamageParam;
    private bool hasDeathParam;

    // Start is called before the first frame update
    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        CacheAnimatorParameters();
        CacheFlashMaterials();

        GameObject player = GameObject.FindGameObjectWithTag(Tag.PLAYER);
        if (player != null)
        {
            playerProperty = player.GetComponent<PlayerProperty>();
            playerTransform = player.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowing) return;
        if (playerProperty == null || playerTransform == null) return;

        attackCooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (playerProperty.IsDead)
        {
            if (state == EnemyState.FightingState)
            {
                state = EnemyState.NormalState;
                childState = EnemyState.RestingState;
                restTimer = 0;
                isAttacking = false;
                enemyAgent.isStopped = false;
            }
            UpdateWander();
            UpdateLocomotionAnimation();
            return;
        }

        if (state == EnemyState.FightingState)
        {
            UpdateFight(distance);
            UpdateLocomotionAnimation();
            return;
        }

        if (distance <= detectRange)
        {
            state = EnemyState.FightingState;
            enemyAgent.isStopped = false;
            UpdateFight(distance);
            UpdateLocomotionAnimation();
            return;
        }

        UpdateWander();
        UpdateLocomotionAnimation();
    }

    private void UpdateFight(float distance)
    {
        if (distance > detectRange * 1.5f)
        {
            state = EnemyState.NormalState;
            childState = EnemyState.RestingState;
            restTimer = 0;
            isAttacking = false;
            enemyAgent.isStopped = false;
            return;
        }

        FacePlayer();

        if (distance <= attackRange)
        {
            enemyAgent.isStopped = true;
            if (!isAttacking && attackCooldownTimer <= 0f)
            {
                StartCoroutine(AttackRoutine());
            }
            return;
        }

        if (isAttacking) return;

        enemyAgent.isStopped = false;
        enemyAgent.SetDestination(playerTransform.position);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;
        SetSpeedParameter(0f);

        if (enemyAgent != null) enemyAgent.isStopped = true;
        FacePlayer();

        if (hasAttackParam) animator.SetTrigger(AttackParam);
        PlaySound(attackClip, 0.9f);

        yield return new WaitForSeconds(attackWindup);

        if (!isDead && playerProperty != null && !playerProperty.IsDead && playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) <= attackRange * 1.3f)
        {
            playerProperty.TakeDamage(attackDamage);
            CameraController.ShakeCamera(0.18f, 0.08f);
        }

        isAttacking = false;
    }

    private void UpdateWander()
    {
        if (childState == EnemyState.RestingState)
        {
            restTimer += Time.deltaTime;

            if (restTimer > restTime)
            {
                enemyAgent.isStopped = false;
                Vector3 randomPosition = FindRandomPosition();
                enemyAgent.SetDestination(randomPosition);
                childState = EnemyState.MovingState;
            }
        }
        else if (childState == EnemyState.MovingState)
        {
            if (!enemyAgent.pathPending &&
                (!enemyAgent.hasPath || enemyAgent.remainingDistance <= enemyAgent.stoppingDistance + 0.1f))
            {
                restTimer = 0;
                childState = EnemyState.RestingState;
            }
        }
    }

    Vector3 FindRandomPosition()
    {
        Vector3 randomDir = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
        return transform.position + randomDir.normalized * Random.Range(2, 5);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        HP -= damage;

        if (hasDamageParam) animator.SetTrigger(DamageParam);
        PlaySound(hitClip, 1f);
        SpawnHitEffect();
        SpawnDamagePopup(damage);
        StartCoroutine(HitFlashRoutine());
        StartCoroutine(KnockbackRoutine());
        HitStop.Play();

        if (HP <= 0)
        {
            HP = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;

        SetEmission(Color.black);
        StopAllCoroutines();

        if (enemyAgent != null)
        {
            enemyAgent.isStopped = true;
            enemyAgent.enabled = false;
        }

        Collider bodyCollider = GetComponent<Collider>();
        if (bodyCollider != null) bodyCollider.enabled = false;

        if (hasDeathParam) animator.SetTrigger(DeathParam);
        PlayDeathSound();

        EventCenter.EnemyDied(this);
        DropItems();

        Destroy(gameObject);
    }

    private void DropItems()
    {
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            SpawnPickableItem();
        }
    }

    // The enemy is destroyed this frame, so its own AudioSource would cut the clip off.
    private void PlayDeathSound()
    {
        if (dieClip == null) return;
        AudioSource.PlayClipAtPoint(dieClip, transform.position, 1f);
    }

    private void SpawnPickableItem()
    {
        ItemSO item = dropItem != null ? dropItem : ItemDBManager.Instance.GetRandomItem();
        //print(transform.position);
        GameObject go = GameObject.Instantiate(item.prefab, transform.position, Quaternion.identity);
        go.tag = Tag.INTERACTABLE;
        Animator anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }
        PickableObject po = go.AddComponent<PickableObject>();
        po.itemSO = item;

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
            collider.isTrigger = false;
        }
        Rigidbody rgd = go.GetComponent<Rigidbody>();
        if (rgd != null)
        {
            rgd.isKinematic = false;
            rgd.useGravity = true;
        }
    }

    private void FacePlayer()
    {
        if (playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(toPlayer), 10f * Time.deltaTime);
        }
    }

    private void UpdateLocomotionAnimation()
    {
        if (animator == null || !hasSpeedParam) return;

        float speed = enemyAgent != null ? enemyAgent.velocity.magnitude : 0f;
        animator.SetFloat(SpeedParam, speed > 0.15f ? 1f : 0f);
        if (hasAngularSpeedParam) animator.SetFloat(AngularSpeedParam, 0f);
    }

    private void SetSpeedParameter(float value)
    {
        if (animator != null && hasSpeedParam) animator.SetFloat(SpeedParam, value);
    }

    private IEnumerator HitFlashRoutine()
    {
        SetEmission(Color.white * 2f);
        yield return new WaitForSecondsRealtime(hitFlashTime);
        SetEmission(Color.black);
    }

    private IEnumerator KnockbackRoutine()
    {
        if (playerTransform == null) yield break;
        if (enemyAgent == null || !enemyAgent.enabled || !enemyAgent.isOnNavMesh) yield break;

        Vector3 direction = transform.position - playerTransform.position;
        direction.y = 0;
        if (direction.sqrMagnitude < 0.0001f) yield break;
        direction.Normalize();

        float speed = knockbackDistance / knockbackTime;
        float elapsed = 0f;
        while (elapsed < knockbackTime && !isDead)
        {
            float deltaTime = Time.unscaledDeltaTime;
            enemyAgent.Move(direction * (speed * deltaTime));
            elapsed += deltaTime;
            yield return null;
        }
    }

    private void CacheFlashMaterials()
    {
        List<Material> materials = new List<Material>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.materials)
            {
                if (material == null || !material.HasProperty("_EmissionColor")) continue;

                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                materials.Add(material);
            }
        }
        flashMaterials = materials.ToArray();
    }

    private void SetEmission(Color color)
    {
        if (flashMaterials == null) return;

        foreach (Material material in flashMaterials)
        {
            if (material == null) continue;
            // Re-enable on every flash: the renderer can hand back a fresh
            // material instance whose keyword state was reset.
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
        }
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null) return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            switch (parameter.name)
            {
                case SpeedParam: hasSpeedParam = true; break;
                case AngularSpeedParam: hasAngularSpeedParam = true; break;
                case AttackParam: hasAttackParam = true; break;
                case DamageParam: hasDamageParam = true; break;
                case DeathParam: hasDeathParam = true; break;
            }
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;

        ParticleSystem effect = Instantiate(hitEffectPrefab, transform.position + Vector3.up * 1.1f, Quaternion.identity);
        Destroy(effect.gameObject, 2f);
    }

    private void SpawnDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        GameObject popupObject = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 1.9f, Quaternion.identity);
        DamagePopup popup = popupObject.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Initialize(damage, new Color(1f, 0.85f, 0.2f));
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}
