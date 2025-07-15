using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyAOE: Attach to a prefab with a SpriteRenderer and a Collider2D (set as trigger).
/// Fills the sprite over time, then deals damage to objects with specified tags in trigger area, then destroys itself.
/// </summary>
public class EnemyAOE : MonoBehaviour
{
    [Header("AOE Settings")]
    public SpriteRenderer spriteRenderer; // Assign in inspector
    public Collider2D triggerCollider; // Assign in inspector, must be set as trigger
    public float fillDuration = 2f; // Time to fill sprite
    public float damage = 20f;
    public List<string> targetTags = new List<string> { "Player" };
    public Color fillColor = Color.red;
    public Color emptyColor = new Color(1, 1, 1, 0.2f);

    private bool isFilled = false;
    private HashSet<GameObject> targetsInArea = new HashSet<GameObject>();

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;
        spriteRenderer.color = emptyColor;
    }

    void OnEnable()
    {
        StartCoroutine(FillAndExplode());
    }

    private IEnumerator FillAndExplode()
    {
        float timer = 0f;
        Material mat = spriteRenderer.material;
        if (!mat.HasProperty("_FillAmount"))
        {
            Debug.LogWarning("EnemyAOE sprite material missing _FillAmount property. Progressive fill will not work.");
        }
        while (timer < fillDuration)
        {
            float t = timer / fillDuration;
            // Progressive fill: left to right
            if (mat.HasProperty("_FillAmount"))
            {
                mat.SetFloat("_FillAmount", t); // 0 = empty, 1 = full
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (mat.HasProperty("_FillAmount"))
        {
            mat.SetFloat("_FillAmount", 1f);
        }
        spriteRenderer.color = fillColor;
        isFilled = true;
        // Deal damage to all targets in area
        foreach (var go in targetsInArea)
        {
            if (go == null) continue;
            PawnMaster pawn = go.GetComponent<PawnMaster>();
            if (pawn != null)
            {
                GameEvents.instance.HitPawn(damage, pawn, gameObject, GameEvents.DamageType.Aoe, transform, 0f, null);
            }
        }
        // Optionally play explosion effect here
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFilled && targetTags.Contains(other.tag))
        {
            targetsInArea.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isFilled && targetsInArea.Contains(other.gameObject))
        {
            targetsInArea.Remove(other.gameObject);
        }
    }
}
