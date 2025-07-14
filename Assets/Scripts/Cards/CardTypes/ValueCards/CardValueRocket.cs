using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueRocket : CardMaster
{
    // id: 209
    // name: Rocket
    // This card has Damage: +30 if you have Coin: >= 100
    // Damage: 1

    [Header("Rocket Settings")]
    [Tooltip("Bonus damage if coin condition met")] 
    public int bonusDamage = 30;
    [Tooltip("Coin threshold for bonus")] 
    public int coinThreshold = 100;

    private bool bonusApplied = false;

    protected override void Awake()
    {
        base.Awake();
        GameEvents.instance.OnUpdateCoins += OnUpdateCoinsHandler;
        ApplyBonusIfNeeded();
    }

    private void OnUpdateCoinsHandler(int diffCoin)
    {
        ApplyBonusIfNeeded();
    }

    private void ApplyBonusIfNeeded()
    {
        if (CoinCounter.instance != null && CoinCounter.instance.CanSpendCoins(0))
        {
            int currentCoins = CoinCounter.instance.coinCurrent;
            if (currentCoins >= coinThreshold && !bonusApplied)
            {
                damage += bonusDamage;
                default_damage += bonusDamage;
                bonusApplied = true;
                CardMaster.InvokeUpdateCardTexts();
            }
            else if (currentCoins < coinThreshold && bonusApplied)
            {
                damage -= bonusDamage;
                default_damage -= bonusDamage;
                bonusApplied = false;
                CardMaster.InvokeUpdateCardTexts();
            }
        }
    }

    public override void OnCardDestroyed()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnUpdateCoins -= OnUpdateCoinsHandler;
        base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(
            string.Format(card_description, bonusDamage, coinThreshold, damage));
    }
}
