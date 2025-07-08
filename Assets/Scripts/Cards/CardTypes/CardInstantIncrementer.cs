using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantIncrementer : CardMaster
{
    [Header("Instant Incrementer Settings")]
    public float incrementValue = 1f;
    private BuffEntry buffEntry;
    [Header("Buff Entry Text")]
    public string buffName = "Incrementer";
    [TextArea (3, 10)]
    public string buffDescription = "Increase damage value {0} at the end of the level";

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
                buffEntry = link.AddBuffEntry(GetBuffEntryName(), GetBuffEntryText(), 0);

                // Register to update buffEntry's name and description when card texts update
                CardMaster.OnUpdateCardTexts += UpdateBuffEntry;
            }
        }
        // Destroy self after registering
        OnCardDestroyed();
    }

    public override string GetBuffEntryName()
    {
        return buffName;
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(buffDescription, incrementValue));
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, incrementValue));
    }
}
