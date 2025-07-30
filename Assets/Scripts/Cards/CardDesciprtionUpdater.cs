
using UnityEngine;
using TMPro;
using GLTFast.Schema;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardDesciprtionUpdater : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    private CardMaster cardMaster;

    [Header("UI References")]
    public TMP_Text headingText;
    public TMP_Text descriptionText;
    public TMP_Text cardBondText;
    public TMP_Text cardModifiableValuesText;
    public TMP_Text cardRarityText;
    public TMP_Text cardIdFooter;
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

    [Header("Tags Tansform")]
    public Transform tagsTransform; // Parent for tags visuals
    public GameObject tagPrefab; // Prefab for each tag visual

    [System.Serializable]
    public class KeywordColorPair
    {
        public GameSettings.Keyword keyword;
        public Color color = Color.white;
    }

    [Header("Keyword Colors")]
    public List<KeywordColorPair> keywordColors = new List<KeywordColorPair>();

    // --- Hover tip logic ---
    private Coroutine hoverTipCoroutine;
    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (hoverTipCoroutine != null) StopCoroutine(hoverTipCoroutine);
        hoverTipCoroutine = StartCoroutine(ShowCardTipsAfterDelay(1f));
    }
    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (hoverTipCoroutine != null)
        {
            StopCoroutine(hoverTipCoroutine);
            hoverTipCoroutine = null;
        }
        CanvasManager.HideTip();
    }
    private System.Collections.IEnumerator ShowCardTipsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowCardTips();
    }
    private void ShowCardTips()
    {
        if (cardMaster == null) return;
        // Show tips for additional_mousetips
        if (cardMaster.additional_mousetips != null)
        {
            foreach (var keyword in cardMaster.additional_mousetips)
            {
                CanvasManager.ShowTip(keyword);
            }
        }
        // Show tips for card_conditions
        if (cardMaster.card_conditions != null)
        {
            foreach (var cond in cardMaster.card_conditions)
            {
                var keyword = GameSettings.GetKeyWord(cond);
                CanvasManager.ShowTip(keyword);
            }
        }
    }

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
        // Write card ID to footer if assigned
        if (cardIdFooter != null && cardMaster != null)
        {
            cardIdFooter.text = "ID: " + cardMaster.card_id;
        }

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

        // add card rarity and card type to keywords
        if (cardMaster != null)
        {
            if (!cardMaster.additional_tags.Contains(GameSettings.GetKeyWord(cardMaster.card_rarity)))
                cardMaster.additional_tags.Add(GameSettings.GetKeyWord(cardMaster.card_rarity));
            if (!cardMaster.additional_tags.Contains(GameSettings.GetKeyWord(cardMaster.card_type)))
                cardMaster.additional_tags.Add(GameSettings.GetKeyWord(cardMaster.card_type));
        }

        // --- Create tag prefabs for additional_keywords ---
        if (tagsTransform != null && tagPrefab != null && cardMaster != null && cardMaster.additional_tags != null)
        {
            // Remove old tags
            for (int i = tagsTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(tagsTransform.GetChild(i).gameObject);
            }
            foreach (var keyword in cardMaster.additional_tags)
            {
                var go = Instantiate(tagPrefab, tagsTransform);
                // Find TMP_Text in children
                var tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = GameSettings.LocalizeText(GameSettings.GetKeywordTip(keyword).tipTitle);
                }
                // Find Image in children and set color
                var img = go.GetComponentInChildren<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = GetKeywordColor(keyword);
                }
            }
        }
        // headingText.text = cardMaster.GetName();
        // descriptionText.text = cardMaster.GetDescription();

        UpdateTexts();
    }

    // Returns a color for a given keyword (customize as needed)
    private Color GetKeywordColor(GameSettings.Keyword keyword)
    {
        if (keywordColors != null)
        {
            var pair = keywordColors.Find(x => x.keyword == keyword);
            if (pair != null)
                return pair.color;
        }
        return Color.white;
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
                // Remove all current condition visuals
                for (int i = conditionVisuals.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(conditionVisuals.transform.GetChild(i).gameObject);
                }
                // Recreate visuals for each condition in order
                foreach (var cond in cardMaster.card_conditions)
                {
                    if (_conditionPrefabDict.TryGetValue(cond, out var prefab) && prefab != null)
                    {
                        var go = Instantiate(prefab, conditionVisuals.transform);
                        var holder = go.GetComponent<ConditionVisualHolder>();
                        if (holder != null) holder.condition = cond;
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        UpdateTexts();
    }
}



