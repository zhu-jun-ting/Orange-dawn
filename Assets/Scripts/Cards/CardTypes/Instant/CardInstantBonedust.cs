using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantBonedust : CardMaster
{
    // id: 423
    // name: Bonedust
    // desc: When discarded, Grow all cards on board 1 time (if applicable)

    public override bool OnCardDestroyed()
    {
        // Grow all cards on board once
        List<CardMaster> allCards = BoardArea.instance.GetCardsOnBoard();
        foreach (CardMaster cm in allCards)
        {
            if (cm != null && cm.numberTypesCanBeModified.Count > 0)
            {
                cm.Grow(deathrattle_times);
            }
        }
        return base.OnCardDestroyed(); // Call base method to handle destruction
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, deathrattle_times));
    }
}
