using UnityEngine;
using UnityEngine.UI;

public class CardCommon : MonoBehaviour
{
    public enum LinkType
    {
        Common,
        Value,
        Condition,
        Action,
    }

    [Header("Link Sprites (Order: Common, Value, Condition, Action)")]
    public Sprite commonLinkSprite;
    public Sprite valueLinkSprite;
    public Sprite conditionLinkSprite;
    public Sprite actionLinkSprite;

    [Header("Link GameObjects (Order: Up, Left, Right, Down)")]
    public GameObject upLinkGO;
    public GameObject leftLinkGO;
    public GameObject rightLinkGO;
    public GameObject downLinkGO;

    [Header("Layout for Buff Panel")]
    public GameObject buffPanelLayout;
    public GameObject buffEntryPrefab;

    private CardMaster cardMaster;

    void Awake()
    {
        cardMaster = GetComponent<CardMaster>();
    }

    void Start()
    {
        if (cardMaster == null) return;
        // Assign sprites based on link type for each direction
        SetLinkSprite(upLinkGO, cardMaster.up_link_type);
        SetLinkSprite(leftLinkGO, cardMaster.left_link_type);
        SetLinkSprite(rightLinkGO, cardMaster.right_link_type);
        SetLinkSprite(downLinkGO, cardMaster.down_link_type);
    }

    private void SetLinkSprite(GameObject linkGO, CardMaster.LinkType linkType)
    {
        if (linkGO == null) return;
        var img = linkGO.GetComponent<Image>();
        if (img == null) return;
        switch (linkType)
        {
            case CardMaster.LinkType.Common:
                img.sprite = commonLinkSprite;
                break;
            case CardMaster.LinkType.Value:
                img.sprite = valueLinkSprite;
                break;
            case CardMaster.LinkType.Condition:
                img.sprite = conditionLinkSprite;
                break;
            case CardMaster.LinkType.Action:
                img.sprite = actionLinkSprite;
                break;
        }
    }

    /// <summary>
    /// Adds a buff entry to the buff panel. If a buff with the same name exists, stacks it. Otherwise, instantiates a new entry and arranges by descending order.
    /// </summary>
    public void AddBuffDescription(string buffName, string buffDescription, int order = 0)
    {
        if (buffPanelLayout == null || buffEntryPrefab == null || string.IsNullOrEmpty(buffName)) return;
        
        // Search for existing buff entry by name
        BuffEntry existing = null;
        foreach (Transform child in buffPanelLayout.transform)
        {
            BuffEntry entry = child.GetComponent<BuffEntry>();
            if (entry != null && entry.name == buffName)
            {
                existing = entry;
                break;
            }
        }
        if (existing != null)
        {
            existing.StackBuff();
            return;
        }

        // Instantiate new buff entry
        GameObject newEntryGO = Instantiate(buffEntryPrefab, buffPanelLayout.transform);
        BuffEntry newEntry = newEntryGO.GetComponent<BuffEntry>();
        if (newEntry != null)
        {
            newEntry.name = buffName;
            newEntry.description = buffDescription;
            newEntry.order = order;
            newEntry.SetBuffName(buffName);
            newEntry.SetBuffDescription(buffDescription);
        }

        // Insert in descending order (greatest order at top)
        int insertIndex = 0;
        for (int i = 0; i < buffPanelLayout.transform.childCount; i++)
        {
            Transform child = buffPanelLayout.transform.GetChild(i);
            BuffEntry entry = child.GetComponent<BuffEntry>();
            if (entry != null && entry != newEntry && entry.order > order)
            {
                insertIndex = i + 1;
            }
        }
        newEntryGO.transform.SetSiblingIndex(insertIndex);
    }

}
