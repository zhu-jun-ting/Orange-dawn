using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMaster : PawnMaster
{
    [Header("Parameters")]
    // public NPCShooterStat stat;
    public float moveSpeed;
    // public Transform target;
    public float followRange; // how far away can NPC stay with player
    public float maxHP;
    protected float curHP;
    protected Rigidbody2D rb;
    public float damage;
    public float attackInterval = 2f; // How often NPC attacks
    public float detectorRange = 3f; // Range for the detector to find targets
    public List<string> triggerTags = new List<string> { "Player", "NPC", "Enemy", "Bullet" }; 

    [Header("Common Charge Settings")]
    public float chargeDuration = 2f; // Duration of the charge effect
    public float changeSpeedFactor = 1.5f; // How much to increase speed during charge
    public float changeAttackIntervalFactor = 0.5f; // How much to increase attack interval
    public float minIntervalBetweenCharges = 5f; // Minimum time between charges
    public string chargeEffectName = "FxCharge"; // Effect to play on charge

    [Header("Hurt Effects")]
    protected SpriteRenderer sr;
    public float hurtDuration = 0.3f;
    protected float hurtCounter;

    [Header("Game Objects")]
    protected GameObject explosionEffect;
    public GameObject health_bar;
    [Tooltip("Collider for myself")] public Collider2D rigidbodyCollider;

    // internal vars
    protected float hitBackFactor;
    protected EnemyHealthBar enemy_health_bar;
    protected CombatManager combat_manager; 
    protected GameObject player;
    private bool is_moving;
    private Vector2 destination;

    // consts
    private const float random_walk_probability = 0.2f;



    // The enum for NPC states -> 
    //      Idle: no attack job, just walk around player
    //      Attack: find a enemy and try fight with it
    public enum State { Idle, Attacking }
    protected State state;

    private float stuckTimer = 0f;
    private Vector2 lastPosition;

    // Per-instigator damage cooldown
    protected Dictionary<GameObject, float> lastDamageTimes = new Dictionary<GameObject, float>();
    protected float damageCooldown = 0.1f;

    public virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        is_moving = false;
        isNPC = true;
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
        if (rigidbodyCollider == null)
        {
            rigidbodyCollider = gameObject.GetComponent<Collider2D>();
            if (rigidbodyCollider == null)
            {
                Debug.LogError("NPCMaster: rigidbodyCollider is not set and no Collider2D found on the object.");
            }
        }
        if (GameEvents.instance != null) GameEvents.instance.PawnSpawn(this);
    }


    public override void Update()
    {
        base.Update();
        if (hurtCounter <= 0)
        {
            sr.material.SetFloat("_FlashAmount", 0);
        }
        else
        {
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
                        destination = GetRandomLocationInCircle(player.transform.position, followRange);
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
            if ((Vector2.Distance(transform.position, player.transform.position) > followRange && !is_moving) ||
                (!is_moving && UnityEngine.Random.Range(0f, 1f) < random_walk_probability))
            {
                destination = GetRandomLocationInCircle(player.transform.position, followRange);
                is_moving = true;
                lastPosition = transform.position;
                stuckTimer = 0f;
            }

            if (Vector2.Distance(transform.position, destination) < .5f) { is_moving = false; }
            
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
            
            if (Vector2.Distance(transform.position, destination) < .5f) {
                is_moving = false;
            }
        }
    }

    public override void UpdatePerSecond()
    {
        base.UpdatePerSecond();
        if ((Vector2.Distance(transform.position, player.transform.position) > followRange && !is_moving) ||
                (!is_moving && UnityEngine.Random.Range(0f, 1f) < random_walk_probability))
        {
            destination = GetRandomLocationInCircle(player.transform.position, followRange);
            destination = CombatManager.instance?.TryGetSpawnLocation(player.transform.position, followRange) ?? destination; // Ensure the destination is valid
            is_moving = true;
        }
    }

    /// <summary>
    ///  When NPC is being hit by a pinball, charges NPC to increase its attack interval and some other stats for a short time.
    /// </summary>
    /// <returns></returns>
    private Coroutine chargeCoroutineInstance;

    public virtual bool Charge(int _overrideDuration = -1, bool _ignoreCoolDown = false)
    {
        if (!_ignoreCoolDown && Time.time - lastChargeTime < minIntervalBetweenCharges)
            return false;

        lastChargeTime = Time.time;

        // If ignore cooldown and coroutine is running, finish it early and reset stats
        if (_ignoreCoolDown && chargeCoroutineInstance != null)
        {
            // Reset stats before stopping coroutine
            OnEndCharge();
            StopCoroutine(chargeCoroutineInstance);
            chargeCoroutineInstance = null;
        }

        chargeCoroutineInstance = StartCoroutine(ChargeCoroutine(_overrideDuration));
        GameEvents.instance.NPCCharge(this);
        return true;
    }

    private float lastChargeTime = -Mathf.Infinity;
    float originalMoveSpeed = 0f;
    float originalAttackInterval = 0f;

    private IEnumerator ChargeCoroutine(int _overrideDuration)
    {
        // play the charge effect
        if (!string.IsNullOrEmpty(chargeEffectName))
        {
            CombatManager.PlayFx(chargeEffectName, transform.position, 0.7f, parent: transform);
        }

        originalMoveSpeed = moveSpeed;
        originalAttackInterval = attackInterval;

        OnStartCharge();
        yield return new WaitForSeconds(_overrideDuration > 0 ? _overrideDuration : chargeDuration);
        OnEndCharge();

    }

    /// <summary>
    /// Called when charge starts, can be overridden to change stats. Remember to call base.OnStartCharge() if overriding.
    /// </summary>
    public virtual void OnStartCharge()
    {
        moveSpeed *= changeSpeedFactor;
        attackInterval *= changeAttackIntervalFactor;
    }

    /// <summary>
    /// Called when charge ends, can be overridden to reset stats. Remember to call base.OnEndCharge() if overriding.
    /// </summary>
    public virtual void OnEndCharge()
    {
        moveSpeed = originalMoveSpeed;
        attackInterval = originalAttackInterval;
    }

    
    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Charge();
        }
    }

    
    public override bool Heal(float _amount)
    {
        if (curHP + _amount >= maxHP)
        {
            curHP = maxHP;
        }
        else
        {
            curHP += _amount;
        }

        enemy_health_bar.SetHealth(maxHP, curHP);
        CombatManager.PlayFx("FxHeal", transform.position, 1f, parent: transform);
        if ((int)curHP == (int)maxHP) isFullHealth = true; // Set to true when healing

        // CombatManager.instance.HandleShowDamageUI((int)_amount, this, GameEvents.DamageType.Heal, transform.position);
        return true; // Return true to indicate healing was successful
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
        HurtFlash();
        if (_hit_back_factor != 0 || instigator != null) HitBack(instigator.transform);
        enemy_health_bar.SetHealth(maxHP, curHP);

        if (curHP <= 0)
        {
            GameEvents.instance?.PawnDie(this, _amount, instigator, damage_type_, source);
            combat_manager.HandleEnemyDeath(gameObject);
            Destroy(gameObject, 0.2f);
        }

        
        isFullHealth = false; // Set to false when taking damage
        return base.TakeDamage(_amount, reciever, instigator, damage_type_, location, _hit_back_factor, source);
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

    // protected void Hurt(GameObject _pawn, float _amount) {
    //     _pawn.GetComponent<IBuffable>().TakeDamage(_amount, GameEvents.DamageType.Normal, 0f, gameObject);
    // }

    private Vector2 GetRandomLocationInCircle(Vector2 initial_location, float radius) {
        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI*2);
        Vector2 offset = UnityEngine.Random.Range(0f, radius) * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        return initial_location + offset;
    }
}
