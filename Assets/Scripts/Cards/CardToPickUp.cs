using UnityEngine;
using UnityEngine.UI;

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

    void Awake()
    {
        if (hintUIChild != null)
        {
            hintUIChild.SetActive(false);
            var text = hintUIChild.GetComponentInChildren<Text>();
            if (text != null)
                text.text = "Press F to pickup";
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideHint();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
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
            bool added = cardMaster.TryPurchaseCard();
            if(added) Destroy(gameObject);
        }
        
    }
}
