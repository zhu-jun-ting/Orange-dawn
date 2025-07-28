

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantBlend : CardMaster
{
    // id: 421
    // name: Blend
    // desc: Discard the linked card, then find a card with same bond

    public override void OnCardEnable()
    {
        // Only one linked card (first found)
        CardMaster linked = up_link_cardmaster ?? left_link_cardmaster ?? right_link_cardmaster ?? down_link_cardmaster;
        if (linked != null && linked.card_bonds != null && linked.card_bonds.Count > 0)
        {
            var bonds = linked.card_bonds;
            if (bonds != null && bonds.Count == 0)
            {
                ShowPopup("No Bonds");
                return;
            }
            // Discard the linked card using its OnCardDestroyed
            linked.OnCardDestroyed();
            // If successfully discarded (destroyed), find 3 cards with the same bond
            List<GameObject> candidates = CardDatabase.FindCards(cm =>
            {
                if (cm.card_bonds == null || bonds == null) return false;
                if (cm.card_bonds.Count == 0 || bonds.Count == 0) return false;
                // At least one bond matches
                foreach (var b in bonds)
                    if (cm.card_bonds.Contains(b)) return true;
                return false;
            });

            if (candidates != null && candidates.Count > 0)
            {
                // Ensure unique cards (no duplicates)
                HashSet<GameObject> uniqueCards = new HashSet<GameObject>(candidates);
                List<GameObject> selectCards = new List<GameObject>(uniqueCards);
                // Select up to 3 cards (already shuffled)
                int count = Mathf.Min(3, selectCards.Count);
                selectCards = selectCards.GetRange(0, count);
                if (selectCards.Count > 0 && CardManager.instance != null)
                    CardManager.instance.QueueSelectCardObjects(selectCards, true, 1f, null);
            }

            SoundManager.PlaySFX("GetCard");
            OnCardDestroyed(); // Destroy self after applying
        }
    }

    public override string GetDescription() => GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description)));

    public override UIStar.StarType GetStarType(CardMaster cardMaster = null)
    {
        return base.GetStarType(cardMaster);
    }
}
