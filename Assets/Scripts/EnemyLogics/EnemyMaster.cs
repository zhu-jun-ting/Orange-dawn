using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// All using directives must be at the top of the file

public class EnemyMaster : PawnMaster

{
    [Header("Parameters")]
    public float maxHP = 7f;
    public float moveSpeed = 1.5f;
    [HideInInspector] public float curHP = 7f;

    [Header("Attacking")]
    public GameObject enemyAOEPrefab; // Prefab for AOE attack
    public float attackScale = 1f; // Scale for AOE attack size
    public float attackDamage = 10f;
    public float attackDuration = 1f;
    public float attackCooldown = 2f; // Cooldown between attacks

    [Header("Melee Attack Detector")]
    public Collider2D meleeAttackDetector; // Collider to detect melee attacks
    public List<string> meleeAttackTags; // Tags to filter melee attacks, e.g., "Player", "Bullet", etc.

    [Header("Hurt Effects")]
    public SpriteRenderer sr;
    public float hurtDuration = 0.5f;
    protected Color originalColor; // Store the original color

    [Header("Game Objects")]
    public GameObject health_bar;
    public Transform target;


    [Header("Collidable Objects That Can Hurt Enemy")]
    // This can be used to specify which objects can hurt the enemy, e.g., player attacks, traps, etc.
    // You can add a list or array here if needed, or use tags/layers to filter collisions.
    public List<string> collidableObjectsTags;
    public List<string> collidableObjectsLayers;
    public float minCollisionDamage = 2f; // Minimum force to consider a hit as damage
    public float collisionDamageScale = 1f; // You can expose this as a public parameter if needed

    [Header("Enemy Dots")]
    public Transform dotSpawnPoint; // Point where DOT effects spawn
    public List<DotInfo> currentDots = new List<DotInfo>(); // Store active DOTs on this enemy
    
    [System.Serializable]
    public class DropEntry
    {
        public CombatManager.DropItem dropItem;
        public float chance;
    }

    [Header("Dropping Objects")]
    public List<DropEntry> dropEntries = new List<DropEntry>();






    // internal vars
    protected Rigidbody2D rb;
    protected float hitBackFactor;
    protected EnemyHealthBar enemyHealthBar;


    // singletons
    protected CombatManager combatManager;
    protected GameEvents gameEvents;
    protected bool is_alive;

    // Per-instigator damage cooldown
    protected Dictionary<GameObject, float> lastDamageTimes = new Dictionary<GameObject, float>();
    protected float damageCooldown = 0.1f;

    protected bool isFlashing = false;


    public virtual void Awake()
    {
        isEnemy = true;
    }


    public override void Start()
    {
        base.Start();
        curHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        enemyHealthBar = health_bar.GetComponent<EnemyHealthBar>();
        originalColor = sr.color; // Store the original color
        target = PlayerController.instance.transform;

        // get singleton references
        combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager == null) Debug.LogError("combat manager can not be found.");
        is_alive = true;

