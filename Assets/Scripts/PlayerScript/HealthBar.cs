using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public TMPro.TextMeshProUGUI healthText;
    public static float HealthCurrent;
    public static float HealthMax;

    public Image healthResponsive; // Assign in inspector: the falling bar image
    public float fallingSpeed = 2f; // Units per second

    private Image healthBar;
    private float responsiveFill = 1f;

    public Transform maxWidth;

    void Start()
    {
        healthBar = GetComponent<Image>();
        responsiveFill = 1f;
    }

    void Update()
    {
        float fillAmount = (HealthMax == 0f) ? 1f : (float)HealthCurrent / (float)HealthMax;
        fillAmount = Mathf.Clamp01(fillAmount);

        // Main health bar instantly matches health
        var rectTransform = healthBar.rectTransform;
        float parentWidth = maxWidth.GetComponent<RectTransform>().rect.width;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillAmount * parentWidth);

        // Responsive bar falls slowly to match health
        if (healthResponsive != null)
        {
            // Lerp the responsive fill down to the current fill
            if (responsiveFill > fillAmount) 
            {
                responsiveFill -= fallingSpeed * Time.deltaTime;
                if (responsiveFill < fillAmount) responsiveFill = fillAmount;
            }
            else
            {
                responsiveFill = fillAmount; // Snap up instantly if healing
            }
            var responsiveRect = healthResponsive.rectTransform;
            responsiveRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, responsiveFill * parentWidth);
        }

        healthText.text = HealthCurrent.ToString() + "/" + HealthMax.ToString();
    }
}
