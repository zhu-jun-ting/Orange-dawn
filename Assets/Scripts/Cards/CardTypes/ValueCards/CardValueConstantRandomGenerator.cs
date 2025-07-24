using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;
using System.Collections.Generic;
using NUnit.Framework;

public class CardValueConstantRandomGenerator : CardMaster
{
    [Header("Attributes: Counts")]
    public Vector2Int attributeCountRange = new Vector2Int(1, 6);
    [Tooltip("The bias towards min value, higher values increase the chance of rolling towards minimum value.")][UnityEngine.Range(1, 20)] public float attributeCountBias = 3f;

    [Header("Attributes: Values Ranges")]

    public Vector2Int damageRange = new Vector2Int(1, 10);
    public Vector2Int healthRange = new Vector2Int(1, 20);
    public Vector2Int probabilityRange = new Vector2Int(1, 10);
    public Vector2Int amountRange = new Vector2Int(1, 1);
    public Vector2Int manaRange = new Vector2Int(-3, 3);
    public Vector2Int coinRange = new Vector2Int(-3, 3);
    [Tooltip("The bias towards min value, higher values increase the chance of rolling towards minimum value.")][UnityEngine.Range(1, 20)] public float attributeValueBias = 1.5f;

    [Header("Attributes: Occurrences Factors")]
    [Tooltip("Factors to control the occurrence of each attribute in the generated card. 0 means never occur, 1 means high occurrence.")]
    [UnityEngine.Range(0, 1)] public float damageOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float healthOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float probabilityOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float amountOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float manaOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float coinOccurrenceFactor = 0.5f;

    [Header("Random Generator: Calculate Coin Cost")]
    public const int costPerLinkOpened = 5;

    public const int costPerDamage = 2;
    public const int costPerHealth = 1;
    public const int costPerProbability = 2;
    public const int costPerAmount = 35;
    public const int costPerMana = 5;
    public const int costPerCoin = 5;
    
    public const int thresholdUncommon = 17;
    public const int thresholdRare = 30;
    public const int thresholdEpic = 50;
    public const int thresholdLegendary = 75;

    [Header("Random Generator: Link Counts")]
    public Vector2Int linkCountRange = new Vector2Int(1, 4);
    [Tooltip("The bias towards min value, higher values increase the chance of rolling towards minimum value.")][UnityEngine.Range(1, 20)] public float linkCountBias = 3f;

    [Header("Optional: Random Bond")]
    [Tooltip("The chance to have a random bond on the card")][UnityEngine.Range(0, 1)] public float randomBondProbability = 0.2f;

    [Header("Optional: Random Conditions")]
    public Vector2Int conditionCountRange = new Vector2Int(0, 5);
    [Tooltip("The bias towards min value, higher values increase the chance of rolling towards minimum value.")][UnityEngine.Range(1, 20)] public float conditionCountBias = 3f;

    // Condition occurrence factors
    [UnityEngine.Range(0, 1)] public float undraggableOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float powerfulOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float frailOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float fragileOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float temporaryOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float volatileOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float growingOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float decayingOccurrenceFactor = 0.5f;
    [UnityEngine.Range(0, 1)] public float eternalOccurrenceFactor = 0.5f;

    [Header("Universal Settings")]
    public bool generateRandomBond = false;
    public bool generateRandomConditions = false;


    private List<CardCondition> allConds = new List<CardCondition>();
    private Dictionary<CardCondition, float> condWeights = new Dictionary<CardCondition, float>();
    // internal vars

    public static CardValueConstantRandomGenerator instance;
    
    protected override void Awake()
    {
        GENERATE();
        base.Awake();
        instance = this; // Set the instance to this for static access
    }

    public override void Start()
    {
        
        base.Start(); 
    }

    // Static method to get a random bond
    public CardBond GetRandomBond()
    {
        if (UnityEngine.Random.value < randomBondProbability)
        {
            var bondValues = System.Enum.GetValues(typeof(CardBond));
            int idx = UnityEngine.Random.Range(0, bondValues.Length);
            return (CardBond)bondValues.GetValue(idx);
        }
        return default;
    }

