
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 100f;
    public float speed = 10f;
    public float recon = 4f;
    public float interval = 2f;
    public int bulletNum = 1; // number of bullets fired per shot (if > 1, it will be a shotgun-like spread)
    public float bulletAngle = 15f; // angle between bullets in shotgun spread
    public float critChance = 0.05f; // critical hit chance
    public float critDamage = 2.0f; // critical hit damage multiplier
    public int penetrate = 0; // number of enemies a bullet can penetrate
    public float hit_back = 1f; // knockback effect on hit
    public float tempDamage = 0f; // temporary damage, reset after each level cleared
    public float tempSpeed = 0f; // temporary speed, reset after each level cleared
    public GameObject bulletPrefab;
    public GameObject shellPrefab;
    public GameObject owner;
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
    private int initialBulletNum;
    private float initialCritChance;
    private float initialCritDamage;
    private int initialPenetrate;
    private float initialHitBack;


    public System.Action onGunFire;

    private void Awake()
    {
        // Store initial values
        initialDamage = damage;
        initialSpeed = speed;
        initialRecon = recon;
        initialInterval = interval;
        initialBulletNum = bulletNum;
        initialCritChance = critChance;
        initialCritDamage = critDamage;
        initialPenetrate = penetrate;
        initialHitBack = hit_back;

        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelCleared += ResetTemp;
        }
    }

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        muzzlePos = transform.Find("Muzzle");
        shellPos = transform.Find("BulletShell");
        flipY = transform.localScale.y;

        // Reset temporary stats when a level is cleared
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

    public void ResetTemp()
    {
        tempDamage = 0f;
        tempSpeed = 0f;
    }

    public void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelCleared -= ResetTemp;
        }
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

    public virtual void Fire()
    {
        if (bulletNum == 1)
        {
            animator.SetTrigger("Shoot");

            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = muzzlePos.position;
            var gunBullet = bullet.GetComponent<GunBullet>();
            if (gunBullet != null)
            {
                gunBullet.trigger_tags.Add("Enemy");
                gunBullet.att = damage + tempDamage;
                gunBullet.hit_back = hit_back;
                gunBullet.critChance = critChance;
                gunBullet.critDamage = critDamage;
                gunBullet.SetOwner(gameObject);
                gunBullet.gun = this;
                // gunBullet.AddIgnore("Player, NPC"); // Ignore self

                float angel = Random.Range(-recon, recon);
                bullet.GetComponent<GunBullet>().SetSpeed(Quaternion.AngleAxis(angel, Vector3.forward) * direction, speed + tempSpeed);

                // Instantiate(shellPrefab, shellPos.position, shellPos.rotation);
                GameObject shell = ObjectPool.Instance.GetObject(shellPrefab);
                shell.transform.position = shellPos.position;
                shell.transform.rotation = shellPos.rotation;
            }

        }
        else
        {
            animator.SetTrigger("Shoot");

            int median = bulletNum / 2;
            for (int i = 0; i < bulletNum; i++)
            {
                GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
                bullet.transform.position = muzzlePos.position;
                var gunBullet = bullet.GetComponent<GunBullet>();
                if (gunBullet != null)
                {
                    gunBullet.trigger_tags.Add("Enemy");
                    gunBullet.att = damage + tempDamage;
                    gunBullet.hit_back = hit_back;
                    gunBullet.SetOwner(gameObject);
                    gunBullet.gun = this;
                    gunBullet.penetrate = penetrate;
                    gunBullet.critChance = critChance;
                    gunBullet.critDamage = critDamage;
                    // gunBullet.AddIgnore("Player, NPC"); // Ignore self


                    if (bulletNum % 2 == 1)
                    {
                        gunBullet.SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median), Vector3.forward) * direction, speed + tempSpeed);
                    }
                    else
                    {
                        gunBullet.SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median) + bulletAngle / 2, Vector3.forward) * direction, speed + tempSpeed);
                    }
                }
            }

            GameObject shell = ObjectPool.Instance.GetObject(shellPrefab);
            shell.transform.position = shellPos.position;
            shell.transform.rotation = shellPos.rotation;
        }

        onGunFire?.Invoke();

    }

    // Resets all gun stats to their initial values
    public virtual void Reset()
    {
        damage = initialDamage;
        speed = initialSpeed;
        recon = initialRecon;
        interval = initialInterval;
        bulletNum = initialBulletNum;
        critChance = initialCritChance;
        critDamage = initialCritDamage;
        penetrate = initialPenetrate;
        hit_back = initialHitBack;
        // Debug.Log("Gun stats reset to initial values." + $" Damage: {damage}, Speed: {speed}, Recon: {recon}, Interval: {interval}"); 
    }
    
    public void SetBulletSprite(Sprite sprite)
    {
        if (bulletPrefab != null)
        {
            var bulletRenderer = bulletPrefab.GetComponent<SpriteRenderer>();
            if (bulletRenderer != null && sprite != null)
            {
                bulletRenderer.sprite = sprite;
            }
        }
    }
}
