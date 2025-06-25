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
}
