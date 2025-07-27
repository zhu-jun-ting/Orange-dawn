using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : PawnMaster
{


    [Header("Player Stats")]
    public float initial_max_health = 50000f;
    public float initial_move_speed = 3f;
    public float initial_dash_speed_multiplier = 3f;
    public float initial_dash_duration = 0.1f;
    public float initial_dodge = 0.05f;
    public float max_health = 50000f;
    public float moveSpeed = 3f;
    public float dodge = 0.05f;


    [Header("Player Components")]
    public GameObject test;
    private Renderer myRender;
    public List<GameObject> guns;
    public GameObject pistol; // the pistol is the default gun

    [Header("movement")]
    private Rigidbody2D rb;
    private float moveH, moveV;
    private float dashMoveH, dashMoveV;


    [Header("Dashing")]
    private float dashSpeedMultiplier;
    private float dashDuration;
    private float startDashTime;

    [Header("Internal States")]
    private bool isDashing;
    private int frameCount;
    public static PlayerController instance;

    [Header("Animation")]
    public Animator animator;
    private bool isHurt = false;
    private bool isRunning = false;
    private bool isCharging = false;



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
    private Vector2 loggedLocation = Vector2.zero;

    // Track the running music AudioSource
    private AudioSource runningMusicSource = null;
    private bool wasMovingLastFrame = false;

    [Header("DO NOT MODIFY")]
    public GameObject fire_aoe;

    private void Awake()
    {
        // register the instance
        instance = this;
        // Set default animation state to idle
        if (animator != null)
        {
            animator.SetBool("isHurt", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isCharging", false);
        }
        isPlayer = true; // Set this pawn as player
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        myRender = GetComponent<Renderer>();
        max_health = initial_max_health;
        HealthBar.HealthCurrent = max_health;
        HealthBar.HealthMax = max_health;

        rb = GetComponent<Rigidbody2D>();
        frameCount = 0;

        moveSpeed = initial_move_speed;
        dodge = initial_dodge;
        dashSpeedMultiplier = initial_dash_speed_multiplier;
        dashDuration = initial_dash_duration;

        UpdateFireAOE();

        // Register input events
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove += HandleMove;
            InputManager.Instance.OnPause += HandlePause;
            // Add more as needed (e.g., OnFire)
        }

        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnUpdateHealth += HandleOnUpdateHealth;
        }
    }

    public void Reset()
    {
        max_health = initial_max_health;
        moveSpeed = initial_move_speed;
        dodge = initial_dodge;
        dashSpeedMultiplier = initial_dash_speed_multiplier;
        dashDuration = initial_dash_duration;

        UpdateMaxHealth();
    }

    private void OnEnable()
    {

    }

    public override void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove -= HandleMove;
            InputManager.Instance.OnPause -= HandlePause;
        }
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnUpdateHealth -= HandleOnUpdateHealth;
        }
        base.OnDisable();
    }

    public void ActivatePistol(bool isActive) => pistol.SetActive(isActive);

    // use the variables about fire_aoe and update the scale, damage of the AOE
    private void UpdateFireAOE() {
        fire_aoe.transform.GetChild(0).transform.localScale = new Vector3(fire_aoe_range, fire_aoe_range, fire_aoe_range);
        ContiniousAOE fire_aoe_controller = fire_aoe.GetComponentInChildren<ContiniousAOE>();
        fire_aoe_controller.damage = fire_aoe_damage;
        fire_aoe.SetActive(have_fire_aoe);
    }

    private void HandleMove(Vector2 move)
    {
        moveH = move.x * moveSpeed;
        moveV = move.y * moveSpeed;
    }  

    private void HandlePause()
    {
        // Implement pause menu logic here
        Debug.Log("Pause triggered");
    }

    // Update is called once per frame
    public override void Update()
    {
        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            ProcessDash();
        }


        base.Update();
    }

    private void ProcessDash()
    {
        isDashing = true;
        startDashTime = Time.time;
        dashMoveH = rb.linearVelocity.x * dashSpeedMultiplier;
        dashMoveV = rb.linearVelocity.y * dashSpeedMultiplier;
        StartCharging();
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
                StopCharging();
            }
        }

        Flip();

        // Animation: running
        bool moving = Mathf.Abs(moveH) > 0.01f || Mathf.Abs(moveV) > 0.01f;
        if (animator != null)
        {
            if (moving && !isCharging)
            {
                isRunning = true;
                animator.SetBool("isRunning", true);
            }
            else if (!moving && !isCharging)
            {
                isRunning = false;
                animator.SetBool("isRunning", false);
            }
        }
        // Debug.Log("isRunning: " + isRunning + " moveH: " + moveH + " moveV: " + moveV);
    
        // --- Running music logic ---
        if (moving && !wasMovingLastFrame)
        {
            // Start or resume running music
            if (runningMusicSource == null)
            {
                runningMusicSource = SoundManager.PlayMusic("Running", 1f, true);
            }
            else if (!runningMusicSource.isPlaying)
            {
                runningMusicSource.Play();
            }
        }
        else if (!moving && wasMovingLastFrame)
        {
            // Stop running music
            if (runningMusicSource != null)
            {
                SoundManager.StopMusic(runningMusicSource);
                runningMusicSource = null;
            }
        }
        wasMovingLastFrame = moving;

        moveH = 0f;
        moveV = 0f;
    }




    public override bool TakeDamage(float _amount, PawnMaster reciever, GameObject instigator, GameEvents.DamageType damage_type_, Transform location, float _hit_back_factor, Gun source = null)
    {

        // check dodge
        if (UnityEngine.Random.value < dodge)
        {
            GameEvents.instance?.PlayerDodge(transform);
            return false; // Dodge successful, no damage taken
        }

        // Animation: hurt
        if (animator != null)
        {
            isHurt = true;
            animator.SetBool("isHurt", true);
            StartCoroutine(ResetHurtFlag(0.3f)); // Reset after 0.3s (adjust as needed)
        }

        HealthBar.HealthCurrent -= _amount;

        // Actual Death Logic here
        if (HealthBar.HealthCurrent <= 0)
        {
            Instantiate(test, gameObject.transform.position, gameObject.transform.rotation);
            gameObject.SetActive(false);
        }

        
        isFullHealth = false; // Set to false when taking damage
        return base.TakeDamage(_amount, reciever,instigator, damage_type_, location, _hit_back_factor, source); // Return true to indicate damage was taken
    }
    private System.Collections.IEnumerator ResetHurtFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isHurt = false;
        if (animator != null) animator.SetBool("isHurt", false);
    }

    // Call this to start charging animation
    public void StartCharging()
    {
        if (animator != null && !isCharging)
        {
            isCharging = true;
            animator.SetBool("isCharging", true);
        }
    }

    // Call this to stop charging animation
    public void StopCharging()
    {
        if (animator != null && isCharging)
        {
            isCharging = false;
            animator.SetBool("isCharging", false);
        }
    }


    void OnDestroy()
    {
        // deregister all events
        // GameEvents.instance.onHitEnemy -= OnHitEnemy;
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

    public override void UpdatePerSecond()
    {
        base.UpdatePerSecond();
        GameEvents.instance?.PlayerMove(Vector2.Distance(loggedLocation, transform.position));
        loggedLocation = transform.position;
    }








    public void UpdateMaxHealth()
    {
        HealthBar.HealthMax = max_health;
    }

    public void HandleOnUpdateHealth(int diffHealth)
    {
        if (diffHealth == 0) return; // No change in health
        HealthBar.HealthCurrent += diffHealth;
        if (HealthBar.HealthCurrent > HealthBar.HealthMax)
        {
            HealthBar.HealthCurrent = HealthBar.HealthMax; // Clamp to max health
        }
        else if (HealthBar.HealthCurrent < 0)
        {
            HealthBar.HealthCurrent = 0; // Clamp to zero
        }
        GameEvents.instance?.ShowNumberUI(Math.Abs((int)diffHealth), this, GameEvents.DamageType.Normal, (Vector2)transform.position, "Cost");
    }

    private void OnHitEnemy(float damage_, EnemyMaster enemy_)
    {
        // Debug.Log("player hit enemy of damage" + damage_);  
        if (use_lifesteal)
        {
            // here player can recover from the damage made with a percentage
            Heal(lifesteal_percent * damage_);
        }
    }

    public override bool Heal(float _amount)
    {
        if ((int)HealthBar.HealthCurrent == (int)HealthBar.HealthMax) return false; // No healing if already at max health

        if (HealthBar.HealthCurrent + _amount >= HealthBar.HealthMax)
        {
            HealthBar.HealthCurrent = HealthBar.HealthMax;
        }
        else
        {
            HealthBar.HealthCurrent += _amount;
        }

        CombatManager.PlayFx("FxHeal", transform.position, 1f, parent: transform);
        if ((int)HealthBar.HealthCurrent == (int)HealthBar.HealthMax) isFullHealth = true; // Set to true when healing
        return true;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
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




    private void Flip()
    {
        if (Camera.main == null)
            return;

        float mouseWorldX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        if (transform.position.x < mouseWorldX)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (transform.position.x > mouseWorldX)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}
