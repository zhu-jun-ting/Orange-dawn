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
        if (popupImage != null)
            popupImage.rectTransform.localScale = imageStartScale;
        if (popupImage != null)
            popupImage.color = imageStartColor;
        if (popupText != null)
            popupText.color = textStartColor;
        Invoke(nameof(BeginFadeOut), stayDuration);
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

    private void BeginFadeOut()
    {
        // Enlarge and fade image
        if (popupImage != null)
        {
            imageTween = popupImage.rectTransform.DOScale(imageStartScale * enlargeScale, fadeDuration).SetEase(enlargeEase);
            imageTween = popupImage.DOFade(0f, fadeDuration).SetEase(fadeEase);
        }
        // Fade text
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
