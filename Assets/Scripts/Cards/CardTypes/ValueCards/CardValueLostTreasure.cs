using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueLostTreasure : CardMaster
{
    // id: 219
    // name: Lost Treasure
    // desc: When discarded, give you Coin: 30"

    [Header("Lost Treasure Settings")]
    public int coinGain = 30; // Amount of Coin to gain when this card is discarded

    public override bool OnCardDestroyed()
    {
        // Give you 30 Coin
        if (CoinCounter.instance != null)
        {
            CoinCounter.instance.AddCoin(coinGain);
            ShowPopup($"Coin: {coinGain}");
        }
        else
        {
            Debug.LogWarning("CoinCounter instance is null, cannot add coins.");
        }
        return base.OnCardDestroyed(); // Call base method to handle destruction
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, coinGain, health));
    }
}
