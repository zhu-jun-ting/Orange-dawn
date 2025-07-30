using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyShooter : EnemyMaster
{

    [Header("Shooter Stats")]
    public float shoot_range;
    public float shoot_interval;
    public float attackBulletSpeed = 6f; // Speed of the bullet when shooting

    [Header("Game Objects")]
    private ShootRangeDetector shoot_detector;
    public GameObject bullet_prefab;
    


    public enum State {walking, shooting};

    private State state;
    private IEnumerator shoot_timer;

    // Start is called before the first frame update

    public override void Awake()
    {
        base.Awake();
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
        if (!isBeingHitBack) FollowTarget(target);

        
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
            GameObject bullet = ObjectPool.Instance.GetObject(bullet_prefab, transform.position, Quaternion.identity);
            GunBullet gunBullet = bullet.GetComponent<GunBullet>();
            if (gunBullet == null) {
                Debug.LogError("Bullet prefab does not have GunBullet component!");
                yield break;
            }
            gunBullet.trigger_tags = new List<string> { "Player" };
            gunBullet.SetSpeed(target.transform.position - transform.position, attackBulletSpeed);
            gunBullet.att = attackDamage;
            gunBullet.SetOwner(gameObject);
            gunBullet.AddIgnore(transform);
        }
    }

}

