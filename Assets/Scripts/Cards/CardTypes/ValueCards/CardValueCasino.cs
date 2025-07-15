using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueCasino : CardMaster
{
    // id: 208
    // name: Casino
    // This card has Probability: +5 for every Coin: 50 you have
    // Probability: 2%

    [Header("Casino Settings")]
    [Tooltip("Probability gained per coin chunk")] 
    public int probabilityPerChunk = 5;
    [Tooltip("Coin chunk size for bonus")] 
    public int coinChunkSize = 50;

    private int currentChunks = 0;

    protected override void Awake()
    {
        base.Awake();
        GameEvents.instance.OnUpdateCoins += OnUpdateCoinsHandler;
        ApplyBonusProbability();
    }

    private void OnUpdateCoinsHandler(int diffCoin)
    {
        ApplyBonusProbability();
    }

    private void ApplyBonusProbability()
    {
        if (CoinCounter.instance != null && CoinCounter.instance.CanSpendCoins(0))
        {
            int currentCoins = CoinCounter.instance.coinCurrent;
            int chunks = currentCoins / coinChunkSize;

            if (chunks != currentChunks)
            {
                int diff = chunks - currentChunks;
                probability += probabilityPerChunk * diff;
                default_probability += probabilityPerChunk * diff;
                CardMaster.InvokeUpdateCardTexts();
                currentChunks = chunks;
            }
        }
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        if (GameEvents.instance != null)
            GameEvents.instance.OnUpdateCoins -= OnUpdateCoinsHandler;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probabilityPerChunk, coinChunkSize, probability));
    }
}
