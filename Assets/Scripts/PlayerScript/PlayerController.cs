using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : PawnMaster
{
    private float health;
    public PlayerStat player_stat;
    public GameObject test;
    public int Blinks;
    public float time;
    private Renderer myRender;

    public GameObject[] guns;
    private int gunNum;

    [Header("movement")]
    private Rigidbody2D rb;
    private float moveH, moveV;
    private float dashMoveH, dashMoveV;
    private float moveSpeed;

    private GameObject shadowPrefab;

    [Header("Dashing")]
    private float dashSpeedMultiplier;
    private float dashDuration;

    private float startDashTime;

    [Header("Internal States")]
    private bool isDashing;

    private int frameCount;

    // [Header("Player Stats")]
    private float hit_back_factor;

    public static PlayerController instance;



    // ----------------------- buffs modifiers
    // 1. if player have a AOE under feet
    [Header("buffs indicator")]
    public bool have_fire_aoe = false;
    // these should be default values of the AOE
    public bool use_lifesteal = true;

    // for fire aoe
    private float fire_aoe_range = 5f;
    private float fire_aoe_damage = 5f;
    // for lifesteal
    private float lifesteal_percent = 0f;


    [Header("DO NOT MODIFY")]
    public GameObject fire_aoe;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        //    Debug.Log("hello");
        myRender = GetComponent<Renderer>();
        health = player_stat.max_health;

        HealthBar.HealthCurrent = health;
        HealthBar.HealthMax = health;

        rb = GetComponent<Rigidbody2D>();
        frameCount = 0;

        moveSpeed = player_stat.move_speed;
        dashSpeedMultiplier = player_stat.dash_speed_multiplier;
        dashDuration = player_stat.dash_duration;

        // isntantiate fire aoe prefab and setup parameters 
        UpdateFireAOE();

        // TODO: for test only
        // DOTBuff buff = ScriptableObject.CreateInstance(typeof(DOTBuff)) as DOTBuff;
        // buff.Init(dot_stat);
        // ApplyBuff(buff);

        // register all events handlers
        GameEvents.instance.onHitEnemy += OnHitEnemy;

        // register the instance
        instance = this;
    }

    // use the variables about fire_aoe and update the scale, damage of the AOE
    private void UpdateFireAOE() {
        fire_aoe.transform.GetChild(0).transform.localScale = new Vector3(fire_aoe_range, fire_aoe_range, fire_aoe_range);
        ContiniousAOE fire_aoe_controller = fire_aoe.GetComponentInChildren<ContiniousAOE>();
        fire_aoe_controller.damage = fire_aoe_damage;
        fire_aoe.SetActive(have_fire_aoe);
    }

    // Update is called once per frame
    void Update()
    {
        SwitchGun();
        moveH = Input.GetAxis("Horizontal") * moveSpeed;
        moveV = Input.GetAxis("Vertical") * moveSpeed;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing) {
            ProcessDash();
        }

        // test buff key
        if (Input.GetKeyDown(KeyCode.F)) {
            ContiniousAOEStat s = ScriptableObject.CreateInstance(typeof(ContiniousAOEStat)) as ContiniousAOEStat;
            
            s.additional_aoe_damage_per_tick = 5f;
            s.additional_aoe_range = 5f;
            s.buff_duration = 999f;
            s.buff_icon = null;
            Buff buff = new Buff(s);
            
            ApplyBuff(buff);
        }
    }

    public override void FixedUpdate()
    {

        // basic logic for dashing:
        base.FixedUpdate();
        frameCount++;
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveH, moveV);
        }
        else
        {
            rb.linearVelocity = new Vector2(dashMoveH, dashMoveV);

            // if (frameCount % ShadowPool.instance.framePerShadow == 0) ShadowPool.instance.GetFromPool();

            if (Time.time >= startDashTime + dashDuration)
            {
                isDashing = false;
            }
        }
        Flip();

        // spawn aoe under player's feet
        // fire_aoe.SetActive(have_fire_aoe);
        // fire_aoe.transform.position = transform.GetChild(0).transform.position;

    }

    public override void Damage(float damage, GameEvents.DamageType damage_type_, float hit_back_, Transform instigator_)
    {
        health -= damage;
        HealthBar.HealthCurrent = health;

        if (health <= 0)
        {
            Instantiate(test, gameObject.transform.position, gameObject.transform.rotation);
            gameObject.SetActive(false);
        }
        BlinkPlayer(Blinks, time);

        CombatManager.instance.HandleShowDamageUI((int)damage, this, GameEvents.DamageType.Normal, transform.position);
    }

    void OnDestroy()
    {
        // deregister all events
        GameEvents.instance.onHitEnemy -= OnHitEnemy;
    }

    void SwitchGun()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            guns[gunNum].SetActive(false);
            if (--gunNum < 0)
            {
                gunNum = guns.Length - 1;
            }
            guns[gunNum].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            guns[gunNum].SetActive(false);
            if (++gunNum > guns.Length - 1)
            {
                gunNum = 0;
            }
            guns[gunNum].SetActive(true);
        }
    }

    public override void UpdatePlayerContinuousAOE(ContiniousAOEStat stat)
    {
        if (!have_fire_aoe)
        {
            // if the first time have this AOE, add that to active and set with defualt values
            have_fire_aoe = true;
        }
        else
        {
            // next times, add the additional powerups to this AOE
            fire_aoe_range += stat.additional_aoe_range;
            fire_aoe_damage += stat.additional_aoe_damage_per_tick;
            
        }
        UpdateFireAOE();
    }

    public override void AddLifeStealPercent(float percent_)
    {
        // if (!use_lifesteal) use_lifesteal = true;
        lifesteal_percent += percent_;

        if (lifesteal_percent >= 0.5f) {
            Debug.LogWarning("player lifesteal exceeds 50%");
        }

    }












    private void OnHitEnemy(float damage_, EnemyMaster enemy_)
    {
        // Debug.Log("player hit enemy of damage" + damage_);  
        if (use_lifesteal) {
            // here player can recover from the damage made with a percentage
            GainHealth(lifesteal_percent * damage_);
        } 
    }

    public void GainHealth(float health)
    {
        if (HealthBar.HealthCurrent + health >= HealthBar.HealthMax)
        {
            HealthBar.HealthCurrent = HealthBar.HealthMax;
        }

        else
        {
            HealthBar.HealthCurrent += health;
        }

        CombatManager.instance.HandleShowDamageUI((int)health, this, GameEvents.DamageType.Heal, transform.position);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {


        // if (collision.gameObject.CompareTag("Enemy") == true)
        // {
        //     Damage(5, 0f, transform);
        // }
    }

    void BlinkPlayer(int numBlinks, float seconds)
    {
        if (gameObject.activeSelf == true)
        {
            StartCoroutine(DoBlinks(numBlinks, seconds));
        }
    }

    IEnumerator DoBlinks(int numBlinks, float seconds)
    {
        for (int i = 0; i < numBlinks * 2; i++)
        {
            myRender.enabled = !myRender.enabled;
            yield return new WaitForSeconds(seconds);
        }
        myRender.enabled = true;
    }

    private void ProcessDash()
    {
        isDashing = true;
        startDashTime = Time.time;
        dashMoveH = rb.linearVelocity.x * dashSpeedMultiplier;
        dashMoveV = rb.linearVelocity.y * dashSpeedMultiplier;
        // ShadowPool.instance.SetPrefabAndTransform(shadowPrefab, transform);
    }

    private void Flip()
    {
        if (transform.position.x < Camera.main.ScreenToWorldPoint(Input.mousePosition).x)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (transform.position.x > Camera.main.ScreenToWorldPoint(Input.mousePosition).x)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}
