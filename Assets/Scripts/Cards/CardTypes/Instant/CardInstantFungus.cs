using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantFungus : CardMaster
{
    // id: 422
    // name: Fungus
    // desc: Make linked card Grow once per 5 cards discarded

    [Header("Fungus Settings")]
    public int growInterval = 5; // Grow once per 5 cards discarded

    private CardMaster linked;
    private int times = 0;

    public override void OnCardEnable()
    {
        linked = up_link_cardmaster ?? left_link_cardmaster ?? right_link_cardmaster ?? down_link_cardmaster;
        if (linked != null)
        {
            times = GameEvents.discardedCardsCount / growInterval;
            if (linked.numberTypesCanBeModified.Count > 0)
            {
                linked.Grow(times);
                SoundManager.PlaySFX("GetCard");
                OnCardDestroyed();
            }
            else
            {
                ShowPopup("Can't grow this card!");
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, growInterval, times));
    }
}
