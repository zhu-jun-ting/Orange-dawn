using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class HandArea : MonoBehaviour
{
    public static HandArea instance;
    public RectTransform rectTransform; // this is the card holder
    public Transform zoomableTransform; // Transform for zoomable cards, if any

    [Header("Hand State")]
    public List<CardMaster> handCards = new List<CardMaster>();

    [Header("Discarded")]
    public List<CardMaster> discardedCards = new List<CardMaster>();
    public Transform DiscardedCardsParent; // Parent for discarded cards, if any

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

    public void AddDiscardedCard(CardMaster card)
    {
        if (!discardedCards.Contains(card))
            discardedCards.Add(card);
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

    /// <summary>
    /// Adds a card GameObject to the hand area, handling instancing and animated placement.
    /// </summary>
    public void AddCardObject(GameObject go, RectTransform rectTransform_ = null)
    {
        // Return if no CardMaster component
        var cardMaster = go.GetComponent<CardMaster>();
        if (cardMaster == null) return;

        // If CardMaster has no instance in the scene, create a new instance and move it to hand
        if (cardMaster.instance == null)
        {
            // Instantiate a new card GameObject
            // Instantiate the card at the center of the screen (in Canvas space)
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenCenter, null, out Vector2 localPoint);
            GameObject newCard = Instantiate(go, rectTransform.TransformPoint(localPoint), Quaternion.identity, rectTransform.parent);

            var newCardMaster = newCard.GetComponent<CardMaster>();
            if (newCardMaster == null) return;
            // Move to hand with animation
            MoveCardToHand(newCardMaster, rectTransform_);

            // AddCard(newCardMaster);
        }
        else
        {
            // Move the existing instance to hand with animation
            MoveCardToHand(cardMaster.instance, rectTransform_);
            // AddCard(cardMaster.instance);
        }
    }

    /// <summary>
    /// Moves a CardMaster to the first available spot in the hand area with an ease animation.
    /// </summary>
    private void MoveCardToHand(CardMaster card, RectTransform rectTransform_)
    {
        // Find a vacant position in the hand area
        Vector2? vacantPos = FindVacantHandPosition(card);
        Vector2 targetPos;
        if (vacantPos.HasValue)
        {
            targetPos = vacantPos.Value;
        }
        else
        {
            // If no vacant spot, pick a random position within the hand area
            if (rectTransform_ == null)
                targetPos = GetRandomHandPosition(card);
            else
                targetPos = rectTransform_.anchoredPosition;
        }
        // Animate the card to the target position (requires a RectTransform)
        var rt = card.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.SetParent(rectTransform, true);
            rt.SetAsLastSibling();
            // Use DOTween for smooth animation
            rt.DOAnchorPos(targetPos, 0.4f).SetEase(DG.Tweening.Ease.OutBack);
        }
        // Add to handCards if not already present
        AddCard(card);
    }

    /// <summary>
    /// Finds the first vacant, non-overlapping position in the hand area (searches left-to-right, top-to-bottom).
    /// The returned position is relative to the center-anchored HandArea (so top-left is at (-width/2, height/2)).
    /// </summary>
    private Vector2? FindVacantHandPosition(CardMaster card)
    {
        var rt = card.GetComponent<RectTransform>();
        if (rt == null) return null;
        float cardWidth = rt.rect.width;
        float cardHeight = rt.rect.height;
        float spacing = (GameSettings.instance ? GameSettings.instance.boardMargin : 10f) * zoomableTransform.localScale.x;
        float areaWidth = rectTransform.rect.width;
        float areaHeight = rectTransform.rect.height;
        // Offset so (0,0) is top-left in center-anchored HandArea
        Vector2 topLeft = new Vector2(-areaWidth / 2f, areaHeight / 2f);
        int maxCols = Mathf.FloorToInt((areaWidth + spacing) / (cardWidth + spacing));
        int maxRows = Mathf.FloorToInt((areaHeight + spacing) / (cardHeight + spacing));
        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                Vector2 pos = topLeft + new Vector2(
                    spacing + col * (cardWidth + spacing),
                    -spacing - row * (cardHeight + spacing)
                );
                if (!IsOverlappingHandCard(pos, cardWidth, cardHeight))
                    return pos;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a given position would overlap any existing hand card.
    /// </summary>
    private bool IsOverlappingHandCard(Vector2 pos, float width, float height)
    {
        Rect newRect = new Rect(pos, new Vector2(width, height));
        foreach (var c in handCards)
        {
            if (c == null) continue;
            var rt = c.GetComponent<RectTransform>();
            if (rt == null) continue;
            Rect cardRect = new Rect(rt.anchoredPosition, rt.rect.size);
            if (newRect.Overlaps(cardRect))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns a random position within the hand area, using card size and center-anchored HandArea.
    /// </summary>
    private Vector2 GetRandomHandPosition(CardMaster card)
    {
        var rt = card.GetComponent<RectTransform>();
        float cardWidth = rt ? rt.rect.width : 160f;
        float cardHeight = rt ? rt.rect.height : 220f;
        float areaWidth = rectTransform.rect.width;
        float areaHeight = rectTransform.rect.height;
        float x = Random.Range(-areaWidth / 2f, areaWidth / 2f - cardWidth);
        float y = Random.Range(-areaHeight / 2f + cardHeight, areaHeight / 2f);
        return new Vector2(x, y);
    }
}
