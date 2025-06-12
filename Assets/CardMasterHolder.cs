using UnityEngine;
using TMPro;

public class CardMasterHolder : MonoBehaviour
{
    [Header("Card Data Holder")]
    public CardMaster cardMaster;

    [Header("UI References")]
    public TMP_Text headingText;
    public TMP_Text descriptionText;

    private void OnEnable()
    {
        CardMaster.OnUpdateCardTexts += UpdateAllCardTexts;
        UpdateTexts();
    }

    private void OnDisable()
    {
        CardMaster.OnUpdateCardTexts -= UpdateAllCardTexts;
    }

    private void UpdateAllCardTexts()
    {
        // Find all CardMasterHolder components in the scene and update their texts
        var holders = FindObjectsOfType<CardMasterHolder>();
        foreach (var holder in holders)
        {
            holder.UpdateTexts();
        }
    }

    public void UpdateTexts()
    {
        if (cardMaster != null)
        {
            if (headingText != null)
                headingText.text = cardMaster.card_name;
            if (descriptionText != null)
                descriptionText.text = cardMaster.card_description;
            // Debug.Log(cardMaster.card_description);
        }
    }

    private void OnValidate()
    {
        UpdateTexts();
    }
}