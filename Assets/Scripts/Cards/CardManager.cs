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


    // --- Card Add/Select UI Queue Logic ---
    private Queue<System.Func<System.Collections.IEnumerator>> cardUiQueue = new Queue<System.Func<System.Collections.IEnumerator>>();
    private bool isCardUiSequenceRunning = false;
    [Header("Card UI Queue Settings")]
    [Tooltip("Delay (in seconds) between card UI sequences")] 
    public float cardUiQueueDelay = 0.5f;
    [Tooltip("Delay (in seconds) after each card UI sequence before next")] 
    public float cardUiQueuePostDelay = 1f;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Public method to add a card add UI sequence to the queue.
    /// </summary>
    public void QueueAddCardObjects(List<GameObject> cardPrefabs, float waitTime = 1f)
    {
        // Check all previous queued AddCardObjectsSequence methods and combine cardPrefabs
        bool combined = false;
        if (cardUiQueue.Count > 0)
        {
            var queueArray = cardUiQueue.ToArray();
            for (int i = queueArray.Length - 1; i >= 0; i--)
            {
                var queuedFunc = queueArray[i];
                var method = queuedFunc.Method;
                if (method != null && method.Name.Contains("QueueAddCardObjects"))
                {
                    var target = queuedFunc.Target;
                    if (target != null)
                    {
                        var fi = target.GetType().GetField("cardPrefabs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                        if (fi != null)
                        {
                            var existingList = fi.GetValue(target) as List<GameObject>;
                            if (existingList != null)
                            {
                                existingList.AddRange(cardPrefabs);
                                combined = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (!combined)
        {
            cardUiQueue.Enqueue(() => AddCardObjectsSequence(cardPrefabs, waitTime));
        }
        TryRunNextCardUiSequence();
    }

    public void QueueAddCardObjects(List<int> cardIds, float waitTime = 1f)
    {
        List<GameObject> cardPrefabs = new List<GameObject>();
        foreach (int cardId in cardIds)
        {
            GameObject prefab = CardDatabase.GetCard(cardId);
            if (prefab != null)
                cardPrefabs.Add(prefab);
        }
        QueueAddCardObjects(cardPrefabs, waitTime);
    }


    /// <summary>
    /// Public method to add a card select UI sequence to the queue.
    /// </summary>
    public void QueueSelectCardObjects(List<GameObject> cards, bool addToHand = true, float waitTime = 1f, System.Action<GameObject> onSelected = null)
    {
        cardUiQueue.Enqueue(() => SelectCardObjectsSequence(cards, addToHand, waitTime, onSelected));
        TryRunNextCardUiSequence();
    }


    /// --- Coroutines ---
    private void TryRunNextCardUiSequence()
    {
        if (!isCardUiSequenceRunning && cardUiQueue.Count > 0)
        {
            isCardUiSequenceRunning = true;
            StartCoroutine(RunCardUiSequence());
        }
    }

    private System.Collections.IEnumerator RunCardUiSequence()
    {
        // If in battle, hang the whole sequence until level is cleared
        if (CombatManager.isInBattle)
        {
            bool levelCleared = false;
            System.Action handler = () => { levelCleared = true; };
            if (GameEvents.instance != null)
                GameEvents.instance.OnLevelCleared += handler;
            while (!levelCleared)
                yield return null;
            yield return new WaitForSeconds(2f);
            if (GameEvents.instance != null)
                GameEvents.instance.OnLevelCleared -= handler;
        }
        while (cardUiQueue.Count > 0)
        {
            var next = cardUiQueue.Dequeue();
            yield return StartCoroutine(next());
            if (cardUiQueueDelay > 0f)
                yield return new WaitForSeconds(cardUiQueueDelay);
            if (cardUiQueuePostDelay > 0f)
                yield return new WaitForSeconds(cardUiQueuePostDelay);
        }
        isCardUiSequenceRunning = false;
    }

    // --- Add Card Sequence with Battle State Check ---
    private System.Collections.IEnumerator AddCardObjectsSequence(List<GameObject> cardPrefabs, float waitTime)
    {
        // // If in battle, wait for level clear event and 2s, then show UI
        // if (CombatManager.isInBattle)
        // {
        //     bool levelCleared = false;
        //     System.Action handler = () => { levelCleared = true; };
        //     if (GameEvents.instance != null)
        //         GameEvents.instance.OnLevelCleared += handler;
        //     // Wait until OnLevelCleared is triggered
        //     while (!levelCleared)
        //         yield return null;
        //     // Wait 2 seconds after level clear
        //     yield return new WaitForSeconds(2f);
        //     if (GameEvents.instance != null)
        //         GameEvents.instance.OnLevelCleared -= handler;
        // }
        // Now show the add card UI sequence
        yield return StartCoroutine(AddCardObjectsCoroutine(cardPrefabs, waitTime));
    }

    // --- Select Card Sequence with Battle State Check ---
    private System.Collections.IEnumerator SelectCardObjectsSequence(List<GameObject> cards, bool addToHand, float waitTime, System.Action<GameObject> onSelected)
    {
        // if (CombatManager.isInBattle)
        // {
        //     bool levelCleared = false;
        //     System.Action handler = () => { levelCleared = true; };
        //     if (GameEvents.instance != null)
        //         GameEvents.instance.OnLevelCleared += handler;
        //     while (!levelCleared)
        //         yield return null;
        //     yield return new WaitForSeconds(2f);
        //     if (GameEvents.instance != null)
        //         GameEvents.instance.OnLevelCleared -= handler;
        // }
        yield return StartCoroutine(SelectCardObjectsCoroutine(cards, addToHand, waitTime, onSelected));
    }

    /// <summary>
    ///  Adds multiple card GameObjects to the hand area, handling instancing and animated placement.
    ///  Now private, use QueueAddCardObjects instead.
    /// </summary>
    private System.Collections.IEnumerator AddCardObjectsCoroutine(List<GameObject> cardPrefabs, float waitTime = 1f)
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

        // 2. Check layout group
        if (horizontalLayoutGroupAdd == null)
        {
            Debug.LogError("CardManager: horizontalLayoutGroupAdd not assigned!");
            yield break;
        }

        // 3. Instantiate all cards and add to layout group
        List<GameObject> newCards = new List<GameObject>();
        List<CardMaster> cardMasters = new List<CardMaster>();
        foreach (var prefab in cardPrefabs)
        {
            GameObject newCard = Instantiate(prefab, horizontalLayoutGroupAdd);
            newCard.transform.SetAsLastSibling();
            if (newCard.TryGetComponent<CardMaster>(out var cardMaster))
            {
                cardMasters.Add(cardMaster);
            }
            newCards.Add(newCard);

            // DebriManager.ScatterUIPixels at position of each card's position by passing its UI anchored position (local to parent)
            if (newCard.TryGetComponent<RectTransform>(out var cardRect))
            {
                DebriManager.ScatterUIPixels(cardRect);
            }
        }

        // 4. Make cards stay at the center for waitTime to let players read cards
        yield return StartCoroutine(CardPromptSequenceMultiple(newCards, cardMasters, waitTime));
    }

    // Helper coroutine for multiple cards
    private System.Collections.IEnumerator CardPromptSequenceMultiple(List<GameObject> newCards, List<CardMaster> cardMasters, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        bool finished = false;
        System.Action onFinish = () => { finished = true; };
        // Start the finish coroutine and wait for it to complete (veil hidden)
        yield return StartCoroutine(FinishCardPromptMultipleWithCallback(newCards, cardMasters, GetLayoutRectTransform(), onFinish));
        while (!finished)
            yield return null;
    }

    // Helper: get the layout rect transform if available
    private RectTransform GetLayoutRectTransform()
    {
        return horizontalLayoutGroupAdd != null ? horizontalLayoutGroupAdd.GetComponent<RectTransform>() : null;
    }

    // Helper: Finish and signal completion (waits for veil to be hidden)
    private System.Collections.IEnumerator FinishCardPromptMultipleWithCallback(List<GameObject> newCards, List<CardMaster> cardMasters, RectTransform cardHolderRT, System.Action onFinish)
    {
        yield return StartCoroutine(FinishCardPromptMultiple(newCards, cardMasters, cardHolderRT));
        // Wait for veil to be fully hidden (CanvasGroup alpha == 0 or inactive)
        if (cardPromptVeilAdd != null)
        {
            var cg = cardPromptVeilAdd.GetComponent<CanvasGroup>();
            float timeout = 2f; // safety timeout
            float t = 0f;
            while (cardPromptVeilAdd.gameObject.activeSelf && (cg == null || cg.alpha > 0.01f))
            {
                yield return null;
                t += Time.unscaledDeltaTime;
                if (t > timeout) break;
            }
        }
        onFinish?.Invoke();
    }

    private System.Collections.IEnumerator FinishCardPromptMultiple(List<GameObject> newCards, List<CardMaster> cardMasters, RectTransform cardHolderRT)
    {
        yield return new WaitForSeconds(0.1f);
        if (cardMasters != null && handArea != null)
        {
            foreach (var cardMaster in cardMasters)
            {
                if (cardMaster != null)
                {
                    var rt = cardMaster.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.SetParent(handArea.rectTransform, true);
                    handArea.MoveCardToHand(cardMaster, null);
                }
            }
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
        return CardDatabase.GetCard(cardId);
    }
}
