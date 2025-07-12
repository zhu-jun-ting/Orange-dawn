using System.Collections.Generic;
using UnityEngine;

public class CardValueAddStar : CardMaster
{
    public override void OnCardEnable()
    {
        // For each uiStarPositions, use gridLocation as base and add offset, then apply all stat fields
        if (BoardArea.instance != null && uiStarPositions != null)
        {
            foreach (var offset in uiStarPositions)
            {
                int targetRow = gridLocation.x + offset.x;
                int targetCol = gridLocation.y + offset.y;
                var card = BoardArea.instance.GetCell(targetRow, targetCol);
                if (card != null)
                {
                    var valuePairs = new (NumberType, float)[] {
                        (NumberType.Damage, damage),
                        (NumberType.Health, health),
                        (NumberType.Probability, probability),
                        (NumberType.Amount, amount),
                        (NumberType.Mana, mana),
                        (NumberType.Speed, speed),
                        (NumberType.Time, time),
                        (NumberType.Coin, coin)
                    };
                    foreach (var (nType, nValue) in valuePairs)
                    {
                        if (Mathf.Abs(nValue) > 0.0001f)
                        {
                            card.UpdateNumberValue(nType, nValue, this);
                        }
                    }
                }
            }
        }
        base.OnCardEnable();
    }
}
