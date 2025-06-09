using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasergun : Gun
{
    [Header("Laser Settings")]
    public float maxLaserLength = 30f;
    public float damagePerSecond = 10f;
    public float damageInterval = 0.5f; // Exposed interval for damage ticks
    public LayerMask enemyLayer;
    public LineRenderer laserRenderer;
    public GameObject effect;

    private bool isShooting = false;
    private Coroutine laserCoroutine;
    private Dictionary<EnemyMaster, float> enemyHitTimers = new Dictionary<EnemyMaster, float>();

    protected override void Start()
    {
        base.Start();
        if (laserRenderer == null)
            laserRenderer = muzzlePos.GetComponent<LineRenderer>();
        if (effect == null && transform.Find("Effect") != null)
            effect = transform.Find("Effect").gameObject;
        if (laserRenderer != null)
            laserRenderer.enabled = false;
        if (effect != null)
            effect.SetActive(false);
    }

    protected override void Shoot()
    {
        Vector2 direction = (mousePos - new Vector2(transform.position.x, transform.position.y)).normalized;
        transform.right = direction;

        if (Input.GetButtonDown("Fire1"))
        {
            isShooting = true;
            if (laserCoroutine == null)
                laserCoroutine = StartCoroutine(LaserFire(direction));
        }
        if (Input.GetButtonUp("Fire1"))
        {
            isShooting = false;
            if (laserCoroutine != null)
            {
                StopCoroutine(laserCoroutine);
                laserCoroutine = null;
            }
            if (laserRenderer != null)
                laserRenderer.enabled = false;
            if (effect != null)
                effect.SetActive(false);
        }
        animator.SetBool("Shoot", isShooting);
    }

    private IEnumerator LaserFire(Vector2 initialDirection)
    {
        if (laserRenderer != null)
            laserRenderer.enabled = true;
        if (effect != null)
            effect.SetActive(true);
        enemyHitTimers.Clear();
        while (isShooting)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (currentMousePos - (Vector2)transform.position).normalized;
            transform.right = direction;

            RaycastHit2D[] hits = Physics2D.RaycastAll(muzzlePos.position, direction, maxLaserLength, enemyLayer);
            Vector3 endPoint = muzzlePos.position + (Vector3)direction * maxLaserLength;
            HashSet<EnemyMaster> enemiesThisFrame = new HashSet<EnemyMaster>();
            if (hits.Length > 0)
            {
                endPoint = hits[hits.Length - 1].point;
                foreach (var hit in hits)
                {
                    var enemy = hit.collider.GetComponent<EnemyMaster>();
                    if (enemy != null)
                    {
                        enemiesThisFrame.Add(enemy);
                        if (!enemyHitTimers.ContainsKey(enemy))
                            enemyHitTimers[enemy] = 0f;
                        enemyHitTimers[enemy] += Time.deltaTime;
                        if (enemyHitTimers[enemy] >= damageInterval)
                        {
                            enemy.Damage(damagePerSecond * damageInterval, GameEvents.DamageType.Normal, 0f, transform);
                            enemyHitTimers[enemy] = 0f;
                        }
                    }
                }
            }
            // Remove enemies not hit this frame from the timer dictionary
            var keys = new List<EnemyMaster>(enemyHitTimers.Keys);
            foreach (var enemy in keys)
            {
                if (!enemiesThisFrame.Contains(enemy))
                    enemyHitTimers.Remove(enemy);
            }
            if (laserRenderer != null)
            {
                laserRenderer.SetPosition(0, muzzlePos.position);
                laserRenderer.SetPosition(1, endPoint);
            }
            if (effect != null)
            {
                effect.transform.position = endPoint;
                effect.transform.forward = -direction;
            }
            yield return null;
        }
        if (laserRenderer != null)
            laserRenderer.enabled = false;
        if (effect != null)
            effect.SetActive(false);
        enemyHitTimers.Clear();
    }
}
