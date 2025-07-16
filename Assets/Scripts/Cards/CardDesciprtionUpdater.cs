using UnityEngine;
using TMPro;
using GLTFast.Schema;
using System.Collections.Generic;
using UnityEngine.UI;

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
    public List<Transform> woodVisuals; // List of card links to update
    public List<Transform> ironVisuals; // List of card links to update

    [Header("Condition Visuals")]
    public VerticalLayoutGroup conditionVisuals; // Parent for condition visuals
    [System.Serializable]
    public class ConditionPrefabPair
    {
        public CardMaster.CardCondition condition;
        public GameObject prefab;
    }

    public List<ConditionPrefabPair> conditionPrefabList;
    private Dictionary<CardMaster.CardCondition, GameObject> _conditionPrefabDict;
    private void Awake()
    {
        // Build the dictionary from the list
        _conditionPrefabDict = new Dictionary<CardMaster.CardCondition, GameObject>();
        if (conditionPrefabList != null)
        {
            foreach (var pair in conditionPrefabList)
            {
                if (!_conditionPrefabDict.ContainsKey(pair.condition) && pair.prefab != null)
                    _conditionPrefabDict.Add(pair.condition, pair.prefab);
            }
        }
    }




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

            // Show/hide visuals based on draggable
            if (woodVisuals != null)
            {
                foreach (var t in woodVisuals)
                {
                    if (t != null) t.gameObject.SetActive(!cardMaster.card_conditions.Contains(CardMaster.CardCondition.IsUndraggable));
                }
            }
            if (ironVisuals != null)
            {
                foreach (var t in ironVisuals)
                {
                    if (t != null) t.gameObject.SetActive(!!cardMaster.card_conditions.Contains(CardMaster.CardCondition.IsUndraggable));
                }
            }
        

            // --- Update card conditions UI ---
            if (conditionVisuals != null && _conditionPrefabDict != null && cardMaster.card_conditions != null)
            {
                // Build a list of current drawn conditions and their GameObjects
                var drawn = new List<(CardMaster.CardCondition, GameObject)>();
                for (int i = 0; i < conditionVisuals.transform.childCount; i++)
                {
                    var go = conditionVisuals.transform.GetChild(i).gameObject;
                    var holder = go.GetComponent<ConditionVisualHolder>();
                    if (holder != null)
                        drawn.Add((holder.condition, go));
                }

                // Remove visuals not in card_conditions
                for (int i = drawn.Count - 1; i >= 0; i--)
                {
                    if (!cardMaster.card_conditions.Contains(drawn[i].Item1))
                        Destroy(drawn[i].Item2);
                }

                // Insert or move visuals for each condition in order
                int insertIndex = 0;
                foreach (var cond in cardMaster.card_conditions)
                {
                    var existing = drawn.Find(x => x.Item1.Equals(cond));
                    if (existing.Item2 != null)
                    {
                        // Move to correct order if needed
                        if (existing.Item2.transform.GetSiblingIndex() != insertIndex)
                            existing.Item2.transform.SetSiblingIndex(insertIndex);
                    }
                    else if (_conditionPrefabDict.TryGetValue(cond, out var prefab) && prefab != null)
                    {
                        var go = Instantiate(prefab, conditionVisuals.transform);
                        go.transform.SetSiblingIndex(insertIndex);
                        var holder = go.GetComponent<ConditionVisualHolder>();
                        if (holder != null) holder.condition = cond;
                    }
                    insertIndex++;
                }
            }
        }
    }

    private void OnValidate()
    {
        UpdateTexts();
    }
}



