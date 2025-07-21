using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class FloorDoors : MonoBehaviour
{
    [Header("Fade Settings")]
    public bool alwaysActive = false;
    public float fadeDuration = 0.5f;
    public CanvasGroup canvasGroup; // Assign if using UI, else will use SpriteRenderer
    public List<UnityEngine.Tilemaps.TilemapRenderer> tilemapRenderers = new List<UnityEngine.Tilemaps.TilemapRenderer>(); // List of tilemap renderers to fade

    private void Start()
    {
        GameEvents.instance.OnLevelStart += HandleLevelStart;
        GameEvents.instance.OnLevelCleared += HandleLevelCleared;
        gameObject.SetActive(false); // Start inactive
    }

    private void OnDestroy()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelStart -= HandleLevelStart;
            GameEvents.instance.OnLevelCleared -= HandleLevelCleared;
        }
    }

    private void HandleLevelStart(int levelIndex)
    {
        SetActive(true);
    }

    private void HandleLevelCleared()
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (alwaysActive)
        {
            gameObject.SetActive(true);
            return;
        }
        gameObject.SetActive(true); // Always enable for fade
        float targetAlpha = active ? 1f : 0f;
        bool faded = false;
        if (canvasGroup != null)
        {
            faded = true;
            canvasGroup.DOFade(targetAlpha, fadeDuration).OnComplete(() => {
                if (!active) gameObject.SetActive(false);
            });
        }
        if (tilemapRenderers != null && tilemapRenderers.Count > 0)
        {
            faded = true;
            int finished = 0;
            foreach (var rend in tilemapRenderers)
            {
                if (rend != null && rend.material != null)
                {
                    Color startColor = rend.material.color;
                    Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
                    rend.material.DOFade(targetAlpha, fadeDuration).OnComplete(() => {
                        finished++;
                        if (!active && finished == tilemapRenderers.Count)
                            gameObject.SetActive(false);
                    });
                }
            }
        }
        if (!faded)
        {
            // No fade, just set active
            gameObject.SetActive(active);
        }
    }
}
