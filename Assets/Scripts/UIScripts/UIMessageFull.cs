using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIMessageFull : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    private float fadeDuration;
    private float showDuration;
    private CanvasGroup canvasGroup;
    private CanvasGroup parentCanvasGroup;
    private bool isFadingOut = false;
    private bool parentFadingOut = false;
    private Tween fadeTween;
    private Tween parentFadeTween;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (transform.parent != null)
        {
            parentCanvasGroup = transform.parent.GetComponent<CanvasGroup>();
            if (parentCanvasGroup == null && transform.parent != null)
                parentCanvasGroup = transform.parent.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetText(string text)
    {
        if (messageText == null)
            messageText = GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
            messageText.text = text;
    }

    public void SetDurationAndFade(float duration, float fadeTime)
    {
        showDuration = duration;
        fadeDuration = fadeTime;
        Invoke(nameof(StartFadeOut), showDuration);
    }

    private void StartFadeOut()
    {
        if (isFadingOut) return;
        isFadingOut = true;
        fadeTween = canvasGroup.DOFade(0, fadeDuration).OnComplete(OnFadeOutComplete);

        // Fade parent if only child, or if all siblings are fading
        if (transform.parent != null && parentCanvasGroup != null)
        {
            // Only child logic
            if (transform.parent.childCount == 1)
            {
                parentFadingOut = true;
                parentFadeTween = parentCanvasGroup.DOFade(0, fadeDuration).OnComplete(OnParentFadeOutComplete);
            }
            else
            {
                // All siblings fading logic
                bool allSiblingsFading = true;
                foreach (Transform sibling in transform.parent)
                {
                    if (sibling == this.transform) continue;
                    var siblingMsg = sibling.GetComponent<UIMessageFull>();
                    if (siblingMsg != null && !siblingMsg.isFadingOut)
                    {
                        allSiblingsFading = false;
                        break;
                    }
                }
                if (allSiblingsFading)
                {
                    parentFadingOut = true;
                    parentFadeTween = parentCanvasGroup.DOFade(0, fadeDuration).OnComplete(OnParentFadeOutComplete);
                }
            }
        }
    }

    private void OnFadeOutComplete()
    {
        Destroy(gameObject);
    }

    private void OnParentFadeOutComplete()
    {
        if (transform.parent != null)
            transform.parent.gameObject.SetActive(false);
    }

    void OnTransformParentChanged()
    {
        // If parent is fading out and a new entry is added, stop fade and reset alpha
        if (parentFadingOut && parentCanvasGroup != null)
        {
            if (parentFadeTween != null && parentFadeTween.IsActive())
                parentFadeTween.Kill();
            parentCanvasGroup.alpha = 1f;
            parentFadingOut = false;
        }
    }

    void OnDestroy()
    {
        if (fadeTween != null && fadeTween.IsActive()) fadeTween.Kill();
        if (parentFadeTween != null && parentFadeTween.IsActive()) parentFadeTween.Kill();
    }
}
