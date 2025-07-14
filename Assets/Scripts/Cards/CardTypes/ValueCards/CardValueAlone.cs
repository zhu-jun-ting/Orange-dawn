using System.Collections.Generic;
using UnityEngine;

public class CardValueAlone : CardMaster
{
    [Header("Alone Damage Settings")]
    public float increment = 0.5f;
    private float damageMultiplier = 1f;

    public override void OnCardEnable()
    {
        if (BoardArea.instance != null)
        {
            GetStarCards(out List<CardMaster> starCards, out List<UnityEngine.Vector2Int> emptySlots, out List<UnityEngine.Vector2Int> lockedSlots);
            int emptyCount = emptySlots != null ? emptySlots.Count : 0;
            damageMultiplier = 1f + increment * emptyCount;
            // Apply damageMultiplier to all linked cards
            CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
            foreach (CardMaster link in linked)
            {
                if (link != null)
                {
                    link.UpdateNumberValue(NumberType.Damage, damageMultiplier, this, isMult: true, isPermanent: false);
                }
            }
        }
        base.OnCardEnable();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, increment, damageMultiplier));
    }
}
