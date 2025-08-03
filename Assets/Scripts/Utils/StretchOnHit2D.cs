using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Stretches a target transform (and all its children) when hit by a collider with a matching tag.
/// The stretch direction is determined by the incoming collider's direction.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StretchOnHit2D : MonoBehaviour
{
    [Header("Stretch Settings")]
    public Transform targetToStretch; // The transform to stretch (and all its children)
    public List<string> triggerTags = new List<string> { "Bullet", "Player" };
    public float stretchStrength = 0.05f; // How much to stretch (0.2 = 20% stretch)
    public float shrinkStrength = 0.2f;   // How much to shrink in the impact direction
    public float stretchDuration = 0.15f; // How long the stretch lasts
    public float restoreDuration = 0.18f; // How long to restore to original scale
    public Ease stretchEase = Ease.OutQuad;
    public Ease restoreEase = Ease.OutBack;
    public bool affectChildren = true;

    private Vector3 originalScale;
    private List<Transform> allTargets = new List<Transform>();
    private Dictionary<Transform, Tween> activeTweens = new Dictionary<Transform, Tween>();
    // Store original scale and position for each child
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        if (targetToStretch == null) targetToStretch = transform;
        originalScale = targetToStretch.localScale;
        allTargets.Clear();
        originalScales.Clear();
        if (affectChildren)
            allTargets.AddRange(targetToStretch.GetComponentsInChildren<Transform>(true));
        else
            allTargets.Add(targetToStretch);
        // Store original scale and position for each target
        foreach (var t in allTargets)
        {
            if (!originalScales.ContainsKey(t))
                originalScales[t] = t.localScale;
            if (!originalPositions.ContainsKey(t))
                originalPositions[t] = t.localPosition;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggerTags == null || triggerTags.Count == 0 || !triggerTags.Contains(collision.collider.tag)) return;
        Vector2 hitDir = (collision.transform.position - transform.position).normalized;
        StretchByDirection(hitDir);
    }

    private void StretchByDirection(Vector2 hitDir)
    {
        // Determine main axis of impact
        float absX = Mathf.Abs(hitDir.x);
        float absY = Mathf.Abs(hitDir.y);

        if (targetToStretch == null) targetToStretch = transform;
        originalScale = targetToStretch.localScale;
        allTargets.Clear();
        originalScales.Clear();
        if (affectChildren)
            allTargets.AddRange(targetToStretch.GetComponentsInChildren<Transform>(true));
        else
            allTargets.Add(targetToStretch);
        // Store original scale and position for each target
        foreach (var t in allTargets)
        {
            if (!originalScales.ContainsKey(t))
                originalScales[t] = t.localScale;
            if (!originalPositions.ContainsKey(t))
                originalPositions[t] = t.localPosition;
        }

        // --- Reset all targets to original scale and position before stretching ---
        foreach (var t in allTargets)
        {
            if (activeTweens.TryGetValue(t, out Tween tween) && tween.IsActive())
                tween.Kill();
            if (originalScales.ContainsKey(t))
                t.localScale = originalScales[t];
            if (originalPositions.ContainsKey(t))
                t.localPosition = originalPositions[t];
        }

        Vector3 targetScale = originalScale;
        if (absX > absY)
        {
            // Horizontal impact
            if (hitDir.x > 0)
            {
                // Hit from right: shrink X, stretch Y, shift left
                targetScale = new Vector3(1f - shrinkStrength, 1f + stretchStrength, 1f);
                foreach (var t in allTargets)
                {
                    Vector3 offset = new Vector3(-shrinkStrength * 0.5f, 0f, 0f);
                    Vector3 baseScale = originalScales.ContainsKey(t) ? originalScales[t] : originalScale;
                    Vector3 basePos = originalPositions.ContainsKey(t) ? originalPositions[t] : Vector3.zero;
                    t.DOScale(Vector3.Scale(baseScale, targetScale), stretchDuration).SetEase(stretchEase);
                    t.DOLocalMove(basePos + baseScale.x * offset, stretchDuration).SetEase(stretchEase).OnComplete(() =>
                    {
                        t.DOScale(baseScale, restoreDuration).SetEase(restoreEase);
                        t.DOLocalMove(basePos, restoreDuration).SetEase(restoreEase);
                    });
                }
                return;
            }
            else
            {
                // Hit from left: shrink X, stretch Y, shift right
                targetScale = new Vector3(1f - shrinkStrength, 1f + stretchStrength, 1f);
                foreach (var t in allTargets)
                {
                    Vector3 offset = new Vector3(shrinkStrength * 0.5f, 0f, 0f);
                    Vector3 baseScale = originalScales.ContainsKey(t) ? originalScales[t] : originalScale;
                    Vector3 basePos = originalPositions.ContainsKey(t) ? originalPositions[t] : Vector3.zero;
                    t.DOScale(Vector3.Scale(baseScale, targetScale), stretchDuration).SetEase(stretchEase);
                    t.DOLocalMove(basePos + baseScale.x * offset, stretchDuration).SetEase(stretchEase).OnComplete(() =>
                    {
                        t.DOScale(baseScale, restoreDuration).SetEase(restoreEase);
                        t.DOLocalMove(basePos, restoreDuration).SetEase(restoreEase);
                    });
                }
                return;
            }
        }
        else
        {
            // Vertical impact
            if (hitDir.y > 0)
            {
                // Hit from above: shrink Y, stretch X, shift down
                targetScale = new Vector3(1f + stretchStrength, 1f - shrinkStrength, 1f);
                foreach (var t in allTargets)
                {
                    Vector3 offset = new Vector3(0f, -shrinkStrength * 0.5f, 0f);
                    Vector3 baseScale = originalScales.ContainsKey(t) ? originalScales[t] : originalScale;
                    Vector3 basePos = originalPositions.ContainsKey(t) ? originalPositions[t] : Vector3.zero;
                    t.DOScale(Vector3.Scale(baseScale, targetScale), stretchDuration).SetEase(stretchEase);
                    t.DOLocalMove(basePos + baseScale.y * offset, stretchDuration).SetEase(stretchEase).OnComplete(() =>
                    {
                        t.DOScale(baseScale, restoreDuration).SetEase(restoreEase);
                        t.DOLocalMove(basePos, restoreDuration).SetEase(restoreEase);
                    });
                }
                return;
            }
            else
            {
                // Hit from below: shrink Y, stretch X, shift up
                targetScale = new Vector3(1f + stretchStrength, 1f - shrinkStrength, 1f);
                foreach (var t in allTargets)
                {
                    Vector3 offset = new Vector3(0f, shrinkStrength * 0.5f, 0f);
                    Vector3 baseScale = originalScales.ContainsKey(t) ? originalScales[t] : originalScale;
                    Vector3 basePos = originalPositions.ContainsKey(t) ? originalPositions[t] : Vector3.zero;
                    t.DOScale(Vector3.Scale(baseScale, targetScale), stretchDuration).SetEase(stretchEase);
                    t.DOLocalMove(basePos + baseScale.y * offset, stretchDuration).SetEase(stretchEase).OnComplete(() =>
                    {
                        t.DOScale(baseScale, restoreDuration).SetEase(restoreEase);
                        t.DOLocalMove(basePos, restoreDuration).SetEase(restoreEase);
                    });
                }
                return;
            }
        }
        // fallback (should not hit)
        foreach (var t in allTargets)
        {
            Vector3 baseScale = originalScales.ContainsKey(t) ? originalScales[t] : originalScale;
            Vector3 basePos = originalPositions.ContainsKey(t) ? originalPositions[t] : Vector3.zero;
            t.DOScale(baseScale, stretchDuration).SetEase(stretchEase).OnComplete(() =>
            {
                t.DOScale(baseScale, restoreDuration).SetEase(restoreEase);
            });
            t.DOLocalMove(basePos, stretchDuration).SetEase(stretchEase).OnComplete(() =>
            {
                t.DOLocalMove(basePos, restoreDuration).SetEase(restoreEase);
            });
        }
    }
}
