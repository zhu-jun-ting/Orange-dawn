using System;
using UnityEngine;

public class ItemCoinBox : ItemMaster
{
    [Header("CoinBox Settings")]
    [Tooltip("Chance to drop a coin on each hit (0-1)")]
    public float coinDropChance = 0.5f;
    public string tip = "+{0} COIN";
    [Tooltip("How many coins to drop per hit (if chance succeeds)")]
    public int coinDropAmount = 1;

    public override void OnHit(Collision2D collision)
    {
        base.OnHit(collision);
        // Try to drop coins on each hit
        if (UnityEngine.Random.value < coinDropChance)
        {
            if (CombatManager.instance != null)
            {
                CombatManager.instance.SpawnDrop(CombatManager.DropItem.Coin, this.transform, coinDropAmount);
                ShowMessageLocal(GameSettings.AddIcon(String.Format(tip, coinDropAmount)));
            }
        }
    }
}
