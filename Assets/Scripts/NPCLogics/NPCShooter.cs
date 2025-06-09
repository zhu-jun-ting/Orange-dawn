using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPCShooter : NPCMaster, IDetectorHandler
{

    [Header("stats")]
    public NPCShooterStat shooter_stat;
    protected float shoot_range;
    protected float shoot_interval;

    [Header("game objects")]
    private ShootRangeDetector shoot_detector;
    protected GameObject bullet_prefab;

    private IEnumerator shoot_timer;
    private Transform target;
    private Detector detector;

    public override void Awake()
    {
        base.Awake();

        moveSpeed = shooter_stat.move_speed;
        maxHP = shooter_stat.max_health;
        melee_damage = shooter_stat.melee_damage;
        hurtDuration = shooter_stat.hurtDuration;

        shoot_range = shooter_stat.shoot_range;
        shoot_interval = shooter_stat.shoot_interval;
        bullet_prefab = shooter_stat.bullet_prefab;
    }

    // Start is called before the first frame update

    public override void Start()
    {
        base.Start();
        // shoot_detector = gameObject.GetComponentInChildren<ShootRangeDetector>();
        // shoot_detector.target = target;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        // FollowTarget(target);

        
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (state == State.Attacking) {
            
        } else {
            // StopCoroutine(shoot_timer);
        }
    }

    public void HandleOnTriggerEnter2D(int collider_id, GameObject collider, GameObject other) {
        detector = collider.GetComponent<Detector>();
        if (!detector.IsEmptyWithinCollider()) {
            target = detector.GetRandomGameObjectInRange().transform;
            ChangeState(State.Attacking);
        }
    }

    public void HandleOnTriggerExit2D(int collider_id, GameObject collider, GameObject other) {
        detector = collider.GetComponent<Detector>();
        if (detector.IsEmptyWithinCollider()) {
            ChangeState(State.Idle);
        } else if (target != null && target.Equals(other.transform)) {
            target = detector.GetRandomGameObjectInRange().transform;
        }
    }

    public override void ChangeState(State s) {
        if (s == state) {
            return;
        }
        base.ChangeState(s);
        if (s == State.Idle) {
            target = null;
            if (shoot_timer != null) StopCoroutine(shoot_timer);
        } else if (s == State.Attacking) {
            if (shoot_timer != null) StopCoroutine(shoot_timer);
            shoot_timer = Shoot(shoot_interval);
            StartCoroutine(shoot_timer);  
        }
    }

    public void ChangeShootRange(float range) {
        shoot_detector.ChangeColliderRadius(range);
    }

    private IEnumerator Shoot(float waitTime) {
        while (true) {
            
            // print("WaitAndPrint " + Time.time);
            if (detector.IsEmptyWithinCollider()) {
                ChangeState(State.Idle);
            } else if (state == State.Attacking && target != null) {
                GameObject bullet = Instantiate(bullet_prefab, transform.position, Quaternion.identity);
                bullet.GetComponent<GunBullet>().trigger_tags.Add("Enemy");
                bullet.GetComponent<GunBullet>().SetSpeed(target.transform.position - transform.position, 3f);
                bullet.GetComponent<GunBullet>().att = 10f; // TODO: update damage
                bullet.GetComponent<GunBullet>().SetOwner(gameObject);
            } 
            if (target == null) {
                target = detector.GetComponent<Detector>().GetRandomGameObjectInRange().transform;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }
}

