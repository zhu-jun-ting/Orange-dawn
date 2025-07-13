using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ItemCardBox: When destroyed, gives a random card from a provided list, with each card having its own assigned chance.
/// </summary>
public class ItemCardBox : ItemMaster
{
    [Header("CardBox Settings")]
    [Tooltip("List of card prefabs to give")] 
    public List<GameObject> cardPrefabs = new List<GameObject>();
    [Tooltip("Chance for each card prefab (must match cardPrefabs count)")]
    public List<float> cardChances = new List<float>();
    public string tip = "New Card";

    // Called when the item is destroyed (e.g., by animation event or maxHits reached)
    public override void OnItemDestroyed(Collision2D collision)
    {
        GiveCard();
    }

    private void GiveCard()
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0 || cardChances == null || cardChances.Count != cardPrefabs.Count)
            return;
        int idx = RollCardIndex();
        if (idx >= 0 && idx < cardPrefabs.Count && CardManager.instance != null)
        {
            CardManager.instance.QueueAddCardObjects(new List<GameObject> { cardPrefabs[idx] });
            ShowTip(GameSettings.AddIcon(tip));
        }
    }

    // Rolls a card index based on the assigned chances
    private int RollCardIndex()
    {
        float total = 0f;
        foreach (var chance in cardChances)
            total += chance;
        float roll = Random.Range(0f, total);
        for (int i = 0; i < cardChances.Count; i++)
        {
            if (roll < cardChances[i])
                return i;
            roll -= cardChances[i];
        }
        return 0; // fallback: first card
    }
}
