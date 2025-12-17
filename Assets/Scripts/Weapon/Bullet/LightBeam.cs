using System.Collections.Generic;
using UnityEngine;

public class LightBeam : MonoBehaviour, IDetectorHandler
{
    [Header("Beam Settings")]
    public bool useMaxLength = true; // Whether to use max length or not
    public float maxLength = 10f;
    public float damage = 10f;
    public float duration = 0.2f;
    public List<string> targetTags; // Tags to damage
    public Detector detector; // Assign in inspector or via script

    [Header("Debuff Settings")]
    public List<EnemyMaster.DotType> appliedDebuffs = new List<EnemyMaster.DotType>(); // Debuffs to apply on hit
    public float debuffApplyChance = 100f; // Probability (0-100) to apply debuff
    public float debuffDamage = 3f; // Damage per debuff tick
    public float debuffInterval = 0.5f; // Interval between debuff ticks
    public float debuffDuration = 0.5f; // Duration of debuff

    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    private List<GameObject> detectedTargets = new List<GameObject>();
    private Vector2 beamStart;
    private Vector2 beamEnd;
    private bool hasDealtDamage = false;
    private bool beamFired = false;

    void Start()
    {
        beamStart = transform.position;
        if (detector != null)
        {
            detector.collision_handler = this.gameObject;
        }
        // Wait for detector to collect targets, then fire beam in next frame
        Invoke(nameof(FireBeam), 0.02f);
        Destroy(gameObject, duration);
    }

    void FireBeam()
    {
        GameObject target = FindNearestTarget();
        if (useMaxLength)
        {
            if (target != null)
            {
                Vector2 dir = ((Vector2)target.transform.position - beamStart).normalized;
                beamEnd = beamStart + dir * maxLength;
            }
            else
            {
                beamEnd = beamStart + Vector2.right * maxLength;
            }
        }
        else
        {
            if (target != null)
            {
                beamEnd = target.transform.position;
            }
            else
            {
                beamEnd = beamStart;
            }
        }
        DrawBeam();
        DealDamageAlongBeam();
        beamFired = true;
    }

    GameObject FindNearestTarget()
    {
        float minDist = float.MaxValue;
        GameObject nearest = null;
        foreach (var go in detectedTargets)
        {
            if (go == null) continue;
            float dist = Vector2.Distance(transform.position, go.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = go;
            }
        }
        return nearest;
    }

    void DrawBeam()
    {
        // Use the new FX segmented line method for lightning beam
        float width = 1f; // You can expose this as a field if needed
        float maxRatio = 4f; // You can expose this as a field if needed
        CombatManager.PlayFxLine("FxLightning", beamStart, beamEnd, width, maxRatio, duration);
    }

    void DealDamageAlongBeam()
    {
        if (hasDealtDamage) return;
        float rayLength = useMaxLength ? maxLength : Vector2.Distance(beamStart, beamEnd);
        if (rayLength > 0.01f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(beamStart, (beamEnd - beamStart).normalized, rayLength);
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                GameObject go = hit.collider.gameObject;
                if (hitObjects.Contains(go)) continue;
                if (targetTags.Contains(go.tag))
                {
                    PawnMaster pawn = go.GetComponent<PawnMaster>();
                    if (pawn != null)
                    {
                        if (damage >= 1f) GameEvents.instance.HitPawn(damage, pawn, gameObject, GameEvents.DamageType.Normal, go.transform, 0f, null);
                        hitObjects.Add(go);

                        // Apply debuffs to enemies based on probability
                        EnemyMaster enemy = pawn as EnemyMaster;
                        if (enemy != null && appliedDebuffs.Count > 0)
                        {
                            float roll = UnityEngine.Random.Range(0f, 100f);
                            if (roll <= debuffApplyChance)
                            {
                                foreach (var debuffType in appliedDebuffs)
                                {
                                    enemy.AddDot(debuffType, debuffDamage, debuffInterval, debuffDuration);
                                }
                            }
                        }
                    }
                }
            }
        }
        hasDealtDamage = true;
    }

    // IDetectorHandler implementation
    public void HandleOnTriggerEnter2D(int collider_id, GameObject self, GameObject other)
    {
        if (other != null && targetTags.Contains(other.tag) && !detectedTargets.Contains(other))
        {
            detectedTargets.Add(other);
        }
    }

    public void HandleOnTriggerExit2D(int collider_id, GameObject self, GameObject other)
    {
        if (other != null && detectedTargets.Contains(other))
        {
            detectedTargets.Remove(other);
        }
    }
}
