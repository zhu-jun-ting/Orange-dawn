using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns pixel particles from a Sprite2D and scatters them outward in a top-down (no gravity) style.
/// Each pixel becomes a particle that moves outward and slows down over time.
/// </summary>
public class SpritePixelScatter2D : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab; // Assign a small square/circle prefab with SpriteRenderer
    public float particleScale = 0.06f;
    public float gravityScale = 0.5f; // Gravity scale for particles, set to 0 for no gravity
    public float scatterForce = 2.5f;
    public float scatterRandomness = 1.5f;
    public float particleLifetime = 1.5f;
    public int pixelStep = 2; // Sample every Nth pixel for performance

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Call this to trigger the scatter effect and destroy the sprite
    public void ScatterToParticles()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || particlePrefab == null)
            return;

        Texture2D tex = spriteRenderer.sprite.texture;
        Rect rect = spriteRenderer.sprite.textureRect;
        Vector2 pivot = spriteRenderer.sprite.pivot;
        float ppu = spriteRenderer.sprite.pixelsPerUnit;

        // Hide the sprite
        spriteRenderer.enabled = false;

        Color spriteTint = spriteRenderer.color;

        // Loop through pixels
        for (int x = 0; x < rect.width; x += pixelStep)
        {
            for (int y = 0; y < rect.height; y += pixelStep)
            {
                Color color = tex.GetPixel((int)rect.x + x, (int)rect.y + y);
                if (color.a < 0.1f) continue; // Skip transparent

                // Calculate world position of pixel
                Vector2 localPos = new Vector2(
                    (x - pivot.x) / ppu,
                    (y - pivot.y) / ppu
                );
                Vector3 worldPos = transform.TransformPoint(localPos);

                // Spawn particle
                GameObject p = ObjectPool.Instance.GetObject(particlePrefab, worldPos, Quaternion.identity);

                p.transform.localScale = Vector3.one * particleScale;
                var sr = p.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Blend pixel color with sprite tint and add random color variation
                    Color tint = spriteTint;
                    Color randomTint = new Color(
                        1f + Random.Range(-0.08f, 0.08f),
                        1f + Random.Range(-0.08f, 0.08f),
                        1f + Random.Range(-0.08f, 0.08f),
                        1f
                    );
                    sr.color = color * tint * randomTint;
                }

                // Add 2D physics (no gravity)
                var rb = p.GetComponent<Rigidbody2D>();
                if (rb == null) rb = p.AddComponent<Rigidbody2D>();
                rb.gravityScale = gravityScale;
                rb.linearDamping = 2.5f; // Slow down over time
                Vector2 scatterDir = Random.insideUnitCircle.normalized;
                float force = scatterForce + Random.Range(-scatterRandomness, scatterRandomness);
                rb.linearVelocity = scatterDir * force;

                // Destroy after lifetime
                Destroy(p, particleLifetime + Random.Range(-0.2f, 0.2f));
            }
        }
    }

    void OnDestroy()
    {
        ScatterToParticles(); // Ensure particles are spawned when the sprite is destroyed
    }
}
