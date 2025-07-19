using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueStray : CardMaster
{
    // id: 207
    // name: Stray
    // This card has Damage: +20 if you have Coin: <=10 (do not check in battle)
    // Damage: 5

    [Header("Stray Settings")]
    [Tooltip("Bonus damage if coin condition met")] 
    public int bonusDamage = 20;
    [Tooltip("Coin threshold for bonus")] 
    public int coinThreshold = 10;

    private bool bonusApplied = false;

    protected override void Awake()
    {
        base.Awake();
        // Subscribe to coin update event
        GameEvents.instance.OnUpdateCoins += OnUpdateCoinsHandler;
        ApplyBonusIfNeeded();
    }

    private void OnUpdateCoinsHandler(int diffCoin)
    {
        ApplyBonusIfNeeded();
    }

    private void ApplyBonusIfNeeded()
    {
        if (CoinCounter.instance != null && CoinCounter.CanCostCoin(0))
        {
            int currentCoins = CoinCounter.coinCurrent;
            if (currentCoins <= coinThreshold && !bonusApplied && !CombatManager.isInBattle)
            {
                damage += bonusDamage;
                default_damage += bonusDamage;
                bonusApplied = true;
                CardMaster.InvokeUpdateCardTexts();
            }
            else if (currentCoins > coinThreshold && !CombatManager.isInBattle && bonusApplied)
            {
                damage -= bonusDamage;
                default_damage -= bonusDamage;
                bonusApplied = false;
                CardMaster.InvokeUpdateCardTexts();
            }
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
            string.Format(card_description, bonusDamage, coinThreshold, damage));
    }
}
