using UnityEngine;

public class ItemLightningBox : ItemMaster
{
    [Header("LightningBox Settings")]
    [Tooltip("Prefab of the LightBeam to spawn")] public GameObject lightBeamPrefab;
    [Tooltip("Chance to retrigger a light beam at the end location (0-1)")] public float retriggerChance = 0.3f;
    [Tooltip("Maximum retrigger chain count")] public int maxChain = 2;
    [Tooltip("Tags considered as enemies")] public string[] enemyTags = new string[] { "Enemy" };

    public override void OnHit(Collision2D collision)
    {
        base.OnHit(collision);
        if (lightBeamPrefab != null)
        {
            ShootLightningChain(transform.position, 0);
        }
    }

    private void ShootLightningChain(Vector2 startPos, int chainCount)
    {
        GameObject beam = Instantiate(lightBeamPrefab, startPos, Quaternion.identity);
        LightBeam beamScript = beam.GetComponent<LightBeam>();
        if (beamScript != null)
        {
            beamScript.targetTags = new System.Collections.Generic.List<string>(enemyTags);
            beamScript.useMaxLength = false; // Use actual target position
            // Wait for the beam to fire, then possibly retrigger
            beamScript.StartCoroutine(RetriggerAfterBeam(beamScript, chainCount));
        }
    }

    private System.Collections.IEnumerator RetriggerAfterBeam(LightBeam beamScript, int chainCount)
    {
        // Wait for the beam to fire and deal damage
        yield return new WaitForSeconds(beamScript.duration * 0.9f);
        if (chainCount < maxChain && Random.value < retriggerChance)
        {
            Vector2 end = beamScript.transform.position;
            if (beamScript != null)
            {
                // Try to get the actual beam end position if available
                var endField = beamScript.GetType().GetField("beamEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (endField != null)
                {
                    end = (Vector2)endField.GetValue(beamScript);
                }
            }
            ShootLightningChain(end, chainCount + 1);
        }
    }
}
