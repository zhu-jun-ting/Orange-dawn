using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage;
    public float speed;
    public float recon;
    public float interval;
    public int bulletNum = 1; // number of bullets fired per shot (if > 1, it will be a shotgun-like spread)
    public float bulletAngle = 15f; // angle between bullets in shotgun spread
    public float critChance = 0.05f; // critical hit chance
    public float critDamage = 2.0f; // critical hit damage multiplier
    public int penetrate = 0; // number of enemies a bullet can penetrate
    public GameObject bulletPrefab;
    public GameObject shellPrefab;
    protected Transform muzzlePos;
    protected Transform shellPos;
    protected Vector2 mousePos;
    protected Vector2 direction;
    protected float timer;
    protected float flipY;
    protected Animator animator;

    // Store initial values for reset
    private float initialDamage;
    private float initialSpeed;
    private float initialRecon;
    private float initialInterval;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        muzzlePos = transform.Find("Muzzle");
        shellPos = transform.Find("BulletShell");
        flipY = transform.localScale.y;
        // Store initial values
        initialDamage = damage;
        initialSpeed = speed;
        initialRecon = recon;
        initialInterval = interval;
    }

    protected virtual void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < transform.position.x)
            transform.localScale = new Vector3(flipY, -flipY, 1);
        else
            transform.localScale = new Vector3(flipY, flipY, 1);

        Shoot();
    }

    protected virtual void Shoot()
    {
        direction = (mousePos - new Vector2(transform.position.x, transform.position.y)).normalized;
        transform.right = direction;

        if (timer != 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
                timer = 0;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (timer == 0)
            {
                timer = interval;
                Fire();
            }
        }
    }

    protected virtual void Fire()
    {
        if (bulletNum == 1)
        {
            animator.SetTrigger("Shoot");

            // GameObject bullet = Instantiate(bulletPrefab, muzzlePos.position, Quaternion.identity);
            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = muzzlePos.position;
            bullet.GetComponent<GunBullet>().trigger_tags.Add("Enemy");
            bullet.GetComponent<GunBullet>().att = damage;
            bullet.GetComponent<GunBullet>().SetOwner(gameObject);

            float angel = Random.Range(-recon, recon);
            bullet.GetComponent<GunBullet>().SetSpeed(Quaternion.AngleAxis(angel, Vector3.forward) * direction, speed);

            // Instantiate(shellPrefab, shellPos.position, shellPos.rotation);
            GameObject shell = ObjectPool.Instance.GetObject(shellPrefab);
            shell.transform.position = shellPos.position;
            shell.transform.rotation = shellPos.rotation;
        }
        else
        {
            animator.SetTrigger("Shoot");

            int median = bulletNum / 2;
            for (int i = 0; i < bulletNum; i++)
            {
                GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
                bullet.transform.position = muzzlePos.position;
                bullet.GetComponent<GunBullet>().trigger_tags.Add("Enemy");
                bullet.GetComponent<GunBullet>().att = damage;
                bullet.GetComponent<GunBullet>().hit_back = 0.1f;
                bullet.GetComponent<GunBullet>().SetOwner(gameObject);

                if (bulletNum % 2 == 1)
                {
                    bullet.GetComponent<GunBullet>().SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median), Vector3.forward) * direction,speed);
                }
                else
                {
                    bullet.GetComponent<GunBullet>().SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median) + bulletAngle / 2, Vector3.forward) * direction,speed);
                }
            }

            GameObject shell = ObjectPool.Instance.GetObject(shellPrefab);
            shell.transform.position = shellPos.position;
            shell.transform.rotation = shellPos.rotation;
        }

    }

    // Resets all gun stats to their initial values
    public virtual void Reset()
    {
        damage = initialDamage;
        speed = initialSpeed;
        recon = initialRecon;
        interval = initialInterval;
    }
}
