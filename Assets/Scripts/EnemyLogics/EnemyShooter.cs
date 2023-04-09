using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyShooter : EnemyMaster
{

    [Header("stats")]
    public EnemyShooterStat shooter_stat;
    protected float shoot_range;
    protected float shoot_interval;

    [Header("game objects")]
    private ShootRangeDetector shoot_detector;
    protected GameObject bullet_prefab;
    


    public enum State {walking, shooting};

    private State state;
    private IEnumerator shoot_timer;

    // Start is called before the first frame update

    public override void Awake()
    {
        // base.Awake();

        moveSpeed = shooter_stat.move_speed;
        maxHP = shooter_stat.max_health;
        melee_damage = shooter_stat.melee_damage;
        hurtDuration = shooter_stat.hurtDuration;

        shoot_range = shooter_stat.shoot_range;
        shoot_interval = shooter_stat.shoot_interval;
        bullet_prefab = shooter_stat.bullet_prefab;
    }

    public override void Start()
    {
        base.Start();
        shoot_detector = gameObject.GetComponentInChildren<ShootRangeDetector>();
        shoot_detector.target = target;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        FollowTarget(target);

        
    }

    public void ChangeState(State s) {
        if (s == state) {
            return;
        }
        
        state = s;
        // Debug.Log("state changed to:" + s.ToString());
        if (state == State.shooting) {
            shoot_timer = Shoot(shoot_interval);
            StartCoroutine(shoot_timer);
        } else if (state == State.walking) {
            StopCoroutine(shoot_timer);
        }
    }

    public void ChangeShootRange(float range) {
        shoot_detector.ChangeColliderRadius(range);
    }

    private IEnumerator Shoot(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            // print("WaitAndPrint " + Time.time);
            GameObject bullet = Instantiate(bullet_prefab, transform.position, Quaternion.identity);
            bullet.GetComponent<GunBullet>().trigger_tags.Add("Player");
            bullet.GetComponent<GunBullet>().SetSpeed(target.transform.position - transform.position, 3f);
            bullet.GetComponent<GunBullet>().att = 10f; // TODO: update damage
            bullet.GetComponent<GunBullet>().SetOwner(gameObject);
        }
    }

}

