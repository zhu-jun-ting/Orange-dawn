using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class CardValueAdd : CardMaster
{

    [Header("General Settings")]
    public List<CardMaster.NumberType> numberTypes = new List<CardMaster.NumberType>();
    public List<float> numberValues = new List<float>();
    private List<float> defaultNumberValues = new List<float>();

    protected override void Awake()
    {
        base.Awake();
        // Store default values for reset
        defaultNumberValues = new List<float>(numberValues);
        // Sync numberTypesCanBeModified to match numberTypes
        numberTypesCanBeModified = new List<CardMaster.NumberType>(numberTypes);
    }

    public override void OnCardEnable()
    {
        current_gun = null;

        // Call UpdateNumberValue on all linked cards for each value type
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        for (int i = 0; i < numberTypes.Count && i < numberValues.Count; i++)
        {
            var nType = numberTypes[i];
            var nValue = numberValues[i];
            foreach (var link in linked)
            {
                if (link != null)
                {
                    if (link.card_type == CardType.Gun)
                    {
                        // If the link is a gun card, apply at the end
                        CardMaster.OnApplyValuesToGuns += () => link.UpdateNumberValue(nType, nValue, instance);
                    }
                    else
                    {
                        link.UpdateNumberValue(nType, nValue, instance);
                    }
                }
            }
        }

        base.OnCardEnable();
    }


    public override void OnCardDisable()
    {
        base.OnCardDisable(); // Call the base method to clear the current gun reference
    }

    public override void Reset()
    {
        // Reset all values to default
        for (int i = 0; i < numberValues.Count && i < defaultNumberValues.Count; i++)
        {
            numberValues[i] = defaultNumberValues[i];
        }
        base.Reset(); // Call the base reset method to reset other properties
    }

    // return the formatted description of the card
    public override string GetDescription()
    {
        // Build a description line for each value
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
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

        // Add value to the matching numberType
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
            // Highlight if any of the types are present
            foreach (var nType in numberTypes)
            {
                if (cardMaster.numberTypesCanBeModified.Contains(nType))
                    return UIStar.StarType.Yellow;
            }
        }
        return UIStar.StarType.White;
    }
}
