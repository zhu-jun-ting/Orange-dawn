using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and spawns a random ground trap with 30% probability when triggered.
/// </summary>
public class CardInstantStickerTrap : CardInstantStickerMaster
{
    [Header("Sticker Trap Settings")]
    [Range(0f, 1f)] public float spawnProbability = 0.3f;

    [Header("Buff Entry Text")]
    public string trapBuffName = "Trap Sticker";
    [TextArea(3, 10)]
    public string trapBuffDescription = "When this Action Card triggers, Probability: {0} to spawn a random ground trap.";

    public override void ActionOnTrigger(CardMaster card, Transform location)
    {
        if (UnityEngine.Random.value > spawnProbability) return;
        GameObject trapPrefab = ItemDatabase.GetRandomItem(item => item.itemType == ItemMaster.ItemType.Trigger);
        if (trapPrefab != null && location != null)
        {
            SpawnObjects(
                _prefab: trapPrefab,
                _count: 1,
                _position: location.position,
                _radius: 1f,
                _modifyObject: (obj) =>
                {
                    // Optionally modify trap object here if needed
                }
            );
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(trapBuffDescription, (int) (spawnProbability * 100)));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(trapBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(trapBuffDescription, (int) (spawnProbability * 100)));
    }
}
