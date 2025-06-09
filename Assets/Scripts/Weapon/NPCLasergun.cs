using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLasergun : MonoBehaviour
{
    // Expose parameters for the laser gun in the inspector
    [Header("Laser Settings")]
    public float maxLaserLength = 30f;
    public float damagePerSecond = 10f;
    public float damageInterval = 0.5f;
    public LayerMask enemyLayer;
    public LineRenderer laserRenderer;
    public GameObject effect;

    private bool isShooting = false;
    private Coroutine laserCoroutine;
    private Dictionary<EnemyMaster, float> enemyHitTimers = new Dictionary<EnemyMaster, float>();
    private Transform npcTarget;

    public void SetTarget(Transform target)
    {
        npcTarget = target;
        if (npcTarget != null && !isShooting)
            StartFiring();
        if (npcTarget == null)
            StopFiring();
    }

    public void StartFiring()
    {
        if (!isShooting && npcTarget != null)
        {
            isShooting = true;
            if (laserCoroutine == null)
                laserCoroutine = StartCoroutine(NPCLaserFire());
        }
    }

    public void StopFiring()
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

    private IEnumerator NPCLaserFire()
    {
        if (laserRenderer != null)
            laserRenderer.enabled = true;
        if (effect != null)
            effect.SetActive(true);
        enemyHitTimers.Clear();
        while (isShooting && npcTarget != null)
        {
            Vector2 direction = ((Vector2)npcTarget.position - (Vector2)transform.position).normalized;
            transform.right = direction;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxLaserLength, enemyLayer);
            Vector3 endPoint = transform.position + (Vector3)direction * maxLaserLength;
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
            var keys = new List<EnemyMaster>(enemyHitTimers.Keys);
            foreach (var enemy in keys)
            {
                if (!enemiesThisFrame.Contains(enemy))
                    enemyHitTimers.Remove(enemy);
            }
            if (laserRenderer != null)
            {
                laserRenderer.SetPosition(0, transform.position);
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
