using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantRedraw : CardMaster
{
    // id: 420
    // name: Redraw
    // desc: Discard the linked card, then find a card with same rarity

    public override void OnCardEnable()
    {
        // Only one linked card (first found)
        CardMaster linked = up_link_cardmaster ?? left_link_cardmaster ?? right_link_cardmaster ?? down_link_cardmaster;
        if (linked != null && linked.card_bonds != null && linked.card_bonds.Count > 0)
        {
            // Discard the linked card using its OnCardDestroyed
            bool destroyed = linked.OnCardDestroyed();
            if (!destroyed) return; // If not successfully destroyed, do nothing
            // If successfully discarded (destroyed), find 3 cards with the same bond
            var bonds = linked.card_bonds;
            List<GameObject> candidates = CardDatabase.FindCards(cm => cm.card_rarity == linked.card_rarity);

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

    public override string GetDescription() => GameSettings.AddIcon(string.Format(card_description));
}