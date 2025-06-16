using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class HandArea : MonoBehaviour
{
    public static HandArea instance;
    private RectTransform rectTransform;

    [Header("Hand State")]
    public List<CardMaster> handCards = new List<CardMaster>();

    void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // Debug.Log("HandArea OnEnable called");
        CardMaster.OnUpdateCardValues += ResetAllHandCards;
    }

    void OnDisable()
    {
        CardMaster.OnUpdateCardValues -= ResetAllHandCards;
    }

    public bool IsPointInside(Vector2 screenPoint, Camera uiCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera);
    }

    public void AddCard(CardMaster card)
    {
        if (!handCards.Contains(card))
            handCards.Add(card);
    }

    public void RemoveCard(CardMaster card)
    {
        if (handCards.Contains(card))
            handCards.Remove(card);
    }

    public bool ContainsCard(CardMaster card)
    {
        return handCards.Contains(card);
    }

    // Reset all cards in the hand area
    public void ResetAllHandCards()
    {
        if (instance == null || instance.handCards == null) return;
        foreach (var card in instance.handCards)
        {
            if (card != null)
                card.Reset();
                // Debug.Log($"Resetting card: {card.name}");
        }
        
    }
}
