using System.Collections.Generic;
using UnityEngine;

public class LightBeamStraight : MonoBehaviour
{
    [Header("Beam Settings")]
    public bool horizontal = true;
    public bool vertical = false;
    public float length = 10f;
    public float damage = 10f;
    public float duration = 0.2f;
    public List<string> targetTags;
    public float beamWidth = 1f;
    public float maxRatio = 4f;

    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    private Vector2 origin;

    void Start()
    {
        origin = transform.position;
        if (horizontal)
        {
            ShootBeam(origin, Vector2.right * length);
            ShootBeam(origin, Vector2.left * length);
        }
        if (vertical)
        {
            ShootBeam(origin, Vector2.up * length);
            ShootBeam(origin, Vector2.down * length);
        }
        Destroy(gameObject, duration);
    }

    void ShootBeam(Vector2 start, Vector2 offset)
    {
        Vector2 end = start + offset;
        CombatManager.PlayFxLine("FxLightning", start, end, beamWidth, maxRatio, duration);
        DealDamageAlongBeam(start, end);
    }

    void DealDamageAlongBeam(Vector2 start, Vector2 end)
    {
        float rayLength = Vector2.Distance(start, end);
        if (rayLength > 0.01f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(start, (end - start).normalized, rayLength);
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
                    }
                }
            }
        }
    }
}
