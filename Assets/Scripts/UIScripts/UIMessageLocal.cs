using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIMessageLocal : MonoBehaviour
{
    [Header("References")]
    public Image popupImage;
    public TextMeshProUGUI popupText;

    [Header("Effect Settings")]
    public float stayDuration = 0.5f;
    public float fadeDuration = 0.5f;
    public float enlargeScale = 1.3f;
    public Ease enlargeEase = Ease.OutBack;
    public Ease fadeEase = Ease.InSine;

    private Color imageStartColor;
    private Color textStartColor;
    private Vector3 imageStartScale;
    private Tween imageTween;
    private Tween textTween;

    void Awake()
    {
        if (popupImage != null)
            imageStartColor = popupImage.color;
        if (popupText != null)
            textStartColor = popupText.color;
        if (popupImage != null)
            imageStartScale = popupImage.rectTransform.localScale;
    }

    void OnEnable()
    {
        // Start a bit large
        if (popupImage != null)
        {
            popupImage.rectTransform.localScale = imageStartScale * 1.3f;
            popupImage.color = new Color(imageStartColor.r, imageStartColor.g, imageStartColor.b, 0f);
        }
        if (popupText != null)
        {
            popupText.color = new Color(textStartColor.r, textStartColor.g, textStartColor.b, 0f);
        }

        // Entry: shrink to original scale and fade in (0.5s)
        if (popupImage != null)
        {
            imageTween = popupImage.rectTransform.DOScale(imageStartScale, 0.5f).SetEase(Ease.OutBack);
            popupImage.DOFade(imageStartColor.a, 0.5f).SetEase(Ease.OutSine);
        }
        if (popupText != null)
        {
            textTween = popupText.DOFade(textStartColor.a, 0.5f).SetEase(Ease.OutSine);
        }

        // Vibrate for 0.5s after entry
        Invoke(nameof(BeginVibrate), 0.5f);
    }

    public void SetText(string text)
    {
        if (popupText != null)
            popupText.text = text;
    }


    // Set the image sprite (optional, if you want to change it at runtime)
    public void SetImage(Sprite sprite)
    {
        if (popupImage != null)
            popupImage.sprite = sprite;
    }

    // Set the image color at runtime
    public void SetImageColor(Color color)
    {
        if (popupImage != null)
            popupImage.color = color;
    }

    private void BeginVibrate()
    {
        // Vibrate (shake) for 0.5s
        if (popupImage != null)
        {
            popupImage.rectTransform.DOShakePosition(0.5f, strength: 3f, vibrato: 30, randomness: 90, snapping: false, fadeOut: true);
        }
        Invoke(nameof(BeginFadeOut), 0.5f);
    }

    private void BeginFadeOut()
    {
        // Fade out and shrink smaller (0.5s)
        if (popupImage != null)
        {
            imageTween = popupImage.rectTransform.DOScale(imageStartScale * 0.7f, fadeDuration).SetEase(Ease.InBack);
            popupImage.DOFade(0f, fadeDuration).SetEase(fadeEase);
        }
        if (popupText != null)
        {
            textTween = popupText.DOFade(0f, fadeDuration).SetEase(fadeEase);
        }
        // Destroy after fade
        Invoke(nameof(DestroySelf), fadeDuration);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (imageTween != null && imageTween.IsActive()) imageTween.Kill();
        if (textTween != null && textTween.IsActive()) textTween.Kill();
    }
}
