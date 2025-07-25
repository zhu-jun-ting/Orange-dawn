using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Attach this to a world-space Canvas Image with a collider (2D or 3D).
/// Shows a hint when the player is in range and allows pickup with F.
/// </summary>
[RequireComponent(typeof(Collider2D))]

public class CardToPickUp : MonoBehaviour
{
    [Tooltip("Assign a child GameObject with a Text component for the pickup hint.")]
    public GameObject hintUIChild; // Assign in editor as a child object
    private bool playerInRange = false;
    public GameObject cardMasterGO; // Assign the CardMaster GameObject in the editor
    public Transform cardHolder;
    public TextMeshProUGUI pricetext; // Assign in editor if you want to use TextMeshPro for the hint

    // Animation fields (only one set, remove any duplicates below)
    private Vector3 originalScale;
    private Vector3 originalPosition;
    [Header("Card Highlight Animation")]
    public float highlightScale = 1.2f;
    public float highlightMoveY = 1.0f;
    public float tweenDuration = 0.2f;

    void Awake()
    {
        if (hintUIChild != null)
        {
            hintUIChild.SetActive(false);
        }
    }

    public void SetCard(GameObject card)
    {
        if (card != null)
        {
            cardMasterGO = card;
        }
        if (cardHolder != null)
        {
            // Destroy existing card if any
            if (cardHolder.childCount > 0)
            {
                for (int i = cardHolder.childCount - 1; i >= 0; i--)
                {
                    Destroy(cardHolder.GetChild(i).gameObject);
                }
            }
            // If 'card' is already a child of cardHolder, just move it
            if (card.transform.parent != cardHolder)
            {
                card.transform.SetParent(cardHolder);
            }
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.identity;
            card.transform.localScale = Vector3.one;

            // Set price text if possible
            if (pricetext != null)
            {
                var cardMaster = card.GetComponent<CardMaster>();
                if (cardMaster != null)
                {
                    pricetext.text = GameSettings.AddIcon(string.Format("Coin: {0}", cardMaster.card_cost.ToString()));
                }
                else
                {
                    pricetext.text = "?";
                }
            }
            // Disable all mouse interaction on the card and its children while in shop
            SetCardMouseInteractable(card, false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowHint();
            // Animate scale up and move up on card visual
            if (cardMasterGO != null)
            {
                originalScale = transform.localScale;
                originalPosition = transform.localPosition;
                cardMasterGO.transform.DOKill();
                cardMasterGO.transform.DOScale(originalScale * highlightScale, tweenDuration).SetEase(Ease.OutBack);
                cardMasterGO.transform.DOLocalMoveY(originalPosition.y + highlightMoveY, tweenDuration).SetEase(Ease.OutBack);
            }
            if (cardHolder != null)
            {
                var canvas = cardHolder.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder++;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideHint();
            // Animate scale down and move back on card visual
            if (cardMasterGO != null)
            {
                cardMasterGO.transform.DOKill();
                cardMasterGO.transform.DOScale(originalScale, tweenDuration).SetEase(Ease.InBack);
                cardMasterGO.transform.DOLocalMoveY(originalPosition.y, tweenDuration).SetEase(Ease.OutBack);
            }
            if (cardHolder != null)
            {
                var canvas = cardHolder.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder--;
                }
            }
        }
    }


    void Start()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnFKeyPressed += HandlePickupKey;
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnFKeyPressed -= HandlePickupKey;
    }

    private void HandlePickupKey()
    {
        if (!added && playerInRange)
        {
            PickupCard();
        }
    }

    void ShowHint()
    {
        if (hintUIChild != null)
            hintUIChild.SetActive(true);
    }

    void HideHint()
    {
        if (hintUIChild != null)
            hintUIChild.SetActive(false);
    }

    private bool added;
    void PickupCard()
    {
        HideHint();

        var cardMaster = cardMasterGO.GetComponent<CardMaster>();
        if (cardMaster == null)
        {
            Debug.LogError("CardMaster component is missing on the assigned GameObject in CardToPickUp.");
            return;
        }

        if (HandArea.instance != null)
        {
            added = cardMaster.TryPurchaseCard();
            if (added)
            {
                GetComponent<Collider2D>().enabled = false; // Disable collider to prevent further pickups
                pricetext.text = "Sold out"; // Clear price text
                // Enable mouse interaction on the card and its children after purchase
                SetCardMouseInteractable(cardMasterGO, true);
            }
            else
            {
                GameEvents.instance.ShowMessage("Not enough coins to purchase this card!");
            }
        }
        
    }
    /// <summary>
    /// Enables or disables all mouse interaction (raycast targets and colliders) on the card and its children.
    /// </summary>
    public static void SetCardMouseInteractable(GameObject card, bool interactable)
    {
        if (card == null) return;
        // Disable/enable all Unity UI raycast targets
        foreach (var grt in card.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
        {
            grt.raycastTarget = interactable;
        }
        // Disable/enable all 2D and 3D colliders
        foreach (var col in card.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = interactable;
        }
        foreach (var col2d in card.GetComponentsInChildren<Collider2D>(true))
        {
            col2d.enabled = interactable;
        }
        // Optionally, disable/enable EventTrigger or custom pointer handlers
        foreach (var et in card.GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>(true))
        {
            et.enabled = interactable;
        }
        // If you have custom pointer handler scripts, add them here as well
    }
}
