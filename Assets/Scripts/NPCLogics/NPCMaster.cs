using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMaster : PawnMaster
{
    [Header("Parameters")]
    // public NPCShooterStat stat;
    protected float moveSpeed;
    // public Transform target;
    public float follow_range; // how far away can NPC stay with player


    protected float maxHP;
    protected float curHP;
    protected Rigidbody2D rb;
    protected float melee_damage;

    [Header("Hurt Effects")]
    protected SpriteRenderer sr;
    protected float hurtDuration;
    protected float hurtCounter;

    [Header("Gam Objects")]
    protected GameObject explosionEffect;
    public GameObject health_bar;

    // internal vars
    protected float hitBackFactor;
    protected EnemyHealthBar enemy_health_bar;
    protected CombatManager combat_manager; 
    protected GameObject player;
    private bool is_moving;
    private Vector2 destination;

    // consts
    private const float random_walk_probability = 0.01f;



    // The enum for NPC states -> 
    //      Idle: no attack job, just walk around player
    //      Attack: find a enemy and try fight with it
    public enum State { Idle, Attacking }
    protected State state;

    private float stuckTimer = 0f;
    private Vector2 lastPosition;

    public virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        is_moving = false;
    }


    public override void Start()
    {
        base.Start();
        curHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        enemy_health_bar = health_bar.GetComponent<EnemyHealthBar>();
        combat_manager = FindFirstObjectByType<CombatManager>();
        if (combat_manager == null) Debug.LogError("combat manager can not be found.");
        state = State.Idle;
        lastPosition = transform.position;
        stuckTimer = 0f;
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
        // State logic: wander if Idle, otherwise do not wander
        if (state == State.Idle)
        {
            if (is_moving)
            {
                FollowTarget(destination);
                // Stuck detection
                if (Vector2.Distance((Vector2)transform.position, lastPosition) < 0.05f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 1f)
                    {
                        // Try a new destination if stuck for over 1 second
                        destination = GetRandomLocationInCircle(player.transform.position, follow_range);
                        stuckTimer = 0f;
                        lastPosition = transform.position;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                    lastPosition = transform.position;
                }
            }
            if ((Vector2.Distance(transform.position, player.transform.position) > follow_range && !is_moving) ||
                (!is_moving && UnityEngine.Random.Range(0f, 1f) < random_walk_probability))
            {
                destination = GetRandomLocationInCircle(player.transform.position, follow_range);
                is_moving = true;
                lastPosition = transform.position;
                stuckTimer = 0f;
            }
            if (Vector2.Distance(transform.position, destination) < .5f)
            {
                is_moving = false;
            }
        }
        // If Attacking, do not wander
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (state == State.Idle) {
            if (is_moving) {
                FollowTarget(destination);
            }
            

            if ( (Vector2.Distance(transform.position, player.transform.position) > follow_range && !is_moving) || 
                (!is_moving && UnityEngine.Random.Range(0f, 1f) < random_walk_probability) ) 
            {
                destination = GetRandomLocationInCircle(player.transform.position, follow_range);
                is_moving = true;
            }

            if (Vector2.Distance(transform.position, destination) < .5f) {
                is_moving = false;
            }
        }
        
        

    }








    protected void FollowTarget(Transform target)
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }

    protected void FollowTarget(Vector2 position)
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
    }

    public override void TakeDamage(float _amount, GameEvents.DamageType damage_type_, float _hit_back_factor, Transform instigator, Gun source = null)
    {
        base.TakeDamage(_amount, damage_type_, _hit_back_factor, instigator);

        hitBackFactor = _hit_back_factor;
        curHP -= _amount;
        HurtFlash();
        if (_hit_back_factor != 0 || instigator != null) HitBack(instigator);
        enemy_health_bar.SetHealth(maxHP, curHP);

        if (curHP <= 0)
        {
            combat_manager.HandleEnemyDeath(gameObject);
            Destroy(gameObject);
            // Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }

    public virtual void ChangeState(NPCMaster.State s) {
        // Debug.Log("NPC state changed from " + state + " to: " + s);
        state = s;
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

    protected void Hurt(GameObject _pawn, float _amount) {
        _pawn.GetComponent<IBuffable>().TakeDamage(_amount, GameEvents.DamageType.Normal, 0f, transform);
    }

    private Vector2 GetRandomLocationInCircle(Vector2 initial_location, float radius) {
        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI*2);
        Vector2 offset = UnityEngine.Random.Range(0f, radius) * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        return initial_location + offset;
    }
}
