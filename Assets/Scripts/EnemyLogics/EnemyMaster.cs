
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// All using directives must be at the top of the file

public class EnemyMaster : PawnMaster
{
    [Header("Parameters")]
    public EnemyStat stat;
    protected float moveSpeed;
    public Transform target;


    protected float maxHP;
    protected float curHP;
    protected Rigidbody2D rb;
    protected float melee_damage;

    [Header("Hurt Effects")]
    protected SpriteRenderer sr;
    protected float hurtDuration;
    protected Color originalColor; // Store the original color

    [Header("Game Objects")]
    protected GameObject explosionEffect;
    public GameObject health_bar;


    [Header("Collidable Objects That Can Hurt Enemy")]
    // This can be used to specify which objects can hurt the enemy, e.g., player attacks, traps, etc.
    // You can add a list or array here if needed, or use tags/layers to filter collisions.
    public List<string> collidableObjectsTags;
    public List<string> collidableObjectsLayers;
    public float minCollisionDamage = 2f; // Minimum force to consider a hit as damage
    public float collisionDamageScale = 1f; // You can expose this as a public parameter if needed


    // internal vars
    protected float hitBackFactor;
    protected EnemyHealthBar enemy_health_bar;


    // singletons
    protected CombatManager combat_manager;
    protected GameEvents game_events;
    protected bool is_alive;

    // Per-instigator damage cooldown
    protected Dictionary<GameObject, float> lastDamageTimes = new Dictionary<GameObject, float>();
    protected float damageCooldown = 0.1f;

    protected bool isFlashing = false;


    public virtual void Awake()
    {
        moveSpeed = stat.move_speed;
        maxHP = stat.max_health;
        melee_damage = stat.melee_damage;
        hurtDuration = stat.hurtDuration;
    }


    public override void Start()
    {
        base.Start();
        curHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        enemy_health_bar = health_bar.GetComponent<EnemyHealthBar>();
        originalColor = sr.color; // Store the original color

        // get singleton references
        combat_manager = FindFirstObjectByType<CombatManager>();
        if (combat_manager == null) Debug.LogError("combat manager can not be found.");
        is_alive = true;

        game_events = GameEvents.instance;

    }

    public virtual void Update()
    {
        // If you need per-frame logic, add it here
        if (isBeingHitBack && rb != null)
        {
            if (rb.linearVelocity.magnitude < 0.05f)
            {
                isBeingHitBack = false;
            }
        }
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

    }














    protected void FollowTarget(Transform target)
    {
        // rb.linearVelocity = Vector2.zero;
        // transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
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
        enemy_health_bar.SetHealth(maxHP, curHP);

        // invoke this game event
        GameEvents.instance.HitEnemy(_amount, this);

        if (curHP <= 0 && is_alive)
        {
            combat_manager.HandleEnemyDeath(gameObject);
            moveSpeed = 0f;
            Invoke("DestroyMyself", 1.0f);
            is_alive = false;
        }

        base.TakeDamage(_amount, reciever, instigator, damage_type_, location, _hit_back_factor, source);
        return true; // Return true to indicate damage was taken
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

    // Handle collision-based damage when being hit back
    // This method is called by Unity when this enemy collides with another collider
    void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            HurtPlayer(target.gameObject, 10f); // TODO: update parameter
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

}
