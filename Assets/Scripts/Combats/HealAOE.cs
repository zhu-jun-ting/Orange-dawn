using System.Collections.Generic;
using UnityEngine;

public class HealAOE : ImpulseAOE
{

    // Inherit all settings from ImpulseAOE
    // Override OnTriggerEnter2D to heal instead of damage
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitObjects.Contains(other.gameObject)) return;
        if (targetTags.Contains(other.tag))
        {
            float dist = Vector2.Distance(transform.position, other.transform.position);
            float healAmount = Mathf.Lerp(maxDamage, 0, dist / maxRadius); // Use maxDamage as maxHeal
            var pawn = other.GetComponent<PawnMaster>();
            if (pawn != null)
                if (healAmount >= 1f) GameEvents.instance.HealPawn(healAmount, pawn, gameObject, other.transform);
            hitObjects.Add(other.gameObject);

            if (pawn != null && onPawnDamaged != null)
            {
                float damage = Mathf.Lerp(maxDamage, 0, dist / maxRadius);
                if (damage >= 1f)
                    onPawnDamaged.Invoke(pawn, (int)damage);
            }
        }
    }
}
