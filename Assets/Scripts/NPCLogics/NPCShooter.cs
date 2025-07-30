using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;


public class NPCShooter : NPCMaster, IDetectorHandler
{

    [Header("stats")]
    public NPCShooterStat shooter_stat;

    [Header("game objects")]
    private ShootRangeDetector shoot_detector;
    public GameObject bullet_prefab;

    [Header("Charged Bullet")]
    public GameObject bulletWhenCharge;
    public int bulletNumWhenCharge = 5; // Number of bullets to shoot when charged

    private IEnumerator shoot_timer;
    private Transform target;
    private Detector detector;

    public override void Awake()
    {
        base.Awake();

        // moveSpeed = shooter_stat.move_speed;
        // maxHP = shooter_stat.max_health;
        // base.damage = shooter_stat.melee_damage;
        // hurtDuration = shooter_stat.hurtDuration;

        // detectorRange = shooter_stat.shoot_range;
        // attackInterval = shooter_stat.shoot_interval;
        // bullet_prefab = shooter_stat.bullet_prefab;
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

        if (state == State.Attacking)
        {

        }
        else
        {
            // StopCoroutine(shoot_timer);
        }
    }

    public void HandleOnTriggerEnter2D(int collider_id, GameObject collider, GameObject other)
    {
        detector = collider.GetComponent<Detector>();
        if (!detector.IsEmptyWithinCollider())
        {
            target = detector.GetRandomGameObjectInRange().transform;
            ChangeState(State.Attacking);
        }
    }

    public void HandleOnTriggerExit2D(int collider_id, GameObject collider, GameObject other)
    {
        detector = collider.GetComponent<Detector>();
        if (detector.IsEmptyWithinCollider())
        {
            ChangeState(State.Idle);
        }
        else if (target != null && target.Equals(other.transform))
        {
            target = detector.GetRandomGameObjectInRange().transform;
        }
    }


    private float lastStateChangeTime = -1f;
    private const float stateChangeCooldown = 1f;
    public override void ChangeState(State s)
    {
        if (s == state) return;
        if (Time.time - lastStateChangeTime < stateChangeCooldown) return;
        lastStateChangeTime = Time.time;

        base.ChangeState(s);
        if (s == State.Idle)
        {
            target = null;
            if (shoot_timer != null) StopCoroutine(shoot_timer);
        }
        else if (s == State.Attacking)
        {
            if (shoot_timer != null) StopCoroutine(shoot_timer);
            shoot_timer = Shoot(attackInterval);
            StartCoroutine(shoot_timer);
        }
    }

    public void ChangeShootRange(float range)
    {
        shoot_detector.ChangeColliderRadius(range);
    }

    private IEnumerator Shoot(float waitTime)
    {
        while (true)
        {

            // print("WaitAndPrint " + Time.time);
            if (detector.IsEmptyWithinCollider())
            {
                ChangeState(State.Idle);
            }
            else if (state == State.Attacking && target != null)
            {
                GameObject bullet = ObjectPool.Instance.GetObject(bullet_prefab, transform.position, Quaternion.identity);

                bullet.GetComponent<GunBullet>().trigger_tags = triggerTags;
                bullet.GetComponent<GunBullet>().SetSpeed(target.transform.position - transform.position, 3f);
                bullet.GetComponent<GunBullet>().att = damage;
                bullet.GetComponent<GunBullet>().SetOwner(gameObject);
                bullet.GetComponent<GunBullet>().AddIgnore(transform);

            }
            if (target == null)
            {
                var detectorComponent = detector != null ? detector.GetComponent<Detector>() : null;
                var randomObj = detectorComponent != null ? detectorComponent.GetRandomGameObjectInRange() : null;
                target = randomObj != null ? randomObj.transform : null;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }
    
    public override void OnStartCharge()
    {
        base.OnStartCharge();
        ShootArcBullets(bulletNumWhenCharge);
    }

    public override void OnEndCharge()
    {
        base.OnEndCharge();
        ShootArcBullets(bulletNumWhenCharge);
    }

    private void ShootArcBullets(int bulletCount)
    {
        if (bulletWhenCharge == null) return;
        Vector2 shootOrigin = transform.position;
        Vector2 baseDir;
        if (target != null)
        {
            baseDir = ((Vector2)target.position - shootOrigin).normalized;
        }
        else
        {
            float randAngle = Random.Range(0f, 360f);
            baseDir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad)).normalized;
        }
        float arcAngle = 60f; // total arc in degrees
        float startAngle = -arcAngle / 2f;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + arcAngle * ((float)i / (bulletCount - 1));
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
            GameObject bullet = ObjectPool.Instance.GetObject(bulletWhenCharge, shootOrigin, Quaternion.identity);
            var gunBullet = bullet.GetComponent<GunBullet>();
            if (gunBullet != null)
            {
                gunBullet.trigger_tags = triggerTags;
                gunBullet.SetSpeed(dir, 3f);
                gunBullet.att = damage;
                gunBullet.SetOwner(gameObject);
                gunBullet.AddIgnore(transform);
            }
        }
    }
}

