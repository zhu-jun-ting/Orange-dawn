using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and spawns a health potion with 30% probability when triggered.
/// </summary>
public class CardInstantStickerAid : CardInstantStickerMaster
{
    [Header("Sticker Aid Settings")]
    [Range(0f, 1f)] public float spawnProbability = 0.3f;

    [Header("Buff Entry Text")]
    public string aidBuffName = "Aid Sticker";
    [TextArea(3, 10)]
    public string aidBuffDescription = "When this Action Card triggers, Probability: {0} to spawn a health potion nearby.";

    public override void ActionOnTrigger(CardMaster card, Transform target)
    {
        if (UnityEngine.Random.value > spawnProbability) return;
        if (target == null) return;
        if (CombatManager.instance != null) CombatManager.instance.SpawnDrop(CombatManager.DropItem.Health, target);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(aidBuffDescription, (int) (spawnProbability * 100)));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(aidBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(aidBuffDescription, (int) (spawnProbability * 100)));
    }
}
