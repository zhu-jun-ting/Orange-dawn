using UnityEngine;

public class GameSettings : MonoBehaviour

{
    public static GameSettings instance;

    [Header("Board Settings")]
    public int boardRows = 3;
    public int boardColumns = 3;
    public float boardMargin = 10f;

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
            ("STAR", instance.starIcon, instance.starColor), // Add STAR as a type for icon replacement
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
            ("STAR", instance.starIcon), // Add STAR as a frame icon
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
}
