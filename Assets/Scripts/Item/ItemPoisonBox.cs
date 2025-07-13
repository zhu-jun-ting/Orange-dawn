using UnityEngine;

public class ItemPoisonBox : ItemMaster
{
    [Header("PoisonBox Settings")]
    [Tooltip("Prefab of the poison ground to spawn on destruction")]
    public GameObject poisonGroundPrefab;
    [Tooltip("Offset from box position to spawn poison ground")]
    public Vector2 spawnOffset = Vector2.zero;
    [Tooltip("Damage dealt to pawns per tick")]
    public float damagePerTick = 5f;
    [Tooltip("Seconds between damage ticks")]
    public float tickInterval = 1f;

    public override void OnItemDestroyed(Collision2D collision)
    {
        base.OnItemDestroyed(collision);
        SpawnPoisonGround();
    }

    private void SpawnPoisonGround()
    {
        if (poisonGroundPrefab != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)spawnOffset;
            var poisonGround = Instantiate(poisonGroundPrefab, spawnPos, Quaternion.identity);
            var poisonScript = poisonGround.GetComponent<ItemPoisonGround>();
            if (poisonScript != null)
            {
                poisonScript.damagePerTick = damagePerTick;
                poisonScript.tickInterval = tickInterval;
            }
        }
    }
}
