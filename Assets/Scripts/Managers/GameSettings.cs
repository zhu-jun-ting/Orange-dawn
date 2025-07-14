using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSettings : MonoBehaviour

{
    public static GameSettings instance;

    [Header("Board Settings")]
    public int boardRows = 3;
    public int boardColumns = 3;
    public float boardMargin = 10f;
    public float cardSizeX = 60f;
    public float cardSizeY = 60f;

    [Header("Card Drag Colors")]
    public Color colorCanPlace = Color.green;
    public Color colorCannotPlace = Color.red;
    public Color colorLinkInactive = new Color(0, 0, 0, 0.5f); // Black, 50% transparent
    public Color colorLinkActive = new Color(0, 1, 0, 1f); // Green, fully opaque

    [Header("Card Icon Settings")]
    public Color damageColor = new Color(1f, 0f, 0f); // Red
    public string damageIcon = "Sword"; // Icon name for damage
    public Color healthColor = new Color(0f, 1f, 0f);
    public string healthIcon = "Heart";
    public Color speedColor = new Color(0f, 1f, 1f);
    public string speedIcon = "Lightning";
    public Color manaColor = new Color(0f, 0.75f, 1f);
    public string manaIcon = "Mana";
    public Color amountColor = new Color(1f, 1f, 0f);
    public string amountIcon = "Cards";
    public Color probabilityColor = new Color(1f, 0.647f, 0f);
    public string probabilityIcon = "Dice";
    public Color timeColor = new Color(1f, 0f, 1f);
    public string timeIcon = "Star";
    public Color coinColor = new Color(1f, 0f, 1f);
    public string coinIcon = "Coin";


    [Header("Card Colors")]
    public Color mechColor = new Color(0.866f, 0.133f, 0.667f); // #DD22AA
    public string mechIcon = "FrameMech";
    public Color skullColor = new Color(1f, 0.867f, 0.933f); // #FFDDEE
    public string skullIcon = "FrameSkull";
    public Color humanColor = new Color(0.866f, 0.667f, 0.133f); // #DDAA22
    public string humanIcon = "FrameHuman";
    public float destroyEffectDuration = 0.5f;

    [Header("Other Icons")]
    public Color starColor = new Color(1f, 0.843f, 0f);
    public string starIcon = "Star"; // Icon for STAR type

    [Header("Card Borders")]
    public Sprite borderDraggable;
    public Sprite borderUndraggable;

    [Header("Card Conditions")]
    public float fragileDestroyChance = 0.25f;

    /// <summary>
    ///  damage: 1, 2, 3 ..... 40, 50, 60, 70, 80, 90, 100
    ///  health: 10, 20, 30 ..... 200, 300
    ///  mana: 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30
    ///  probability: 5%, 10%, 15%, 20%, 25%, 30%, 35%, 40%, 45%, 50%, 60%, 70%
    ///  amount: 1, 2, 3, 4, 5
    ///  coin: 50, 100, 150, .... 700, 800, 900
    /// </summary>
    public List<float> damageGrowth = new List<float> { 1f, 1f, 1f, 2f, 2f, 3f, -1f, -2f, 5f, -4f, 8f, -5f }; // Growth factors for damage
    public List<float> healthGrowth = new List<float> { 2f, 3f, 2f, 1f, 5f, 6f, -2f, -5f, 8f, -8f, 15f, -9f }; // Growth factors for health
    public List<float> speedGrowth = new List<float> { 1f, 1f, 1f, 2f, 2f, 3f, -1f, -2f, 5f, -4f, 8f, -5f }; // Growth factors for speed
    public List<float> manaGrowth = new List<float> { 1f, 1f, 1f, 2f, 2f, 3f, -1f, -2f, 5f, -4f, 8f, -5f }; // Growth factors for mana
    public List<float> probabilityGrowth = new List<float> { 5f, 3f, 2f, 5f, 3f, 1f, -5f, -10f, 6f, -8f, 15f, -10f }; // Growth factors for probability
    public List<float> timeGrowth = new List<float> { 1f, 1f, 1f, 2f, 2f, 3f, -1f, -2f, 5f, -4f, 8f, -5f }; // Growth factors for time
    public List<float> coinGrowth = new List<float> { 10f, 10f, 10f, 20f, 20f, 30f, -10f, -20f, 30f, -40f, 20f, -50f }; // Growth factors for coin

    public List<float> damageDecay = new List<float> { -1f, -1f, -1f, -2f, -2f, -3f, 1f, 2f, -5f, 4f, -8f, 5f }; // Decay factors for damage
    public List<float> healthDecay = new List<float> { -2f, -3f, -2f, -1f, -5f, -6f, 2f, 5f, -8f, 8f, -15f, 9f }; // Decay factors for health
    public List<float> speedDecay = new List<float> { -1f, -1f, -1f, -2f, -2f, -3f, 1f, 2f, -5f, 4f, -8f, 5f }; // Decay factors for speed
    public List<float> manaDecay = new List<float> { -1f, -1f, -1f, -2f, -2f, -3f, 1f, 2f, -5f, 4f, -8f, 5f }; // Decay factors for mana
    public List<float> probabilityDecay = new List<float> { -5f, -3f, -2f, -5f, -3f, -1f, 5f, 10f, -6f, 8f, -15f, 10f }; // Decay factors for probability
    public List<float> timeDecay = new List<float> { -1f, -1f, -1f, -2f, -2f, -3f, 1f, 2f, -5f, 4f, -8f, 5f }; // Decay factors for time
    public List<float> coinDecay = new List<float> { -10f, -10f, -10f, -20f, -20f, -30f, 10f, 20f, -30f, 40f, -20f, 50f }; // Decay factors for coin

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        Debug.Log(AddIcon("Damage: 10 "));
    }



    /// <summary>
    /// Replaces all "Type: XX" patterns in the input string with TMP icon and colored number markup.
    /// Example: "Damage: 10" => "<sprite name=\"Sword\"> <color=#FF0000>10</color>"
    /// </summary>
    /// <param name="raw">The input string to format.</param>
    /// <returns>Formatted string with TMP icons and colored numbers.</returns>
    public static string AddIcon(string raw)
    {
        if (string.IsNullOrEmpty(raw) || instance == null) return raw;
        string result = raw;

        // Define all types and their icon/color fields using the instance
        var types = new (string type, string icon, Color color)[]
        {
            ("Damage", instance.damageIcon, instance.damageColor),
            ("Health", instance.healthIcon, instance.healthColor),
            ("Speed", instance.speedIcon, instance.speedColor),
            ("Mana", instance.manaIcon, instance.manaColor),
            ("Amount", instance.amountIcon, instance.amountColor),
            ("Probability", instance.probabilityIcon, instance.probabilityColor),
            ("Time", instance.timeIcon, instance.timeColor),
            ("Coin", instance.coinIcon, instance.coinColor), // Add Coin as a type for icon replacement
        };

        foreach (var t in types)
        {
            // Regex: Type: XX (where XX is any non-whitespace sequence between two whitespaces)
            string pattern = $@"{t.type}: ([^\s]+)";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, m =>
            {
                string value = m.Groups[1].Value;
                string colorHex = ColorUtility.ToHtmlStringRGB(t.color);
                return $"<sprite name=\"{t.icon}\"> <color=#{colorHex}>{value}</color>";
            });
        }

        // Add CardBond frame icons for all-caps words surrounded by spaces (e.g. " HUMAN ")
        var bondFrames = new (string key, string icon)[]
        {
            ("HUMAN", instance.humanIcon),
            ("MECH", instance.mechIcon),
            ("SKULL", instance.skullIcon),
            ("DAMAGE", instance.damageIcon),
            ("HEALTH", instance.healthIcon),
            ("SPEED", instance.speedIcon),
            ("MANA", instance.manaIcon),
            ("AMOUNT", instance.amountIcon),
            ("PROBABILITY", instance.probabilityIcon),
            ("TIME", instance.timeIcon),
            ("STAR", instance.starIcon),
            ("COIN", instance.coinIcon),
        };
        foreach (var b in bondFrames)
        {
            // Regex: match 'HUMAN' or 'MECH' or 'SKULL' etc.
            // This will match the exact word, case-sensitive, surrounded by spaces or at the start
            string pattern = $@"{b.key}";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, m => $"<sprite name=\"{b.icon}\">");
        }

        return result;
    }

    public static bool IsConditionAllowed(CardMaster.CardType cardType, CardMaster.CardCondition condition)
    {
        switch (cardType)
        {
            case CardMaster.CardType.Base:
            case CardMaster.CardType.Gun:
                return condition == CardMaster.CardCondition.IsUndraggable;
            case CardMaster.CardType.Value:
            case CardMaster.CardType.Action:
                return true; // All conditions allowed
            case CardMaster.CardType.Instant:
                return false; // None allowed
            default:
                return false;
        }
    }

    public static float Growth(CardMaster.NumberType numberType)
    {
        if (instance == null) return 0f;

        switch (numberType)
        {
            case CardMaster.NumberType.Damage:
                return instance.damageGrowth[Random.Range(0, instance.damageGrowth.Count)];
            case CardMaster.NumberType.Health:
                return instance.healthGrowth[Random.Range(0, instance.healthGrowth.Count)];
            case CardMaster.NumberType.Mana:
                return instance.manaGrowth[Random.Range(0, instance.manaGrowth.Count)];
            case CardMaster.NumberType.Probability:
                return instance.probabilityGrowth[Random.Range(0, instance.probabilityGrowth.Count)];
            case CardMaster.NumberType.Coin:
                return instance.coinGrowth[Random.Range(0, instance.coinGrowth.Count)];
            default:
                return 0f;
        }
    }

    public static float Decay(CardMaster.NumberType numberType)
    {
        if (instance == null) return 0f;

        switch (numberType)
        {
            case CardMaster.NumberType.Damage:
                return instance.damageDecay[Random.Range(0, instance.damageDecay.Count)];
            case CardMaster.NumberType.Health:
                return instance.healthDecay[Random.Range(0, instance.healthDecay.Count)];
            case CardMaster.NumberType.Mana:
                return instance.manaDecay[Random.Range(0, instance.manaDecay.Count)];
            case CardMaster.NumberType.Probability:
                return instance.probabilityDecay[Random.Range(0, instance.probabilityDecay.Count)];
            case CardMaster.NumberType.Coin:
                return instance.coinDecay[Random.Range(0, instance.coinDecay.Count)];
            default:
                return 0f;
        }
    }
    
    public static string GetConditionName(CardMaster.CardCondition cond)
    {
        switch (cond)
        {
            case CardMaster.CardCondition.IsFrail: return "Frail";
            case CardMaster.CardCondition.IsFragile: return "Fragile";
            case CardMaster.CardCondition.IsTemporary: return "Temporary";
            case CardMaster.CardCondition.IsVolatile: return "Volatile";
            case CardMaster.CardCondition.IsDecaying: return "Decaying";
            case CardMaster.CardCondition.IsUndraggable: return "Undraggable";
            case CardMaster.CardCondition.IsGrowing: return "Growing";
            case CardMaster.CardCondition.IsPowerful: return "Powerful";
            default: return cond.ToString();
        }
    }
}
