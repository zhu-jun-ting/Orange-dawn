using System.Collections.Generic;
using UnityEngine;

public class CardValueAddStar : CardMaster
{
    [Header("General Settings")]
    public List<CardMaster.NumberType> numberTypes = new List<CardMaster.NumberType>();
    public List<float> numberValues = new List<float>();
    private List<float> defaultNumberValues = new List<float>();

    protected override void Awake()
    {
        base.Awake();
        defaultNumberValues = new List<float>(numberValues);
        numberTypesCanBeModified = new List<CardMaster.NumberType>(numberTypes);
    }

    public override void OnCardEnable()
    {
        // For each uiStarPositions, use gridLocation as base and add offset, then apply all values
        if (BoardArea.instance != null && uiStarPositions != null)
        {
            foreach (var offset in uiStarPositions)
            {
                int targetRow = gridLocation.x + offset.x;
                int targetCol = gridLocation.y + offset.y;
                var card = BoardArea.instance.GetCell(targetRow, targetCol);
                if (card != null)
                {
                    for (int i = 0; i < numberTypes.Count && i < numberValues.Count; i++)
                    {
                        var nType = numberTypes[i];
                        var nValue = numberValues[i];
                        if (card.card_type == CardType.Gun)
                        {
                            CardMaster.OnApplyValuesToGuns += () => card.UpdateNumberValue(nType, nValue, instance);
                        }
                        else
                        {
                            card.UpdateNumberValue(nType, nValue, instance);
                        }
                    }
                }
            }
        }
        base.OnCardEnable();
    }

    public override void Reset()
    {
        for (int i = 0; i < numberValues.Count && i < defaultNumberValues.Count; i++)
        {
            numberValues[i] = defaultNumberValues[i];
        }
        base.Reset();
    }

    public override string GetDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("update STAR values");
        for (int i = 0; i < numberTypes.Count && i < numberValues.Count; i++)
        {
            sb.AppendFormat("{0}: {1}", numberTypes[i], numberValues[i]);
            if (i < numberTypes.Count - 1) sb.Append("\n");
        }
        return GameSettings.AddIcon(sb.ToString());
    }

    public override bool UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        bool updated = false;
        for (int i = 0; i < numberTypes.Count && i < numberValues.Count; i++)
        {
            if (numberTypes[i] == numberType)
            {
                numberValues[i] += value;
                updated = true;
            }
        }
        return updated;
    }

    public override bool UpdateSelfNumberValue(CardMaster.NumberType numberType, float value, bool isPermanent = false)
    {
        base.UpdateSelfNumberValue(numberType, value, isPermanent);
        bool updated = false;
        for (int i = 0; i < numberTypes.Count && i < numberValues.Count; i++)
        {
            if (numberTypes[i] == numberType)
            {
                numberValues[i] += value;
                if (isPermanent && i < defaultNumberValues.Count)
                {
                    defaultNumberValues[i] += value;
                }
                updated = true;
            }
        }
        return updated;
    }

    public override UIStar.StarType GetStarType(CardMaster cardMaster = null)
    {
        if (cardMaster == null)
            return UIStar.StarType.White;
        if (cardMaster.numberTypesCanBeModified != null && cardMaster.numberTypesCanBeModified.Count > 0)
        {
            foreach (var nType in numberTypes)
            {
                if (cardMaster.numberTypesCanBeModified.Contains(nType))
                    return UIStar.StarType.Yellow;
            }
        }
        return UIStar.StarType.White;
    }
}
