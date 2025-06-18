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
    public LayerMask wallLayer; // New: wall layer for reflection
    public LineRenderer laserRenderer;
    public GameObject effect;
    public int maxReflectionCount = 2; // New: max number of reflections

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

            // Reflection logic
            Vector3 startPos = transform.position;
            Vector3 currentDir = direction;
            float remainingLength = maxLaserLength;
            int reflections = 0;
            List<Vector3> laserPoints = new List<Vector3> { startPos };
            HashSet<EnemyMaster> enemiesThisFrame = new HashSet<EnemyMaster>();

            while (reflections <= maxReflectionCount && remainingLength > 0.01f)
            {
                RaycastHit2D hitWall = Physics2D.Raycast(startPos, currentDir, remainingLength, wallLayer);
                RaycastHit2D[] hitEnemies = Physics2D.RaycastAll(startPos, currentDir, remainingLength, enemyLayer);
                float segmentLength = remainingLength;
                Vector3 endPoint = startPos + (Vector3)currentDir * remainingLength;

                if (hitWall.collider != null)
                {
                    segmentLength = hitWall.distance;
                    endPoint = hitWall.point;
                }

                // Damage all enemies along this segment
                foreach (var hit in hitEnemies)
                {
                    if (hit.distance <= segmentLength)
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
                                enemy.TakeDamage(damagePerSecond * damageInterval, GameEvents.DamageType.Normal, 0f, gameObject);
                                enemyHitTimers[enemy] = 0f;
                            }
                        }
                    }
                }

                laserPoints.Add(endPoint);

                if (hitWall.collider != null && reflections < maxReflectionCount)
                {
                    // Reflect
                    Vector2 inDir = currentDir;
                    Vector2 normal = hitWall.normal;
                    currentDir = Vector2.Reflect(inDir, normal).normalized;
                    startPos = endPoint + (Vector3)currentDir * 0.01f; // Small offset to avoid self-hit
                    remainingLength -= segmentLength;
                    reflections++;
                }
                else
                {
                    // No more reflections or no wall hit
                    break;
                }
            }

            // Remove enemies not hit this frame from the timer dictionary
            var keys = new List<EnemyMaster>(enemyHitTimers.Keys);
            foreach (var enemy in keys)
            {
                if (!enemiesThisFrame.Contains(enemy))
                    enemyHitTimers.Remove(enemy);
            }
            // Update LineRenderer
            if (laserRenderer != null)
            {
                laserRenderer.positionCount = laserPoints.Count;
                for (int i = 0; i < laserPoints.Count; i++)
                    laserRenderer.SetPosition(i, laserPoints[i]);
            }
            if (effect != null)
            {
                effect.transform.position = laserPoints[laserPoints.Count - 1];
                effect.transform.forward = -currentDir;
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
