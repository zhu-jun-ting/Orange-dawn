using System.Collections.Generic;
using UnityEngine;

public class ImpulseAOE : MonoBehaviour
{
    [Header("AOE Settings")]
    public float maxRadius = 5f;
    public float duration = 1f;
    public float maxDamage = 20f;
    public List<string> targetTags; // Assign tags in the inspector

    [Header("Visuals")]
    public SpriteRenderer circleSprite; // Assign a SpriteRenderer (circle sprite) as a child in the editor

    private float currentRadius = 0f;
    private float timer = 0f;
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    private CircleCollider2D aoeCollider;

    void Awake()
    {
        aoeCollider = gameObject.AddComponent<CircleCollider2D>();
        aoeCollider.isTrigger = true;
        aoeCollider.radius = 0f;
        if (circleSprite != null)
            circleSprite.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        currentRadius = Mathf.Lerp(0, maxRadius, t);
        aoeCollider.radius = currentRadius;

        // Scale the sprite to match the collider and fade alpha
        if (circleSprite != null)
        {
            float diameter = currentRadius * 2f;
            circleSprite.transform.localScale = new Vector3(diameter, diameter, 1f);
            Color c = circleSprite.color;
            c.a = Mathf.Lerp(1f, 0f, t); // Fade out over time
            circleSprite.color = c;
        }

        if (timer >= duration)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitObjects.Contains(other.gameObject)) return;
        if (targetTags.Contains(other.tag))
        {
            float dist = Vector2.Distance(transform.position, other.transform.position);
            float damage = Mathf.Lerp(maxDamage, 0, dist / maxRadius);
            var pawn = other.GetComponent<PawnMaster>();
            if (pawn != null)
                if (damage >= 1f) GameEvents.instance.HitPawn(damage, pawn, gameObject, GameEvents.DamageType.Normal, other.transform, 0f, null);
            hitObjects.Add(other.gameObject);
        }
    }
}
