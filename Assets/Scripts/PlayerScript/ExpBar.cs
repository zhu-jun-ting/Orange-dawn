using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour

{
    public Text ExpText;
    public Text LevelText;
    public static float ExpCurrent;
    public static float ExpMax;
    public static int Level;

    [Header("Exp Setups")]
    public int ExpIncrementPerLevel = 50; // Amount of experience needed to level up

    // ...moved levelUpSelectCards to GameSettings...

    private Image ExperienceBar;
    // Start is called before the first frame update

    public static ExpBar instance;
    void Start()
    {
        instance = this;
        ExperienceBar = GetComponent<Image>();

        ExpMax = 100;
        ExpBar.Level = 1;
        ExpBar.ExpCurrent = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ExperienceBar.fillAmount = (float)ExpCurrent / (float)ExpMax;
        ExpText.text = ExpCurrent.ToString() + "/" + ExpMax.ToString();
        LevelText.text = "LV. " + Level.ToString();
    }

    public static void GainExp(float experience)
    {
        if (experience + ExpCurrent >= ExpMax)
        {
            Level += 1;
            ExpCurrent = ExpCurrent + experience - ExpMax;
            ExpMax += instance.ExpIncrementPerLevel;
            // HealthBar.HealthCurrent = HealthBar.HealthMax + 10;
            // HealthBar.HealthMax += 10;
            GameEvents.instance.LevelUp(Level);

            // --- Level Up Card Selection ---
            // Weighted rarity selection (same as shop)
            float level = Level;
            var gs = GameSettings.instance;
            float cWeight = gs != null ? gs.commonWeight : 1f;
            float uWeight = gs != null ? Mathf.Min(level * gs.uncommonWeightIncrement, gs.uncommonWeightCap) : Mathf.Min(level * 0.04f, 1f);
            float rWeight = gs != null ? Mathf.Min(level * gs.rareWeightIncrement, gs.rareWeightCap) : Mathf.Min(level * 0.03f, 1f);
            float eWeight = gs != null ? Mathf.Min(level * gs.epicWeightIncrement, gs.epicWeightCap) : Mathf.Min(level * 0.015f, 1f);
            float lWeight = gs != null ? Mathf.Min(level * gs.legendaryWeightIncrement, gs.legendaryWeightCap) : Mathf.Min(level * 0.007f, 1f);
            float totalWeight = cWeight + uWeight + rWeight + eWeight + lWeight;

            // Pick 3 cards for selection from GameSettings
            List<GameObject> selectedCards = new List<GameObject>();
            var levelUpSelectCards = gs != null ? gs.levelUpSelectCards : null;
            for (int i = 0; i < 3; i++)
            {
                CardMaster.CardRarity chosenRarity = CardMaster.CardRarity.Common;
                float roll = Random.value * totalWeight;
                if (roll < cWeight)
                    chosenRarity = CardMaster.CardRarity.Common;
                else if (roll < cWeight + uWeight)
                    chosenRarity = CardMaster.CardRarity.Uncommon;
                else if (roll < cWeight + uWeight + rWeight)
                    chosenRarity = CardMaster.CardRarity.Rare;
                else if (roll < cWeight + uWeight + rWeight + eWeight)
                    chosenRarity = CardMaster.CardRarity.Epic;
                else
                    chosenRarity = CardMaster.CardRarity.Legendary;

                // Find a card of the chosen rarity from levelUpSelectCards in GameSettings
                GameObject card = null;
                if (levelUpSelectCards != null && levelUpSelectCards.Count > 0)
                {
                    var candidates = levelUpSelectCards.FindAll(go => {
                        var cm = go.GetComponent<CardMaster>();
                        return cm != null && cm.card_rarity == chosenRarity;
                    });
                    if (candidates.Count > 0)
                        card = candidates[Random.Range(0, candidates.Count)];
                }
                // Fallback: pick any card
                if (card == null && levelUpSelectCards != null && levelUpSelectCards.Count > 0)
                    card = levelUpSelectCards[Random.Range(0, levelUpSelectCards.Count)];
                if (card != null)
                    selectedCards.Add(card);
            }
            // Show these cards to the player for selection using CardManager's queue system
            if (CardManager.instance != null && selectedCards.Count > 0)
            {
                CardManager.instance.QueueSelectCardObjects(selectedCards, true, 0.5f, (selectedCard) => {
                    // Optionally handle what happens after the player selects a card
                    // e.g., show a message, play a sound, etc.
                });
            }
        }
        else
        {
            ExpCurrent += experience;
        }
    }
}
