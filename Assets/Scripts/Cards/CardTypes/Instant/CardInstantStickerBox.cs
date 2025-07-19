using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and spawns a random box with 30% probability when triggered.
/// </summary>
public class CardInstantStickerBox : CardInstantStickerMaster
{
    [Header("Sticker Box Settings")]
    [Range(0f, 1f)] public float spawnProbability = 0.3f;

    [Header("Buff Entry Text")]
    public string boxBuffName = "Box Sticker";
    [TextArea(3, 10)]
    public string boxBuffDescription = "When this Action Card triggers, Probability: {0} to spawn a random box.";

    public override void ActionOnTrigger(CardMaster card, Transform location)
    {
        if (UnityEngine.Random.value > spawnProbability) return;
        GameObject boxPrefab = ItemDatabase.GetRandomItem(item => item.itemType == ItemMaster.ItemType.Box);
        if (boxPrefab != null && location != null)
        {
            SpawnObjects(
                _prefab: boxPrefab,
                _count: 1,
                _position: location.position,
                _radius: 1f,
                _modifyObject: (obj) =>
                {
                    // Optionally modify the spawned box object here if needed
                }
            );
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(boxBuffDescription, (int) (spawnProbability * 100)));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(boxBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(boxBuffDescription, (int) (spawnProbability * 100)));
    }
}
