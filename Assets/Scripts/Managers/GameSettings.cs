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
    public string damageColor = "#FF0000"; // Red
    public string damageIcon = "Sword"; // Icon name for damage
    public string healthColor = "#00FF00";
    public string healthIcon = "Heart";
    public string speedColor = "#00FFFF";
    public string speedIcon = "Lightning";
    public string manaColor = "#00BFFF";
    public string manaIcon = "Mana";
    public string amountColor = "#FFFF00";
    public string amountIcon = "Cards";
    public string probabilityColor = "#FFA500";
    public string probabilityIcon = "Dice";
    public string timeColor = "#FF00FF";
    public string timeIcon = "Star";

    [Header("Card Colors")]
    public float destroyEffectDuration = 0.5f;

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
        var types = new (string type, string icon, string color)[]
        {
            ("Damage", instance.damageIcon, instance.damageColor),
            ("Health", instance.healthIcon, instance.healthColor),
            ("Speed", instance.speedIcon, instance.speedColor),
            ("Mana", instance.manaIcon, instance.manaColor),
            ("Amount", instance.amountIcon, instance.amountColor),
            ("Probability", instance.probabilityIcon, instance.probabilityColor),
            ("Time", instance.timeIcon, instance.timeColor),
        };

        foreach (var t in types)
        {
            // Regex: Type: XX (where XX is any non-whitespace sequence between two whitespaces)
            string pattern = $@"{t.type}: ([^\s]+)";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, m =>
            {
                string value = m.Groups[1].Value;
                return $"<sprite name=\"{t.icon}\"> <color={t.color}>{value}</color>";
            });
        }
        return result;
    }
}
