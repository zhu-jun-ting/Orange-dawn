using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueDagger : CardMaster
{
    // id: 424
    // name: Dagger
    // desc: On turn end, discard the linked card and add half its sell price to a random stat (damage, health, probability) permanently if successfully discarded.

    [Header("Dagger Settings")]
    public float sellPriceModifier = 0.5f;

    private CardMaster linked;
    private System.Random rng = new System.Random();

    protected override void Awake()
    {
        base.Awake();
        // Subscribe to turn end event
        GameEvents.instance.OnLevelCleared += OnTurnEndHandler;
    }

    private void OnTurnEndHandler()
    {
        List<CardMaster> starCards;
        List<Vector2Int> starCardPositions;
        List<Vector2Int> starCardOffsets;
        GetStarCards(out starCards, out starCardPositions, out starCardOffsets);
        linked = (starCards != null && starCards.Count > 0) ? starCards[0] : null;
        if (linked != null)
        {
            int sellPrice = linked.card_sell_price;
            bool discarded = linked.OnCardDestroyed();
            if (discarded && sellPrice > 0)
            {
                float valueToAdd = Mathf.Floor(sellPrice * sellPriceModifier);
                int statIndex = rng.Next(0, 3); // 0: damage, 1: health, 2: probability
                switch (statIndex)
                {
                    case 0:
                        damage += valueToAdd;
                        default_damage += valueToAdd;
                        ShowPopup($"Damage: +{valueToAdd}");
                        break;
                    case 1:
                        health += valueToAdd;
                        default_health += valueToAdd;
                        ShowPopup($"Health: +{valueToAdd}");
                        break;
                    case 2:
                        probability += valueToAdd;
                        default_probability += valueToAdd;
                        ShowPopup($"Probability: +{valueToAdd}");
                        break;
                }
                CardMaster.InvokeUpdateCardTexts();
            }
        }
    }

    public override bool OnCardDestroyed()
    {
        // Unsubscribe to prevent memory leaks
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelCleared -= OnTurnEndHandler;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), damage, health, probability, sellPriceModifier));
    }

    public override UIStar.StarType GetStarType(CardMaster cardMaster = null)
    {
        return cardMaster.card_sell_price > 0 ? UIStar.StarType.Yellow : UIStar.StarType.White;
        
    }
}
