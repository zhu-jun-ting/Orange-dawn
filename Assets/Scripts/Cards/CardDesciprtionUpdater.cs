using UnityEngine;
using TMPro;
using GLTFast.Schema;

public class CardDesciprtionUpdater : MonoBehaviour
{

    private CardMaster cardMaster;

    [Header("UI References")]
    public TMP_Text headingText;
    public TMP_Text descriptionText;
    public TMP_Text cardBondText;
    public TMP_Text cardModifiableValuesText;
    public TMP_Text cardRarityText;
    public UnityEngine.UI.Image backgroundImage;




    // --- Add: Mouse events for cardBondText tip ---
    private void Start()
    {
        // Register mouse events for cardBondText if assigned
        if (cardBondText != null)
        {
            var trigger = cardBondText.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = cardBondText.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // Pointer Enter
            var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((ev) => ShowCardBondTip());
            trigger.triggers.Add(entryEnter);

            // Pointer Exit
            var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((ev) => HideCardBondTip());
            trigger.triggers.Add(entryExit);
        }
    }

    private void ShowCardBondTip()
    {
        if (cardMaster != null && cardMaster.card_bonds != null && cardMaster.card_bonds.Count > 0)
        {
            string bondList = string.Join(", ", cardMaster.card_bonds.ConvertAll(b => b.ToString()));
            CanvasManager.ShowTip("Card Bond", bondList);
        }
    }

    private void HideCardBondTip()
    {
        CanvasManager.HideTip();
    }




    private void OnEnable()
    {
        cardMaster = GetComponent<CardMaster>();
        if (!CombatManager.is_update_card_registered)
        {
            CardMaster.OnUpdateCardTexts += UpdateAllCardTexts;
            CombatManager.is_update_card_registered = true;
        }
        UpdateTexts();
    }

    private void OnDisable()
    {
        if (CombatManager.is_update_card_registered)
        {
            CardMaster.OnUpdateCardTexts -= UpdateAllCardTexts;
            CombatManager.is_update_card_registered = false;
        }
    }

    private void UpdateAllCardTexts()
    {
        // Find all CardMasterHolder components in the scene and update their texts
        var holders = FindObjectsByType<CardDesciprtionUpdater>(FindObjectsSortMode.None);
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
                headingText.text = cardMaster.GetName();
            if (descriptionText != null)
                descriptionText.text = cardMaster.GetDescription();

            // Update cardBondText with all caps CardBond names
            if (cardBondText != null)
            {
                string bondText = "";
                if (cardMaster.card_bonds != null && cardMaster.card_bonds.Count > 0)
                {
                    bondText = string.Join(" ", cardMaster.card_bonds.ConvertAll(b => b.ToString().ToUpper()));
                }
                cardBondText.text = GameSettings.AddIcon(bondText);

                // // Add mouse enter/exit events for tip
                // var trigger = cardBondText.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                // if (trigger == null)
                // {
                //     trigger = cardBondText.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                // }
                // trigger.triggers ??= new System.Collections.Generic.List<UnityEngine.EventSystems.EventTrigger.Entry>();
                // trigger.triggers.Clear();

                // // PointerEnter
                // var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry
                // {
                //     eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
                // };
                // entryEnter.callback.AddListener((_) => {
                //     string tipName = "Card Bond";
                //     string tipDesc = bondText;
                //     CanvasManager.ShowTip(tipName, tipDesc);
                // });
                // trigger.triggers.Add(entryEnter);

                // // PointerExit
                // var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry
                // {
                //     eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
                // };
                // entryExit.callback.AddListener((_) => {
                //     CanvasManager.HideTip();
                // });
                // trigger.triggers.Add(entryExit);
            }

            // Update cardModifiableValuesText with all caps NumberType names
            if (cardModifiableValuesText != null)
            {
                string modText = "";
                if (cardMaster.numberTypesCanBeModified != null && cardMaster.numberTypesCanBeModified.Count > 0)
                {
                    modText = string.Join(" ", cardMaster.numberTypesCanBeModified.ConvertAll(n => n.ToString().ToUpper()));
                }
                cardModifiableValuesText.text = GameSettings.AddIcon(modText);
            }

            // Update cardRarityText with all caps CardRarity name
            if (cardRarityText != null)
            {
                string rarityText = cardMaster.card_rarity.ToString().ToUpper();
                cardRarityText.text = GameSettings.AddIcon(rarityText);
            }

            // Set background color based on first CardBond (if any)
            if (backgroundImage != null && cardMaster.card_bonds != null && cardMaster.card_bonds.Count > 0 && GameSettings.instance != null)
            {
                Color? bondColor = null;
                switch (cardMaster.card_bonds[0])
                {
                    case CardMaster.CardBond.Mech:
                        bondColor = GameSettings.instance.mechColor;
                        break;
                    case CardMaster.CardBond.Skull:
                        bondColor = GameSettings.instance.skullColor;
                        break;
                    case CardMaster.CardBond.Human:
                        bondColor = GameSettings.instance.humanColor;
                        break;
                }
                if (bondColor.HasValue)
                {
                    // backgroundImage is declared as Image, so set color directly
                    backgroundImage.color = bondColor.Value;
                }
            }
        }
    }

    private void OnValidate()
    {
        UpdateTexts();
    }
}



