using UnityEngine;
using TMPro;

public class CardMasterHolder : MonoBehaviour
{
    [Header("Card Data Holder")]
    public CardMaster cardMaster;

    [Header("UI References")]
    public TMP_Text headingText;
    public TMP_Text descriptionText;

    void OnEnable()
    {
        if (cardMaster != null)
        {
            if (headingText != null)
                headingText.text = cardMaster.card_name;
            if (descriptionText != null)
                descriptionText.text = cardMaster.card_description;
        }
    }
}