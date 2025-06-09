using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLaser : NPCMaster, IDetectorHandler
{
    [Header("Laser NPC Stats")]
    public NPCLaserStat laser_stat;
    protected float laser_range;
    protected float laser_interval;

    [Header("Laser Gun Objects")]
    public NPCLasergun npcLaserGunPrefab;
    private NPCLasergun npcLaserGunInstance;
    private ShootRangeDetector shoot_detector;

    private IEnumerator laser_timer;
    private Transform target;
    private Detector detector;

    public override void Awake()
    {
        base.Awake();
        moveSpeed = laser_stat.move_speed;
        maxHP = laser_stat.max_health;
        melee_damage = laser_stat.melee_damage;
        hurtDuration = laser_stat.hurtDuration;
        laser_range = laser_stat.laser_range;
        laser_interval = laser_stat.laser_interval;
    }

    public override void Start()
    {
        base.Start();
        // Instantiate and attach the laser gun to this NPC
        if (npcLaserGunPrefab != null && npcLaserGunInstance == null)
        {
            npcLaserGunInstance = Instantiate(npcLaserGunPrefab, transform);
            npcLaserGunInstance.transform.localPosition = Vector3.zero;
        }
    }

    public override void Update()
    {
        base.Update();
        // Aim and fire at the target if in Attacking state
        if (state == State.Attacking && target != null && npcLaserGunInstance != null)
        {
            npcLaserGunInstance.SetTarget(target);
        }
        else if (npcLaserGunInstance != null)
        {
            npcLaserGunInstance.SetTarget(null); // Stop firing if not attacking
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public void HandleOnTriggerEnter2D(int collider_id, GameObject collider, GameObject other)
    {
        detector = collider.GetComponent<Detector>();
        if (detector != null && !detector.IsEmptyWithinCollider())
        {
            target = detector.GetRandomGameObjectInRange().transform;
            ChangeState(State.Attacking);
        }
    }

    public void HandleOnTriggerExit2D(int collider_id, GameObject collider, GameObject other)
    {
        detector = collider.GetComponent<Detector>();
        if (detector != null && detector.IsEmptyWithinCollider())
        {
            ChangeState(State.Idle);
        }
        else if (target != null && target.Equals(other.transform))
        {
            target = detector.GetRandomGameObjectInRange().transform;
        }
    }

    public override void ChangeState(State s)
    {
        if (s == state)
        {
            return;
        }
        base.ChangeState(s);
        if (s == State.Idle)
        {
            target = null;
            if (laser_timer != null) StopCoroutine(laser_timer);
            if (npcLaserGunInstance != null) npcLaserGunInstance.StopFiring();
        }
        else if (s == State.Attacking)
        {
            if (laser_timer != null) StopCoroutine(laser_timer);
            laser_timer = LaserAttackRoutine(laser_interval);
            StartCoroutine(laser_timer);
        }
    }

    public void ChangeLaserRange(float range)
    {
        if (shoot_detector != null)
            shoot_detector.ChangeColliderRadius(range);
    }

    private IEnumerator LaserAttackRoutine(float waitTime)
    {
        while (true)
        {
            if (detector.IsEmptyWithinCollider())
            {
                ChangeState(State.Idle);
            }
            else if (state == State.Attacking && target != null && npcLaserGunInstance != null)
            {
                npcLaserGunInstance.SetTarget(target);
                npcLaserGunInstance.StartFiring();
            }
            if (target == null)
            {
                target = detector.GetComponent<Detector>().GetRandomGameObjectInRange().transform;
            }
            yield return new WaitForSeconds(waitTime);
        }
    }
}
