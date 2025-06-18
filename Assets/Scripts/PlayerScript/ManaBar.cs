using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public Text manaText;
    public static int ManaCurrent = 10;
    public static int ManaMax = 10;
    private static float manaRegen = 0.5f; // Backing field for ManaRegen

    public static float ManaRegen
    {
        get { return manaRegen; }
        set { manaRegen = value; }
    }

    private Image healthBar;
    // Start is called before the first frame update
    void Start()
    {
        healthBar = GetComponent<Image>();
        GameEvents.instance.OnUpdateMana += OnUpdateMana;
        //HealthCurrent = HealthMax;
    }

    void OnEnable()
    {
        // GameEvents.instance.OnUpdateMana += OnUpdateMana;
    }

    void OnDisable()
    {
        GameEvents.instance.OnUpdateMana -= OnUpdateMana;  
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = (float)ManaCurrent / (float)ManaMax;
        manaText.text = ManaCurrent.ToString() + "/" + ManaMax.ToString();
    }

    public void OnUpdateMana(int diffMana_, int maxMana_)
    {
        if (maxMana_ > 0) ManaMax = maxMana_;

        ManaCurrent = ManaCurrent + diffMana_;
        if (ManaCurrent < 0) ManaCurrent = 0; // Ensure current mana doesn't go below 0
        if (ManaCurrent > ManaMax) ManaCurrent = ManaMax; // Ensure current mana doesn't exceed max mana
    }

    public static bool CanCostMana(int diffMana_)
    {
        return ManaCurrent + diffMana_ >= 0; // Check if the mana cost can be afforded
    }
    
    private float manaRegenAccumulator = 0f;

    void FixedUpdate()
    {
        // Accumulate mana regeneration over time
        manaRegenAccumulator += ManaRegen * Time.fixedDeltaTime;
        if (manaRegenAccumulator >= 1f)
        {
            int regenAmount = Mathf.FloorToInt(manaRegenAccumulator);
            ManaCurrent = Mathf.Min(ManaCurrent + regenAmount, ManaMax);
            manaRegenAccumulator -= regenAmount;
        }
    }
}
