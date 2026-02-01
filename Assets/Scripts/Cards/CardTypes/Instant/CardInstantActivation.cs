using UnityEngine;
using System.Collections.Generic;

// id: 434
// name: Activation
// desc: Activate a random closed cell adjacent to an open cell.

public class CardInstantActivation : CardMaster
{
    public override void OnCardEnable()
    {
        if (BoardArea.instance == null) return;
        
        if (BoardArea.instance.ActivateRandomSlot())
        {
            OnCardDestroyed();
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description)));
    }
}
