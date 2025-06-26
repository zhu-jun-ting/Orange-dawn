using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantIncrementer : CardMaster
{
    [Header("Instant Incrementer Settings")]
    public float incrementValue = 1f;

    public override void OnCardEnable()
    {
        // Register OnThisCardLevelCleared for all linked cards
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        foreach (var link in linked)
        {
            if (link != null)
            {
                // Remove previous to avoid stacking
                link.OnThisCardLevelCleared -= () => link.UpdateSelfNumberValue(NumberType.Damage, incrementValue, true);
                link.OnThisCardLevelCleared += () => link.UpdateSelfNumberValue(NumberType.Damage, incrementValue, true);

                // Also give a new buff entry
                link.AddBuffEntry("Incrementer", "Increase damage value at the end of the level", 0);
            }
        }
        // Destroy self after registering
        OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return string.Format(card_description, incrementValue);
    }
}
