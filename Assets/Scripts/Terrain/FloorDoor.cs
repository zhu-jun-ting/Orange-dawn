using UnityEngine;
using DG.Tweening;

public class FloorDoor : MonoBehaviour
{
    [Header("Fade Settings")]
    public bool alwaysActive = false;
    public float fadeDuration = 0.5f;
    public UnityEngine.Tilemaps.TilemapRenderer tilemapRenderer; // Single tilemap renderer to fade

    private void Start()
    {
        GameEvents.instance.OnLevelStart += HandleLevelStart;
        GameEvents.instance.OnLevelCleared += HandleLevelCleared;
        if (tilemapRenderer == null)
        {
            tilemapRenderer = GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        }
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

    private void HandleLevelStart()
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
            if (tilemapRenderer != null && tilemapRenderer.material != null)
            {
                Color startColor = tilemapRenderer.material.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
                tilemapRenderer.material.DOFade(1f, fadeDuration);
            }
            return;
        }
        gameObject.SetActive(true); // Always enable for fade
        float targetAlpha = active ? 1f : 0f;
        bool faded = false;
        if (tilemapRenderer != null && tilemapRenderer.material != null)
        {
            faded = true;
            Color startColor = tilemapRenderer.material.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            tilemapRenderer.material.DOFade(targetAlpha, fadeDuration).OnComplete(() => {
                if (!active) gameObject.SetActive(false);
            });
        }
        if (!faded)
        {
            // No fade, just set active
            gameObject.SetActive(active);
        }
    }
}
