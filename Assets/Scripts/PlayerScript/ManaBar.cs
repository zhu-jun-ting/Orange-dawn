using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public static int manaCurrent = 10;
    public static int manaMax = 10;
    private static float _manaRegen = 0.5f; // Backing field for manaRegen 


    public TMPro.TextMeshProUGUI manaText;

    public Image manaResponsive; // Assign in inspector: the falling bar image
    public float fallingSpeed = 2f; // Units per second

    private Image manaBar;
    private float responsiveFill = 1f;

    public Transform maxWidth;


    public static float manaRegen
    {
        get { return _manaRegen; }
        set { _manaRegen = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        manaBar = GetComponent<Image>();
        GameEvents.instance.OnUpdateMana += OnUpdateMana;
        responsiveFill = 1f;
    }

    void OnEnable()
    {
        // GameEvents.instance.OnUpdatemana += OnUpdatemana;
    }

    void OnDisable()
    {
        GameEvents.instance.OnUpdateMana -= OnUpdateMana;
    }

    // Update is called once per frame
    void Update()
    {
        float fillAmount = (manaMax == 0) ? 1f : (float)manaCurrent / (float)manaMax;
        fillAmount = Mathf.Clamp01(fillAmount);

        // Main mana bar instantly matches mana
        var rectTransform = manaBar.rectTransform;
        float parentWidth = maxWidth.GetComponent<RectTransform>().rect.width;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillAmount * parentWidth);

        // Responsive bar falls slowly to match mana
        if (manaResponsive != null)
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
            var responsiveRect = manaResponsive.rectTransform;
            responsiveRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, responsiveFill * parentWidth);
        }

        manaText.text = manaCurrent.ToString() + "/" + manaMax.ToString();
    }

    public void OnUpdateMana(int diffmana_, int maxmana_)
    {
        if (maxmana_ > 0) manaMax = maxmana_;

        manaCurrent = manaCurrent + diffmana_;
        if (manaCurrent < 0) manaCurrent = 0; // Ensure current mana doesn't go below 0
        if (manaCurrent > manaMax) manaCurrent = manaMax; // Ensure current mana doesn't exceed max mana
    }

    public static bool CanCostMana(int diffmana_)
    {
        return manaCurrent + diffmana_ >= 0; // Check if the mana cost can be afforded
    }
    
    private float manaRegenAccumulator = 0f;

    void FixedUpdate()
    {
        // Accumulate mana regeneration over time
        manaRegenAccumulator += manaRegen * Time.fixedDeltaTime;
        if (manaRegenAccumulator >= 1f)
        {
            int regenAmount = Mathf.FloorToInt(manaRegenAccumulator);
            manaCurrent = Mathf.Min(manaCurrent + regenAmount, manaMax);
            manaRegenAccumulator -= regenAmount;
        }
    }
}
