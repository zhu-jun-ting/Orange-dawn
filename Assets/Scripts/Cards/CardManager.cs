using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    [Header("References")]
    public BoardArea boardArea;
    public HandArea handArea;

    [Header("Add Card Prompt UI")]
    public Transform cardPromptVeilAdd;
    public RectTransform horizontalLayoutGroupAdd;

    [Header("Select Card Prompt UI")]
    public Transform cardPromptVeilSelect;
    public RectTransform horizontalLayoutGroupSelect;

    // Reference to the card database asset (assign in inspector)
    public CardDatabase cardDatabase;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Adds a card GameObject to the hand area, handling instancing and animated placement.
    /// </summary>
    public void AddCardObject(GameObject go, float waitTime = 1f)
    {
        // 1. Activate the cardPromptVeilAdd transform to black out background with fade in
        if (cardPromptVeilAdd != null)
        {
            cardPromptVeilAdd.gameObject.SetActive(true);
            var cg = cardPromptVeilAdd.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.25f);
            }
        }

        // 2. Create the card object and set it to child of horizontalLayoutGroupAdd under cardPromptVeilAdd
        if (horizontalLayoutGroupAdd == null)
        {
            Debug.LogError("CardManager: horizontalLayoutGroupAdd not assigned!");
            return;
        }
        GameObject newCard = Instantiate(go, horizontalLayoutGroupAdd);
        var cardMaster = newCard.GetComponent<CardMaster>();
        if (cardMaster == null) return;
        newCard.transform.SetAsLastSibling();

        // 3. DebriManager.ScatterUIPixels at position of each card's position by passing its UI anchored position (local to parent)
        if (horizontalLayoutGroupAdd != null && newCard.TryGetComponent<RectTransform>(out var cardRect))
        {
            DebriManager.ScatterUIPixels(cardRect);
        }

        // 4. Make cards stay at the center for waitTime to let players read cards
        StartCoroutine(CardPromptSequence(newCard, cardMaster, waitTime));
    }

    private System.Collections.IEnumerator CardPromptSequence(GameObject newCard, CardMaster cardMaster, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (horizontalLayoutGroupAdd != null)
        {
            var rt = horizontalLayoutGroupAdd.GetComponent<RectTransform>();
            if (rt != null)
            {
                var seq = DOTween.Sequence();
                seq.Join(rt.DOAnchorPosY(-Screen.height, 0.4f));
                seq.AppendInterval(0.1f);
                seq.OnComplete(() => StartCoroutine(FinishCardPrompt(newCard, cardMaster, rt)));
            }
            else
            {
                StartCoroutine(FinishCardPrompt(newCard, cardMaster, null));
            }
        }
        else
        {
            StartCoroutine(FinishCardPrompt(newCard, cardMaster, null));
        }
    }

    private System.Collections.IEnumerator FinishCardPrompt(GameObject newCard, CardMaster cardMaster, RectTransform cardHolderRT)
    {
        yield return new WaitForSeconds(0.1f);
        if (cardMaster != null)
        {
            var rt = cardMaster.GetComponent<RectTransform>();
            if (rt != null && handArea != null)
                rt.SetParent(handArea.rectTransform, true);
            if (handArea != null)
                handArea.MoveCardToHand(cardMaster, null);
        }
        if (cardHolderRT != null)
        {
            cardHolderRT.localScale = Vector3.one;
            cardHolderRT.anchoredPosition = Vector2.zero;
        }
        if (cardPromptVeilAdd != null)
        {
            var cg = cardPromptVeilAdd.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0f, 0.2f).OnComplete(() => cardPromptVeilAdd.gameObject.SetActive(false));
            else
                cardPromptVeilAdd.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Displays a selection of card GameObjects, lets the player pick one, animates the result, and calls a callback.
    /// </summary>
    public void SelectCardObjects(List<GameObject> cards, bool addToHand = true, float waitTime = 1f, System.Action<GameObject> onSelected = null)
    {
        StartCoroutine(SelectCardObjectsCoroutine(cards, addToHand, waitTime, onSelected));
    }

    private System.Collections.IEnumerator SelectCardObjectsCoroutine(List<GameObject> cards, bool addToHand, float waitTime, System.Action<GameObject> onSelected)
    {
        // 1. Fade in veil
        if (cardPromptVeilSelect != null)
        {
            cardPromptVeilSelect.gameObject.SetActive(true);
            var cg = cardPromptVeilSelect.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.25f);
            }
        }
        // 2. Clear previous children
        foreach (Transform child in horizontalLayoutGroupSelect)
            Destroy(child.gameObject);
        // 3. Instantiate and display cards
        List<GameObject> displayedCards = new List<GameObject>();
        List<CardCommon> cardCommons = new List<CardCommon>();
        GameObject selectedCard = null;
        bool waitingForSelection = true;
        System.Action<CardCommon> onCardSelected = (cardCommon) => {
            if (!waitingForSelection) return;
            selectedCard = cardCommon.gameObject;
            waitingForSelection = false;
        };
        foreach (var prefab in cards)
        {
            GameObject card = Instantiate(prefab, horizontalLayoutGroupSelect);
            card.transform.SetAsLastSibling();
            if (card.TryGetComponent<RectTransform>(out var cardRect))
                DebriManager.ScatterUIPixels(cardRect);
            displayedCards.Add(card);
            var cardCommon = card.GetComponent<CardCommon>();
            if (cardCommon != null)
            {
                cardCommon.selectionMode = true;
                cardCommon.OnCardSelected += onCardSelected;
                cardCommons.Add(cardCommon);
            }
        }
        // 5. Wait for player to select
        while (waitingForSelection)
            yield return null;
        // 5.5. Disable selection mode and unsubscribe
        foreach (var cardCommon in cardCommons)
        {
            cardCommon.selectionMode = false;
            cardCommon.OnCardSelected -= onCardSelected;
        }
        // 6. Fade out other cards, highlight selected
        foreach (var card in displayedCards)
        {
            if (card == selectedCard) continue;
            if (card.TryGetComponent<CanvasGroup>(out var cg))
                cg.DOFade(0f, 0.3f);
            else
            {
                var newCg = card.AddComponent<CanvasGroup>();
                newCg.alpha = 1f;
                newCg.DOFade(0f, 0.3f);
            }
            card.transform.DOScale(0.8f, 0.3f);
        }
        // Highlight selected card
        if (selectedCard.TryGetComponent<RectTransform>(out var selRect))
        {
            selectedCard.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.3f);
            selectedCard.transform.DOScale(1f, waitTime).SetEase(Ease.OutBack);
            DebriManager.ScatterUIPixels(selRect);
            yield return new WaitForSeconds(waitTime);
        }
        // 7. Move to hand if needed
        if (addToHand && handArea != null && selectedCard != null)
        {
            var cardMaster = selectedCard.GetComponent<CardMaster>();
            if (cardMaster != null)
                handArea.MoveCardToHand(cardMaster, null);
        }
        // 8. Fade out veil
        if (cardPromptVeilSelect != null)
        {
            var cg = cardPromptVeilSelect.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0f, 0.2f).OnComplete(() => cardPromptVeilSelect.gameObject.SetActive(false));
            else
                cardPromptVeilSelect.gameObject.SetActive(false);
        }
        // 9. Callback
        onSelected?.Invoke(selectedCard);
    }

    /// <summary>
    /// Static method to get a card prefab by card id using the CardManager singleton.
    /// </summary>
    public static GameObject GetCardById(int cardId)
    {
        if (instance == null || instance.cardDatabase == null)
        {
            Debug.LogError("CardManager: No instance or cardDatabase assigned!");
            return null;
        }
        return instance.cardDatabase.GetCard(cardId);
    }
}
