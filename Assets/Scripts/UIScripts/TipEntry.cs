using UnityEngine;
using TMPro;
using DG.Tweening;

public class TipEntry : MonoBehaviour

{
    [SerializeField] private TMP_Text tipName;
    [SerializeField] private TMP_Text tipDescription;
    public string description;
    public string name;

    [Header("Tip Animation")]
    public float fadeDuration = 0.3f;
    public float lifeTime = 5f;
    private CanvasGroup canvasGroup;
    private bool isFadingOut = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        // Fade in
        canvasGroup.DOFade(1f, fadeDuration);
        // Auto-destroy after lifeTime
        Invoke(nameof(FadeOutAndDestroy), lifeTime);
    }

    public void FadeOutAndDestroy()
    {
        if (isFadingOut) return;
        isFadingOut = true;
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        // Cancel invoke if destroyed early
        CancelInvoke();
        if (!isFadingOut && canvasGroup != null && canvasGroup.alpha > 0f)
        {
            canvasGroup.DOFade(0f, fadeDuration);
        }
    }





    // Assigns the tip name text
    public void SetTipName(string newName = null)
    {
        // If newName is not null or empty, update the local name variable
        if (!string.IsNullOrEmpty(newName))
        {
            name = newName;
        }
        // Otherwise, use the existing name value

        if (tipName != null)
        {
            tipName.text = name;
        }
    }

    // Assigns the tip description text
    public void SetTipDescription(string newDescription = null)
    {
        // If newDescription is not null or empty, update the local description variable
        if (!string.IsNullOrEmpty(newDescription))
        {
            description = newDescription;
        }
        // Otherwise, use the existing description value
        if (tipDescription != null)
            tipDescription.text = description;
    }
}

