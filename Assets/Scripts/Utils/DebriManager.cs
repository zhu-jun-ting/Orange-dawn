using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager for spawning impact debris/particles for hit effects.
/// </summary>
public class DebriManager : MonoBehaviour
{
    public static DebriManager instance;

    [Header("Debris Particle Settings (Defaults)")]
    public GameObject particlePrefab; // Assign a small prefab with SpriteRenderer and Rigidbody2D
    public float defaultParticleScale = 1f;
    public float defaultGravityScale = 0f;
    public float defaultScatterRadius = 0.1f;
    public float defaultParticleLifetime = 1.5f;
    public int defaultParticleCount = 10;
    public float defaultScatterRandomness = 1.0f;
    public int defaultLayer = 10; // Set to your 'NoCollision' layer index

    [Header("Scatter Force Range")] 
    public float defaultScatterForce = 2.5f;
    public float minScatterForce = 1f;
    public float maxScatterForce = 10f;

    [Header("Default Color Palette (used if color arg is null)")]
    public List<Color> defaultColors = new List<Color> { Color.yellow, Color.red, Color.white };

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    /// <summary>
    /// Spawns a burst of debris particles at a location, scattering them in a direction with force.
    /// </summary>
    public static void ScatterPixels(
        Vector2 location,
        Vector2 scatterDirection,
        int particleCount = -1,
        float scatterForce = -1f,
        float particleScale = -1f,
        float gravityScale = -1f,
        float scatterRadius = -1f,
        float particleLifetime = -1f,
        float scatterRandomness = -1f,
        int layer = -1,
        Color? color = null
    )
    {
        if (instance == null || instance.particlePrefab == null)
        {
            Debug.LogWarning("DebriManager: No instance or particlePrefab assigned.");
            return;
        }
        if (particleCount <= 0) particleCount = instance.defaultParticleCount;
        if (scatterForce < 0f) scatterForce = instance.defaultScatterForce;
        // Clamp scatter force to min/max
        scatterForce = Mathf.Clamp(scatterForce, instance.minScatterForce, instance.maxScatterForce);
        if (particleScale < 0f) particleScale = instance.defaultParticleScale;
        if (gravityScale < 0f) gravityScale = instance.defaultGravityScale;
        if (scatterRadius < 0f) scatterRadius = instance.defaultScatterRadius;
        if (particleLifetime < 0f) particleLifetime = instance.defaultParticleLifetime;
        if (scatterRandomness < 0f) scatterRandomness = instance.defaultScatterRandomness;
        if (layer < 0) layer = instance.defaultLayer;

        for (int i = 0; i < particleCount; i++)
        {
            // Randomize spawn position within scatterRadius
            Vector2 spawnPos = location + Random.insideUnitCircle * scatterRadius;
            GameObject p = Object.Instantiate(instance.particlePrefab, spawnPos, Quaternion.identity);
            p.transform.localScale = Vector3.one * particleScale;
            p.layer = layer;

            // Set color randomization if SpriteRenderer exists
            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color finalColor;
                if (color.HasValue)
                {
                    // If a color is provided, randomize around it (vary hue, sat, value, alpha slightly)
                    Color.RGBToHSV(color.Value, out float baseH, out float baseS, out float baseV);
                    float h = Mathf.Repeat(baseH + Random.Range(-0.05f, 0.05f), 1f);
                    float s = Mathf.Clamp01(baseS + Random.Range(-0.15f, 0.15f));
                    float v = Mathf.Clamp01(baseV + Random.Range(-0.15f, 0.15f));
                    float a = Mathf.Clamp01(color.Value.a + Random.Range(-0.1f, 0.1f));
                    finalColor = Color.HSVToRGB(h, s, v);
                    finalColor.a = a;
                }
                else
                {
                    // Pick a random color from the palette list, then randomize slightly
                    if (instance.defaultColors == null || instance.defaultColors.Count == 0)
                        instance.defaultColors = new List<Color> { Color.white };
                    Color baseColor = instance.defaultColors[Random.Range(0, instance.defaultColors.Count)];
                    // Slight randomization
                    float r = Mathf.Clamp01(baseColor.r + Random.Range(-0.1f, 0.1f));
                    float g = Mathf.Clamp01(baseColor.g + Random.Range(-0.1f, 0.1f));
                    float b = Mathf.Clamp01(baseColor.b + Random.Range(-0.1f, 0.1f));
                    float a = Mathf.Clamp01(baseColor.a + Random.Range(-0.1f, 0.1f));
                    finalColor = new Color(r, g, b, a);
                }
                sr.color = finalColor;
            }

            // Add 2D physics
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb == null) rb = p.AddComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
            rb.linearDamping = 2.5f;
            // Scatter direction with randomness
            Vector2 dir = (scatterDirection.normalized + Random.insideUnitCircle * scatterRandomness).normalized;
            float force = scatterForce + Random.Range(-scatterRandomness, scatterRandomness);
            force = Mathf.Clamp(force, instance.minScatterForce, instance.maxScatterForce);
            rb.linearVelocity = dir * force;

            // Destroy after lifetime
            Object.Destroy(p, particleLifetime + Random.Range(-0.2f, 0.2f));
        }
    }
}