    // Static method to get random conditions
    public List<CardCondition> GetRandomConditions(int minCondCount = -1, int maxCondCount = -1)
    {
        // Use class values if default
        int minCount = minCondCount < 0 ? conditionCountRange.x : minCondCount;
        int maxCount = maxCondCount < 0 ? conditionCountRange.y : maxCondCount;
        int condCount = RandomIntWithBias(minCount, maxCount, conditionCountBias);
        allConds = new List<CardCondition>
            {
                CardCondition.IsUndraggable,
                CardCondition.IsPowerful,
                CardCondition.IsFrail,
                CardCondition.IsFragile,
                CardCondition.IsTemporary,
                CardCondition.IsVolatile,
                CardCondition.IsGrowing,
                CardCondition.IsDecaying,
                CardCondition.IsEternal
            };
        condWeights = new Dictionary<CardCondition, float> {
            { CardCondition.IsUndraggable, undraggableOccurrenceFactor },
            { CardCondition.IsPowerful, powerfulOccurrenceFactor },
            { CardCondition.IsFrail, frailOccurrenceFactor },
            { CardCondition.IsFragile, fragileOccurrenceFactor },
            { CardCondition.IsTemporary, temporaryOccurrenceFactor },
            { CardCondition.IsVolatile, volatileOccurrenceFactor },
            { CardCondition.IsGrowing, growingOccurrenceFactor },
            { CardCondition.IsDecaying, decayingOccurrenceFactor },
            { CardCondition.IsEternal, eternalOccurrenceFactor }
        };
        var condCandidates = new List<CardCondition>(allConds.FindAll(c => condWeights[c] > 0f));
        int condSelectCount = Mathf.Min(condCount, condCandidates.Count);
        var result = new List<CardCondition>();
        for (int i = 0; i < condSelectCount; i++)
        {
            float totalWeight = 0f;
            foreach (var cond in condCandidates)
                totalWeight += condWeights[cond];
            float roll = UnityEngine.Random.value * totalWeight;
            float accum = 0f;
            CardCondition chosen = condCandidates[0];
            foreach (var cond in condCandidates)
            {
                accum += condWeights[cond];
                if (roll <= accum)
                {
                    chosen = cond;
                    break;
                }
            }
            result.Add(chosen);
            condCandidates.Remove(chosen);
        }
        return result;
    }












    public void GENERATE()
    {
        // 1. Attributes
        var attributeNames = new List<string> { "damage", "health", "probability", "amount", "mana", "coin" };
        var attributeRanges = new Dictionary<string, Vector2Int> {
            { "damage", damageRange },
            { "health", healthRange },
            { "probability", probabilityRange },
            { "amount", amountRange },
            { "mana", manaRange },
            { "coin", coinRange }
        };
        var occurrenceFactors = new Dictionary<string, float> {
            { "damage", damageOccurrenceFactor },
            { "health", healthOccurrenceFactor },
            { "probability", probabilityOccurrenceFactor },
            { "amount", amountOccurrenceFactor },
            { "mana", manaOccurrenceFactor },
            { "coin", coinOccurrenceFactor }
        };

        int attributeCount = RandomIntWithBias(attributeCountRange.x, attributeCountRange.y, attributeCountBias);
        // Weighted random selection without replacement
        var available = attributeNames.FindAll(attr => occurrenceFactors[attr] > 0f);
        var selected = new List<string>();
        var candidates = new List<string>(available);
        int n = Mathf.Min(attributeCount, candidates.Count);
        for (int i = 0; i < n; i++)
        {
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var attr in candidates)
                totalWeight += occurrenceFactors[attr];
            // Roll
            float roll = UnityEngine.Random.value * totalWeight;
            float accum = 0f;
            string chosen = candidates[0];
            foreach (var attr in candidates)
            {
                accum += occurrenceFactors[attr];
                if (roll <= accum)
                {
                    chosen = attr;
                    break;
                }
            }
            selected.Add(chosen);
            candidates.Remove(chosen);
        }

        // Reset all attributes to 0
        damage = health = probability = amount = mana = coin = 0;
        foreach (var attr in selected)
        {
            var range = attributeRanges[attr];
            int value = RandomIntWithBias(range.x, range.y, attributeValueBias);
            switch (attr)
            {
                case "damage": damage = value; break;
                case "health": health = value; break;
                case "probability": probability = value; break;
                case "amount": amount = value; break;
                case "mana": mana = value == 0 ? 1 : value; break;
                case "coin": coin = value == 0 ? 1 : value; break;
            }
        }

        // 2. Links
        int linkCount = RandomIntWithBias(linkCountRange.x, linkCountRange.y, linkCountBias);
        linkCount = Mathf.Clamp(linkCount, 1, 4);
        this.linkCount = linkCount;

