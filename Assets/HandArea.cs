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
}
