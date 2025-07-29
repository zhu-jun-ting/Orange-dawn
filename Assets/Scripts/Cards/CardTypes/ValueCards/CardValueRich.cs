using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueRich : CardMaster
{
    // id: 206
    // name: Rich
    // When you spend any coin, spend 10 more to gain +5 Health for this card
    // Initial Health: 5

    [Header("Rich Settings")]
    [Tooltip("Extra coin spent to gain health")] 
    public int extraCoinCost = 10;
    [Tooltip("Health gained per extra coin spend")] 
    public int healthGainAmount = 5;

    protected override void Awake()
    {
        base.Awake();
        // Subscribe to coin spend event
        GameEvents.instance.OnUpdateCoins += OnUpdateCoinsHandler;
    }

    private void OnUpdateCoinsHandler(int diffCoin)
    {
        // Only trigger if coins are spent (diffCoin < 0)
        if (diffCoin < 0)
        {
            // Try to spend extra coins
            if (CoinCounter.instance != null && CoinCounter.CanCostCoin(-extraCoinCost))
            {
                CoinCounter.instance.AddCoin(-extraCoinCost);
                health += healthGainAmount;
                default_health += healthGainAmount; // Update initial health to reflect the gain
                CardMaster.InvokeUpdateCardTexts(); // Update card texts to reflect new health
            }
            GameEvents.instance.TriggerActionCard(this, PlayerController.instance.transform);

        }
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        // Unsubscribe to prevent memory leaks
        if (GameEvents.instance != null)
            GameEvents.instance.OnUpdateCoins -= OnUpdateCoinsHandler;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(
            string.Format(GameSettings.LocalizeText(card_description), extraCoinCost, (int)healthGainAmount, (int)health));
    }
}
