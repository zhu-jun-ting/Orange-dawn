using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnAreaDensity
{
    public Transform area;
    public float density = 1f; // Higher means more objects
}

[System.Serializable]
public class DecorationPrefabChance
{
    public GameObject prefab;
    public float spawnChance = 1f; // Weight for random selection
}

public class RandomDecorationSpawner : MonoBehaviour
{
    public enum AreaType
    {
        Battleground,
        Uphill
    }
    public AreaType areaType = AreaType.Battleground;
    public bool spawnOnStart = true;
    public float spawnScale = 1f; // Scale factor for spawn density

    [Header("Spawnable Areas and Densities")]
    public List<SpawnAreaDensity> spawnableAreas = new List<SpawnAreaDensity>();

    [Header("Decoration Prefabs and Chances")]
    public List<DecorationPrefabChance> decorationPrefabs = new List<DecorationPrefabChance>();

    [Header("Spawn Settings")]
    public int minObjectsPerArea = 1;
    public int maxObjectsPerArea = 5;

    [Header("Minimum distance between spawned objects")]
    public float minDistance = 1.5f;

    [ContextMenu("Spawn Decorations")]
    public void SpawnDecorations()
    {
        foreach (var areaEntry in spawnableAreas)
        {
            if (areaEntry.area == null) continue;
            int spawnCount = Mathf.RoundToInt(Random.Range(minObjectsPerArea, maxObjectsPerArea + 1) * areaEntry.density);
            List<Vector3> placedPositions = new List<Vector3>();
            int attempts = 0;
            for (int i = 0; i < spawnCount; i++)
            {
                var prefab = GetRandomPrefab();
                if (prefab == null) continue;
                Vector3 spawnPos;
                int maxTries = 20;
                bool found = false;
                for (int tryCount = 0; tryCount < maxTries; tryCount++)
                {
                    spawnPos = GetRandomPointInArea(areaEntry.area);
                    bool tooClose = false;
                    foreach (var pos in placedPositions)
                    {
                        if (Vector3.Distance(spawnPos, pos) < minDistance)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (!tooClose)
                    {
                        placedPositions.Add(spawnPos);
                        var instance = Instantiate(prefab, spawnPos, Quaternion.identity, areaEntry.area);
                        // Flip left/right with 50% chance
                        if (Random.value < 0.5f)
                        {
                            Vector3 localScale = instance.transform.localScale;
                            localScale.x *= -1f;
                            instance.transform.localScale = localScale;
                        }
                        // Randomize scale +-10%
                        float scaleFactor = spawnScale * Random.Range(0.9f, 1.1f);
                        instance.transform.localScale *= scaleFactor;
                        // Apply small color tint
                        var rend = instance.GetComponentInChildren<Renderer>();
                        if (rend != null)
                        {
                            Color baseColor = rend.material.color;
                            float tintStrength = 0.1f;
                            Color tint = new Color(
                                Random.Range(-tintStrength, tintStrength),
                                Random.Range(-tintStrength, tintStrength),
                                Random.Range(-tintStrength, tintStrength),
                                0f);
                            rend.material.color = baseColor + tint;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // If can't find a spot after maxTries, skip this spawn
                    continue;
                }
            }
        }
    }

    private void Start() {
        // Optionally spawn decorations on start
        if (spawnOnStart) SpawnDecorations();
    }

    private GameObject GetRandomPrefab()
    {
        float totalWeight = 0f;
        foreach (var entry in decorationPrefabs)
            totalWeight += entry.spawnChance;
        if (totalWeight <= 0f) return null;
        float roll = Random.value * totalWeight;
        float accum = 0f;
        foreach (var entry in decorationPrefabs)
        {
            accum += entry.spawnChance;
            if (roll <= accum)
                return entry.prefab;
        }
        return decorationPrefabs.Count > 0 ? decorationPrefabs[0].prefab : null;
    }

    private Vector3 GetRandomPointInArea(Transform area)
    {
        // If area has a BoxCollider2D, use bounds
        var box2d = area.GetComponent<BoxCollider2D>();
        if (box2d != null)
        {
            var bounds = box2d.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                area.position.z
            );
        }
        // Otherwise, use area position
        return area.position;
    }
}
