using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CardCommon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum LinkType
    {
        Common,
        Value,
        Condition,
        Action,
    }

    [Header("Link Sprites (Order: Common, Value, Condition, Action)")]
    public Sprite commonLinkSprite;
    public Sprite valueLinkSprite;
    public Sprite conditionLinkSprite;
    public Sprite actionLinkSprite;

    [Header("Link GameObjects (Order: Up, Left, Right, Down)")]
    public GameObject upLinkGO;
    public GameObject leftLinkGO;
    public GameObject rightLinkGO;
    public GameObject downLinkGO;

    [Header("Layout for Buff Panel")]
    public GameObject buffPanelLayout;
    public GameObject buffEntryPrefab;

    [Header("Hover Behavior")]
    public float showDelay = 0.1f; // Delay before showing buff entry
    public float fadeInDuration = 0.3f; // Duration for fade-in effect
    public List<Transform> transformsToShowOnHover; // Positions for buff entries

    // private variables for hover behavior
    private bool isHovering = false;
    private Coroutine showSequenceCoroutine;
    private List<CanvasGroup> activeCanvasGroups = new List<CanvasGroup>();


    [Header("Click Behavior")]
    public float clickEnlargeScale = 2f; // Scale factor when clicked
    public float clickEnlargeDuration = 0.25f; // Duration for click enlarge animation
    public float MinInteractionTimeDiff = 0.5f; // Duration for ease effect on click enlarge
    public List<Transform> transformToShowOnSpotlight; // UI elements to show on spotlight
    public Vector2 spotlightScreenPosition = new Vector2(0.5f, 0.5f); // normalized (0-1) screen center by default
    public Transform balckCurtain;

    [Header("Hover Scale Feedback")]
    public float hoverScale = 1.08f; // How much to scale up on hover
    public float hoverScaleDuration = 0.18f; // Duration for scale tween

    // private variables for click behavior
    private bool isSpotlighted = false;
    private bool isTweening = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Canvas rootCanvas;
    private float lastInteractionTime;
    private Tween hoverScaleTween;


    // private variables for card master
    private CardMaster cardMaster;
    
    public bool IsSpotlighted => isSpotlighted;
    public bool CanInteract => !isSpotlighted && !isTweening && Time.time - lastInteractionTime >= MinInteractionTimeDiff;

    void Awake()
    {
        cardMaster = GetComponent<CardMaster>();
        // Find the root canvas for reparenting
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
            while (rootCanvas.transform.parent != null && rootCanvas.transform.parent.GetComponent<Canvas>() != null)
                rootCanvas = rootCanvas.transform.parent.GetComponent<Canvas>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSpotlighted || isTweening || Time.time - lastInteractionTime < MinInteractionTimeDiff)
            return;
        lastInteractionTime = Time.time;
        SpotlightCard();
    }

    private void SpotlightCard()
    {
        if (isSpotlighted) return;
        isSpotlighted = true;
        isTweening = true;
        // Save original state
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Activate and fade in black curtain
        if (balckCurtain != null)
        {
            balckCurtain.gameObject.SetActive(true);
            var cg = balckCurtain.GetComponent<CanvasGroup>();
            if (cg == null) cg = balckCurtain.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
        }

        // Bring to top of UI
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
            transform.SetAsLastSibling();
        }

        // Enlarge with DOTween
        var scaleTween = transform.DOScale(clickEnlargeScale, clickEnlargeDuration).SetEase(Ease.OutBack);

        // Move to center (or param position)
        Vector2 screenPos = new Vector2(Screen.width * spotlightScreenPosition.x, Screen.height * spotlightScreenPosition.y);
        Vector3 worldPos = screenPos;
        var canvas = rootCanvas != null ? rootCanvas : GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out worldPos);
        }
        var moveTween = transform.DOMove(worldPos, clickEnlargeDuration).SetEase(Ease.OutBack);

        // When both tweens complete, set isTweening = false
        int tweensDone = 0;
        TweenCallback onTweenDone = () => { tweensDone++; if (tweensDone == 2) isTweening = false; };
        scaleTween.OnComplete(onTweenDone);
        moveTween.OnComplete(onTweenDone);

        // Show all spotlight transforms
        foreach (var t in transformToShowOnSpotlight)
        {
            if (t != null)
            {
                t.gameObject.SetActive(true);
                var cg = t.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
                else t.gameObject.SetActive(true);
            }
        }

        // Register global click handler
        StartCoroutine(WaitForClickOutside());
    }

    private IEnumerator WaitForClickOutside()
    {
        // Wait for mouse up to avoid immediate reset
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
        while (isSpotlighted)
        {
            if (Input.GetMouseButtonDown(0) && Time.time - lastInteractionTime >= MinInteractionTimeDiff)
            {
                // Check if pointer is over this card
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        transform as RectTransform, Input.mousePosition, rootCanvas != null ? rootCanvas.worldCamera : null))
                {
                    lastInteractionTime = Time.time; // Update last interaction time
                    ResetSpotlight();
                    yield break;
                }
            }
            yield return null;
        }
    }

    private void ResetSpotlight()
    {
        if (!isSpotlighted) return;
        isSpotlighted = false;
        isTweening = true;
        // Fade out and deactivate black curtain
        if (balckCurtain != null)
        {
            var cg = balckCurtain.GetComponent<CanvasGroup>();
            if (cg == null) cg = balckCurtain.gameObject.AddComponent<CanvasGroup>();
            cg.DOFade(0f, 0.2f).SetEase(Ease.InQuad).OnComplete(() => balckCurtain.gameObject.SetActive(false));
        }
        // Restore scale and position
        var scaleTween = transform.DOScale(originalScale, 0.2f).SetEase(Ease.InQuad);
        var moveTween = transform.DOMove(originalPosition, 0.2f).SetEase(Ease.InQuad);
        int tweensDone = 0;
        TweenCallback onTweenDone = () => { tweensDone++; if (tweensDone == 2) isTweening = false; };
        scaleTween.OnComplete(onTweenDone);
        moveTween.OnComplete(onTweenDone);
        // Restore parent and sibling index after a short delay to allow animation
        StartCoroutine(RestoreParentAfterDelay(0.2f));

        // Hide all spotlight transforms
        foreach (var t in transformToShowOnSpotlight)
        {
            if (t != null)
            {
                t.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator RestoreParentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }

    void OnEnable()
    {
        // Optionally, ensure all hover targets are hidden at start
        HideAllHoverTargetsImmediate();
    }


    // Use UI event system for hover

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSpotlighted || isTweening || Time.time - lastInteractionTime < MinInteractionTimeDiff) return; // Ignore hover if spotlighted or tweening
        isHovering = true;
        if (showSequenceCoroutine != null)
            StopCoroutine(showSequenceCoroutine);
        showSequenceCoroutine = StartCoroutine(ShowTransformsSequence());

        // --- DOTween scale up and bring to top ---
        if (hoverScaleTween != null && hoverScaleTween.IsActive()) hoverScaleTween.Kill();
        transform.SetAsLastSibling();
        hoverScaleTween = transform.DOScale(hoverScale, hoverScaleDuration).SetEase(Ease.OutBack);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSpotlighted || isTweening || Time.time - lastInteractionTime < MinInteractionTimeDiff) return; // Ignore hover if spotlighted or tweening
        isHovering = false;
        if (showSequenceCoroutine != null)
            StopCoroutine(showSequenceCoroutine);
        FadeOutAllHoverTargets();

        // --- DOTween scale back to normal ---
        if (hoverScaleTween != null && hoverScaleTween.IsActive()) hoverScaleTween.Kill();
        hoverScaleTween = transform.DOScale(1.0f, hoverScaleDuration).SetEase(Ease.InQuad);
    }

    private IEnumerator ShowTransformsSequence()
    {
        if (isSpotlighted) yield break; // Do not show hover sequence if spotlighted
        activeCanvasGroups.Clear();
        foreach (var t in transformsToShowOnHover)
        {
            if (t == null) continue;
            // If t has a layout group, show its children in sequence
            var layout = t.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (layout != null && t.childCount > 0)
            {
                // Ensure the parent is active so children are visible
                t.gameObject.SetActive(true);
                CanvasGroup cg = t.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;

                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    yield return StartCoroutine(FadeInTransform(child));
                    if (!isHovering || isSpotlighted) yield break;
                    yield return new WaitForSeconds(showDelay);
                }
            }
            else
            {
                yield return StartCoroutine(FadeInTransform(t));
                if (!isHovering || isSpotlighted) yield break;
                yield return new WaitForSeconds(showDelay);
            }
        }
    }

    private IEnumerator FadeInTransform(Transform t)
    {
        if (t == null) yield break;
        CanvasGroup cg = t.GetComponent<CanvasGroup>();
        if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.gameObject.SetActive(true);
        activeCanvasGroups.Add(cg);
        bool finished = false;
        cg.DOFade(1f, fadeInDuration).SetEase(DG.Tweening.Ease.OutQuad).OnComplete(() => finished = true);
        // Wait until fade is done
        while (!finished)
            yield return null;
        cg.alpha = 1f;
    }

    private void FadeOutAllHoverTargets()
    {
        if (isSpotlighted) return; // Do not fade out hover targets if spotlighted
        foreach (var t in transformsToShowOnHover)
        {
            if (t == null) continue;
            var layout = t.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (layout != null && t.childCount > 0)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    FadeOutTransform(child);
                }
            }
            else
            {
                FadeOutTransform(t);
            }
        }
    }

    private void FadeOutTransform(Transform t)
    {
        if (t == null) return;
        CanvasGroup cg = t.GetComponent<CanvasGroup>();
        if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
        cg.DOFade(0f, fadeInDuration).SetEase(DG.Tweening.Ease.InQuad).OnComplete(() => cg.gameObject.SetActive(false));
    }

    private void HideAllHoverTargetsImmediate()
    {
        if (isSpotlighted)
        {
            // While spotlighted, ensure all spotlight transforms are active and visible
            foreach (var t in transformToShowOnSpotlight)
            {
                if (t != null)
                {
                    t.gameObject.SetActive(true);
                    var cg = t.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;
                }
            }
            return;
        }
        foreach (var t in transformsToShowOnHover)
        {
            if (t == null) continue;
            var layout = t.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (layout != null && t.childCount > 0)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    CanvasGroup cg = child.GetComponent<CanvasGroup>();
                    if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.gameObject.SetActive(false);
                }
            }
            else
            {
                CanvasGroup cg = t.GetComponent<CanvasGroup>();
                if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.gameObject.SetActive(false);
            }
        }
    }
    // For UI: If using Unity UI, you may want to use IPointerEnterHandler/IPointerExitHandler instead of OnMouseEnter/OnMouseExit
    // If so, implement those interfaces and call OnMouseEnter/OnMouseExit from the respective methods.

    void Start()
    {
        if (cardMaster == null) return;
        // Assign sprites based on link type for each direction
        SetLinkSprite(upLinkGO, cardMaster.up_link_type);
        SetLinkSprite(leftLinkGO, cardMaster.left_link_type);
        SetLinkSprite(rightLinkGO, cardMaster.right_link_type);
        SetLinkSprite(downLinkGO, cardMaster.down_link_type);
    }

    private void SetLinkSprite(GameObject linkGO, CardMaster.LinkType linkType)
    {
        if (linkGO == null) return;
        var img = linkGO.GetComponent<Image>();
        if (img == null) return;
        switch (linkType)
        {
            case CardMaster.LinkType.Common:
                img.sprite = commonLinkSprite;
                break;
            case CardMaster.LinkType.Value:
                img.sprite = valueLinkSprite;
                break;
            case CardMaster.LinkType.Condition:
                img.sprite = conditionLinkSprite;
                break;
            case CardMaster.LinkType.Action:
                img.sprite = actionLinkSprite;
                break;
        }
    }

    /// <summary>
    /// Adds a buff entry to the buff panel. If a buff with the same name exists, stacks it. Otherwise, instantiates a new entry and arranges by descending order.
    /// </summary>
    public void AddBuffDescription(string buffName, string buffDescription, int order = 0)
    {
        if (buffPanelLayout == null || buffEntryPrefab == null || string.IsNullOrEmpty(buffName)) return;
        
        // Search for existing buff entry by name
        BuffEntry existing = null;
        foreach (Transform child in buffPanelLayout.transform)
        {
            BuffEntry entry = child.GetComponent<BuffEntry>();
            if (entry != null && entry.name == buffName)
            {
                existing = entry;
                break;
            }
        }
        if (existing != null)
        {
            existing.StackBuff();
            return;
        }

        // Instantiate new buff entry
        GameObject newEntryGO = Instantiate(buffEntryPrefab, buffPanelLayout.transform);
        BuffEntry newEntry = newEntryGO.GetComponent<BuffEntry>();
        if (newEntry != null)
        {
            newEntry.name = buffName;
            newEntry.description = buffDescription;
            newEntry.order = order;
            newEntry.SetBuffName(buffName);
            newEntry.SetBuffDescription(buffDescription);
        }

        // Insert in descending order (greatest order at top)
        int insertIndex = 0;
        for (int i = 0; i < buffPanelLayout.transform.childCount; i++)
        {
            Transform child = buffPanelLayout.transform.GetChild(i);
            BuffEntry entry = child.GetComponent<BuffEntry>();
            if (entry != null && entry != newEntry && entry.order > order)
            {
                insertIndex = i + 1;
            }
        }
        newEntryGO.transform.SetSiblingIndex(insertIndex);
    }

}
