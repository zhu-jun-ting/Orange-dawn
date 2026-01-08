using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueCapitalist : CardMaster
{
    // id: custom
    // name: Capitalist
    // desc: Give this card Health: +2 for each 10 coin you have spent

    [Header("Capitalist Settings")]
    [Tooltip("Health gained per coin chunk spent")] 
    public int healthPerChunk = 2;
    [Tooltip("Coin chunk size for bonus")] 
    public int coinChunkSize = 10;

    private int currentChunks = 0;
    private int totalCoinSpent = 0;

    protected override void Awake()
    {
        base.Awake();
        GameEvents.instance.OnUpdateCoins += OnUpdateCoinsHandler;
        ApplyHealthBonus();
    }

    private void OnUpdateCoinsHandler(int diffCoin)
    {
        // Track negative coins (spent coins)
        if (diffCoin < 0)
        {
            totalCoinSpent += Mathf.Abs(diffCoin);
        }
        ApplyHealthBonus();
    }

    private void ApplyHealthBonus()
    {
        int chunks = totalCoinSpent / coinChunkSize;

        if (chunks != currentChunks)
        {
            int diff = chunks - currentChunks;
            health += healthPerChunk * diff;
            default_health += healthPerChunk * diff;
            CardMaster.InvokeUpdateCardTexts();
            currentChunks = chunks;
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
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), healthPerChunk, coinChunkSize, (int)health));
    }
}