        // 3. Calculate card cost
        int cost = 0;
        cost += Mathf.Abs(Mathf.RoundToInt(damage)) * costPerDamage;
        cost += Mathf.Abs(Mathf.RoundToInt(health)) * costPerHealth;
        cost += Mathf.Abs(Mathf.RoundToInt(probability)) * costPerProbability;
        cost += Mathf.Abs(Mathf.RoundToInt(amount)) * costPerAmount;
        cost += Mathf.Abs(Mathf.RoundToInt(mana)) * costPerMana;
        cost += Mathf.Abs(Mathf.RoundToInt(coin)) * costPerCoin;
        cost += linkCount * costPerLinkOpened;
        card_cost = cost;
        card_sell_price = cost / 4;
        if (cost >= thresholdLegendary)
            card_rarity = CardRarity.Legendary;
        else if (cost >= thresholdEpic)
            card_rarity = CardRarity.Epic;
        else if (cost >= thresholdRare)
            card_rarity = CardRarity.Rare;
        else if (cost >= thresholdUncommon)
            card_rarity = CardRarity.Uncommon;
        else
            card_rarity = CardRarity.Common;

        // 4. Bond
        card_bonds.Clear();
        if (generateRandomBond && UnityEngine.Random.value < randomBondProbability)
        {
            var bondValues = System.Enum.GetValues(typeof(CardBond));
            int idx = UnityEngine.Random.Range(0, bondValues.Length);
            card_bonds.Add((CardBond)bondValues.GetValue(idx));
        }

        // 5. CardConditions
        card_conditions.Clear();
        allConds = new List<CardCondition>
            {
                CardCondition.IsUndraggable,
                CardCondition.IsPowerful,
                CardCondition.IsFrail,
                CardCondition.IsFragile,
                CardCondition.IsTemporary,
                CardCondition.IsVolatile,
                CardCondition.IsGrowing,
                CardCondition.IsDecaying,
                CardCondition.IsEternal
            };
        condWeights = new Dictionary<CardCondition, float> {
            { CardCondition.IsUndraggable, undraggableOccurrenceFactor },
            { CardCondition.IsPowerful, powerfulOccurrenceFactor },
            { CardCondition.IsFrail, frailOccurrenceFactor },
            { CardCondition.IsFragile, fragileOccurrenceFactor },
            { CardCondition.IsTemporary, temporaryOccurrenceFactor },
            { CardCondition.IsVolatile, volatileOccurrenceFactor },
            { CardCondition.IsGrowing, growingOccurrenceFactor },
            { CardCondition.IsDecaying, decayingOccurrenceFactor },
            { CardCondition.IsEternal, eternalOccurrenceFactor }
        };
        if (generateRandomConditions)
        {
            int condCount = RandomIntWithBias(conditionCountRange.x, conditionCountRange.y, conditionCountBias);
            var condCandidates = new List<CardCondition>(allConds.FindAll(c => condWeights[c] > 0f));
            int condSelectCount = Mathf.Min(condCount, condCandidates.Count);
            for (int i = 0; i < condSelectCount; i++)
            {
                float totalWeight = 0f;
                foreach (var cond in condCandidates)
                    totalWeight += condWeights[cond];
                float roll = UnityEngine.Random.value * totalWeight;
                float accum = 0f;
                CardCondition chosen = condCandidates[0];
                foreach (var cond in condCandidates)
                {
                    accum += condWeights[cond];
                    if (roll <= accum)
                    {
                        chosen = cond;
                        break;
                    }
                }
                card_conditions.Add(chosen);
                condCandidates.Remove(chosen);
            }
        }

        // 6. Common fields
        card_name = $"Random Card {Random.Range(100, 999)}";
        card_type = CardType.Value;
        useRandomLinks = true;
    }

    public static float RandomWithBias(float min, float max, float bias)
    {
        // Generate a random value between min and max with bias towards min
        // Bias towards min (smaller values more likely)
        float t = UnityEngine.Random.value; // uniform [0,1)
        float biased = min + (max - min) * Mathf.Pow(t, bias); // t*t biases towards 0

        // If you want integer:
        return biased;
    }
    
    public static int RandomIntWithBias(int min, int max, float bias)
    {
        // Generate a random integer between min and max with bias towards min
        float biased = RandomWithBias(min, max, bias);
        return Mathf.RoundToInt(biased);
    }
}














#if UNITY_EDITOR
[CustomEditor(typeof(CardValueConstantRandomGenerator))]
public class CardValueConstantRandomGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CardValueConstantRandomGenerator myTarget = (CardValueConstantRandomGenerator)target;
        if (GUILayout.Button("Generate Random Damage"))
        {
            myTarget.GENERATE();
            EditorUtility.SetDirty(myTarget);
        }
    }
}
#endif