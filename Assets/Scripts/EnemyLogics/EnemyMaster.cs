using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // internal vars
    protected float hitBackFactor;
    protected EnemyHealthBar enemy_health_bar;


    // singletons
    protected CombatManager combat_manager; 
    protected GameEvents game_events;
    protected bool is_alive;

    protected float lastDamageTime = -1f;
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

    public virtual void Update() {
        // If you need per-frame logic, add it here
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

    public override void TakeDamage(float _amount, GameEvents.DamageType damage_type_, float _hit_back_factor, GameObject instigator, Gun source = null)
    {
        if (Time.time - lastDamageTime < damageCooldown) return; // Prevent double damage in short period
        lastDamageTime = Time.time;

        hitBackFactor = _hit_back_factor;
        curHP -= _amount;

        PlayHurtFlash(); // Add this line to trigger the flash
        if (_hit_back_factor != 0 || instigator != null) HitBack(instigator.transform);
        enemy_health_bar.SetHealth(maxHP, curHP);

        // invoke this game event
        GameEvents.instance.HitEnemy(_amount, this);


        if (curHP <= 0 && is_alive)
        {
            combat_manager.HandleEnemyDeath(gameObject);
            // set a 1 second timer wait it to desotry itself, and let it cannot move
            moveSpeed = 0f;
            Invoke("DestroyMyself", 1.0f);
            // Debug.Log("died");
            is_alive = false;

            // Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // if (gameObject)
        // CombatManager.instance.HandleShowDamageUI((int)_amount, this, damage_type_, transform.position);
        
        base.TakeDamage(_amount, damage_type_, _hit_back_factor, instigator, source);
    }

    // called when the actual time of destorying this pawn
    private void DestroyMyself() {
        Destroy(gameObject);
    }

    protected void HitBack(Transform _instigator)
    {
        // Ensure Rigidbody2D is set to Dynamic, Gravity Scale = 0, and not Kinematic for AddForce to work
        // Optionally, set Drag to control how quickly the enemy stops after knockback
        if (rb == null || _instigator == null) return;
        rb.linearVelocity = Vector2.zero; // Reset velocity for consistent knockback
        Vector2 direction = (transform.position - _instigator.position).normalized;
        rb.AddForce(direction * hitBackFactor, ForceMode2D.Impulse);
        // Debug.Log("hit back with factor: " + direction * hitBackFactor);

    }

    protected void HurtPlayer(GameObject _player, float _amount) {
        _player.GetComponent<IBuffable>().TakeDamage(_amount, GameEvents.DamageType.Normal, 0f, gameObject);
    }

}