        gameEvents = GameEvents.instance;
        if (GameEvents.instance != null) GameEvents.instance.PawnSpawn(this);
    }

    public override void Update()
    {
        // If you need per-frame logic, add it here
        if (isBeingHitBack && rb != null)
        {
            if (rb.linearVelocity.magnitude < 0.05f)
            {
                isBeingHitBack = false;
            }
        }
        base.Update();
    }

    protected IEnumerator HurtFlashCoroutine()
    {
        if (sr != null && sr.material.HasProperty("_FlashAmount"))
        {
            float duration = 0.5f;
            float timer = 0f;
            sr.material.SetFloat("_FlashAmount", 1);
            isFlashing = true;
            while (timer < duration)
            {
                float t = timer / duration;
                sr.material.SetFloat("_FlashAmount", 1 - t);
                timer += Time.deltaTime;
                yield return null;
            }
            sr.material.SetFloat("_FlashAmount", 0);
            isFlashing = false;
        }
    }

    protected void PlayHurtFlash()
    {
        if (!isFlashing)
            StartCoroutine(HurtFlashCoroutine());
    }

    void OnDestroy()
    {
        ClearAllDots();
    }














    protected void FollowTarget(Transform target)
    {
        rb.linearVelocity = Vector2.zero;
        // Add a random offset of about 2 units to the target position
        Vector2 randomOffset = (Vector2)UnityEngine.Random.insideUnitCircle.normalized * 2f;
        Vector2 destination = (Vector2)target.position + randomOffset;
        transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    public override bool TakeDamage(float _amount, PawnMaster reciever, GameObject instigator, GameEvents.DamageType damage_type_, Transform location, float _hit_back_factor, Gun source = null)
    {
        GameObject dealer = instigator != null ? instigator : null;
        if (dealer != null)
        {
            if (lastDamageTimes.TryGetValue(dealer, out float lastTime))
            {
                if (Time.time - lastTime < damageCooldown) return false;
            }
            lastDamageTimes[dealer] = Time.time;
        }

        hitBackFactor = _hit_back_factor;
        curHP -= _amount;

        PlayHurtFlash(); // Add this line to trigger the flash
        if (_hit_back_factor != 0 && instigator != null) HitBack(instigator.transform);
        enemyHealthBar.SetHealth(maxHP, curHP);

        // invoke this game event
        GameEvents.instance.HitEnemy(_amount, this);

        if (curHP <= 0 && is_alive)
        {
            combatManager.HandleEnemyDeath(gameObject);
            moveSpeed = 0f;
            Destroy(gameObject, 0.2f);
            is_alive = false;
            GameEvents.instance?.PawnDie(this, _amount, instigator, damage_type_, source);
        }
        
        isFullHealth = false; // Set to false when taking damage
        return base.TakeDamage(_amount, reciever, instigator, damage_type_, location, _hit_back_factor, source); 
    }

    // called when the actual time of destorying this pawn
    private void DestroyMyself()
    {
        Destroy(gameObject);
    }

    protected bool isBeingHitBack;
    protected void HitBack(Transform _instigator)
    {
        // Ensure Rigidbody2D is set to Dynamic, Gravity Scale = 0, and not Kinematic for AddForce to work
        // Optionally, set Drag to control how quickly the enemy stops after knockback
        if (rb == null || _instigator == null) return;
        rb.linearVelocity = Vector2.zero; // Reset velocity for consistent knockback
        Vector2 direction = (transform.position - _instigator.position).normalized;
        rb.AddForce(direction * hitBackFactor, ForceMode2D.Impulse);

        isBeingHitBack = true;

    }

    protected void HurtPlayer(GameObject _player, float _amount)
    {
        // _player.GetComponent<IBuffable>().TakeDamage(_amount, GameEvents.DamageType.Normal, 0f, gameObject);
        PawnMaster pawnMaster = _player.gameObject.GetComponent<PawnMaster>();
        if (pawnMaster != null && _amount >= 1f) GameEvents.instance.HitPawn(_amount, pawnMaster, gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, 0f, null);

    }

    public void OnMeleeAttackTriggerEnter(Collider2D other)
    {
        if (meleeAttackTags != null && meleeAttackTags.Contains(other.tag))
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(PerformAttack(other));
            }
        }
    }
    private IEnumerator PerformAttack(Collider2D other)
    {
        // Stop movement
        if (rb != null) rb.linearVelocity = Vector2.zero;
        float originalMoveSpeed = moveSpeed;
        moveSpeed = 0f;

        Vector3 spawnDir = (other.transform.position - transform.position).normalized;
        Quaternion spawnRot = Quaternion.LookRotation(Vector3.forward, spawnDir);
        GameObject aoeObj = Instantiate(enemyAOEPrefab, transform.position, spawnRot);
        lastAttackTime = Time.time;
        EnemyAOE enemyAOE = aoeObj.GetComponent<EnemyAOE>();
        if (enemyAOE != null)
        {
            enemyAOE.damage = attackDamage;
            enemyAOE.transform.localScale = Vector3.one * attackScale; // Scale the AOE
            enemyAOE.fillDuration = attackDuration;
        }
        yield return new WaitForSeconds(attackDuration);
        moveSpeed = originalMoveSpeed;
    }


    private float lastAttackTime = 0f;

    // Handle collision-based damage when being hit back
    // This method is called by Unity when this enemy collides with another collider
    void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (meleeAttackTags.Contains(collision.gameObject.tag))
        {
            // Only attack if cooldown has passed
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(PerformAttack(collision.collider));
            }
            return;
        }

        // Only process collision damage if currently being hit back
        // if (!isBeingHitBack) return;

        UnityEngine.GameObject other = collision.gameObject;
        // Check tag match
        bool tagMatch = false;
        if (collidableObjectsTags != null && collidableObjectsTags.Count > 0)
        {
            foreach (var tag in collidableObjectsTags)
            {
                if (!string.IsNullOrEmpty(tag) && other.CompareTag(tag))
                {
                    tagMatch = true;
                    break;
                }
            }
        }
        // Check layer match
        bool layerMatch = false;
        if (collidableObjectsLayers != null && collidableObjectsLayers.Count > 0)
        {
            int otherLayer = other.layer;
            foreach (var layerName in collidableObjectsLayers)
            {
                if (!string.IsNullOrEmpty(layerName) && otherLayer == UnityEngine.LayerMask.NameToLayer(layerName))
                {
                    layerMatch = true;
                    break;
                }
            }
        }

        // Only proceed if tag or layer matches
        if (!tagMatch && !layerMatch) return;

        // Compute the absolute difference between this enemy's velocity and the other's velocity
        // Use linearVelocity instead of velocity (Unity 2022+)
        var otherRb = other.GetComponent<Rigidbody2D>();
        Vector2 myVelocity = rb != null ? rb.linearVelocity : Vector2.zero;
        Vector2 otherVelocity = otherRb != null ? otherRb.linearVelocity : Vector2.zero;
        Vector2 relativeVelocity = myVelocity - otherVelocity;
        float relVel = relativeVelocity.magnitude;

        // Allow designer to scale how velocity translates to damage
        float collisionDamage = relVel * collisionDamageScale;
        // Debug.Log($"[EnemyMaster] Collision damage calculated: {collisionDamage} = {relVel} * {collisionDamageScale} (min required: {minCollisionDamage})");

        if (collisionDamage < minCollisionDamage) return; // Ignore weak collisions

        // Deal collision damage to self, source is the other object
        // Use GameEvents.HitPawn, prefix = "Collision"
        PawnMaster selfPawn = this;
        if (collisionDamage >= 1f) GameEvents.instance.HitPawn(collisionDamage, selfPawn, other, GameEvents.DamageType.Normal, transform, 0f, null, "Collision");

        // --- Spawn debris based on relative velocity ---
        Vector2 collisionNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector2.zero;
        // Debris scatter direction: away from collision (along normal)
        Vector2 scatterDir = -collisionNormal;
        float impactStrength = relVel;
        int particleCount = Mathf.Clamp(Mathf.RoundToInt(impactStrength * 2f), 5, 20);
        float scatterForce = Mathf.Clamp(impactStrength, 1f, 10f);
        DebriManager.ScatterPixels(
            collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position,
            scatterDir,
            particleCount: particleCount,
            scatterForce: scatterForce
        );
    }

    public override bool Heal(float _amount)
    {
        return false; // Enemies cannot be healed
    }


    public enum DotType
    {
        Burn,
        Poison,
        Slow,
        Shock
    }

    // DOT system
    public class DotInfo
    {
        public DotType type;
        public float dotDamage;
        public float dotInterval;
        public float dotDuration;
        public string fxName;
        public bool isStackable;
        public bool useDefault; // Use default behavior if no custom callbacks provided
        public Action<EnemyMaster> onBeginDot;
        public Action<EnemyMaster> onEndDot;
        public Coroutine dotCoroutine;
        public GameObject fxInstance;
        public float startTime;
    }

    
    
    public void AddDot(DotType _type, float _dotDamage, float _dotInterval = 0.5f, float _dotDuration = 2f, string _fxName = null, bool _isStackable = false, Action<EnemyMaster> _onBeginDot = null, Action<EnemyMaster> _onEndDot = null)
    {
        // Only allow one stack per type unless isStackable
        if (!_isStackable && currentDots.Exists(d => d.type == _type)) return;
        DotInfo dot = new DotInfo
        {
            type = _type,
            dotDamage = _dotDamage,
            dotInterval = _dotInterval,
            dotDuration = _dotDuration,
            fxName = _fxName,
            isStackable = _isStackable,
            onBeginDot = _onBeginDot,
            onEndDot = _onEndDot,
            useDefault = _onBeginDot == null && _onEndDot == null, // Use default if no custom callbacks provided
            startTime = Time.time
        };
        dot.dotCoroutine = StartCoroutine(DotCoroutine(dot));
        currentDots.Add(dot);
    }

    private IEnumerator DotCoroutine(DotInfo dot)
    {
        // Begin DOT effect
        dot.onBeginDot?.Invoke(this);
        if (dot.useDefault && dot.type == DotType.Slow)
        {
            // If this is a slow dot, apply slow speed modifier
            moveSpeed *= GameSettings.instance?.slowSpeedModifier ?? 0.5f;
        }

        // Play FX if provided
        if (string.IsNullOrEmpty(dot.fxName)) dot.fxName = GameSettings.GetDotFxName(dot.type);
        dot.fxInstance = CombatManager.PlayFx(dot.fxName, dotSpawnPoint.position, isLooping: true, parent: dotSpawnPoint);
        

        float elapsed = 0f;
        while (elapsed < dot.dotDuration && is_alive)
        {
            // Do dot damage
            if (curHP > 0 && dot.dotDamage >= 1f)
            {
                GameEvents.instance.HitPawn(dot.dotDamage, this, gameObject, GameEvents.DamageType.DotDamage, transform, 0f, null, "DOT");
            }
            yield return new WaitForSeconds(dot.dotInterval);
            elapsed += dot.dotInterval;
        }

        // End of DOT effect
        dot.onEndDot?.Invoke(this);
        if (dot.useDefault && dot.type == DotType.Slow)
        {
            // If this is a slow dot, restore original speed
            moveSpeed /= GameSettings.instance?.slowSpeedModifier ?? 0.5f; // Restore original speed
        }
        // Remove FX
        if (dot.fxInstance != null)
        {
            Destroy(dot.fxInstance);
        }
        currentDots.Remove(dot);
    }

    private void ClearAllDots()
    {
        foreach (var dot in new List<DotInfo>(currentDots))
        {
            if (dot.dotCoroutine != null) StopCoroutine(dot.dotCoroutine);
            if (dot.fxInstance != null) Destroy(dot.fxInstance);
            dot.onEndDot?.Invoke(this);
        }
        currentDots.Clear();
    }
}
