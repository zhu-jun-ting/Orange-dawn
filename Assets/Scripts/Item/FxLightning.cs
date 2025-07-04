using UnityEngine;

public class FxLightning : MonoBehaviour
{
    public GameObject impulseAOEPrefab; // Assign ImpulseAOE prefab
    public float impulseRange = 2f; // Range of AOE effect
    public float impulseDamage = 10f; // Damage for the AOE effect
    public GameObject dustPrefab; // Assign dust effect prefab
    public Transform aoeSpawnPoint; // Where to spawn AOE
    public Transform dustSpawnPoint; // Where to spawn dust
    public Animator animator;

    public void SpawnAt(Vector3 pos, float range = 2f, float damage = 10f)
    {
        this.impulseRange = range; // Set impulse range if provided
        this.impulseDamage = damage; // Set impulse damage if provided
        if (transform.parent != null)
            transform.parent.position = pos;
        else
            transform.position = pos;
        if (animator != null)
            animator.Play("FxLightning", 0, 0f);
    }

    // Called by animation event when lightning hits ground
    public void OnLightningHitGround()
    {
        if (impulseAOEPrefab != null && aoeSpawnPoint != null)
        {
            var aoe = Instantiate(impulseAOEPrefab, aoeSpawnPoint.position, Quaternion.identity);
            var aoeComponent = aoe.GetComponent<ImpulseAOE>();
            if (aoeComponent != null)
            {
                aoeComponent.maxRadius = impulseRange;
                aoeComponent.maxDamage = impulseDamage;
            }
            CombatManager.PlayFx(aoe, aoeSpawnPoint.position, 1f); 
        }
            
        if (dustPrefab != null && dustSpawnPoint != null)
            CombatManager.PlayFx(dustPrefab, dustSpawnPoint.position, 2f * impulseRange);
    }
}
