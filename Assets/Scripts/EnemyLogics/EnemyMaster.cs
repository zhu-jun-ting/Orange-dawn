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
    protected float hurtCounter;

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
        // target = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        enemy_health_bar = health_bar.GetComponent<EnemyHealthBar>();

        // get singleton references
        combat_manager = FindObjectOfType<CombatManager>();
        if (combat_manager == null) Debug.LogError("combat manager can not be found.");
        is_alive = true;

        game_events = GameEvents.instance;
        
    }


    public virtual void Update()
    {
        if (hurtCounter <= 0)
        {
            sr.material.SetFloat("_FlashAmount", 0);
        } else {
            sr.material.SetFloat("_FlashAmount", hurtCounter / hurtDuration);
            hurtCounter -= Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        
    }














    protected void FollowTarget(Transform target)
    {
        rb.velocity = Vector2.zero;
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }

    public override void Damage(float _amount, GameEvents.DamageType damage_type_, float _hit_back_factor, Transform instigator)
    {
        base.Damage(_amount, damage_type_, _hit_back_factor, instigator);

        hitBackFactor = _hit_back_factor;
        curHP -= _amount;
        HurtFlash();
        if (_hit_back_factor != 0 || instigator != null) HitBack(instigator);
        enemy_health_bar.SetHealth(maxHP, curHP);

        // invoke this game event
        // GameEvents.onHitEnemy += OnHitEnemy();
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
        CombatManager.instance.HandleShowDamageUI((int)_amount, this, damage_type_, transform.position);
    }

    // called when the actual time of destorying this pawn
    private void DestroyMyself() {
        Destroy(gameObject);
    }

    protected void HurtFlash()
    {
        sr.material.SetFloat("_FlashAmount", 1);
        hurtCounter = hurtDuration;
    }

    protected void HitBack(Transform _instigator)
    {
        Vector2 diff = (_instigator.position - transform.position) * hitBackFactor * -1;
        transform.position = new Vector2(transform.position.x + diff.x, transform.position.y + diff.y); 
    }

    protected void HurtPlayer(GameObject _player, float _amount) {
        _player.GetComponent<IBuffable>().Damage(_amount, GameEvents.DamageType.Normal, 0f, transform);
    }

}
