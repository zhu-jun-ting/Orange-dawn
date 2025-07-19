using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and calls a lightning FX with 30% probability when triggered.
/// </summary>
public class CardInstantStickerLightning : CardInstantStickerMaster
{
    [Header("Sticker Lightning Settings")]
    [Range(0f, 1f)] public float spawnProbability = 0.2f;
    public GameObject fxLightningPrefab; // Assign in inspector
    public float aoeRange = 2f;
    public float aoeDamage = 12f;

    [Header("Buff Entry Text")]
    public string lightningBuffName = "Lightning Sticker";
    [TextArea(3, 10)]
    public string lightningBuffDescription = "When this Action Card triggers, Probability: {0} to call a lightning that deals Damage: {1}.";

    public override void ActionOnTrigger(CardMaster card, Transform target)
    {
        if (UnityEngine.Random.value > spawnProbability) return;
        if (fxLightningPrefab == null || target == null) return;
        GameObject fx = GameObject.Instantiate(fxLightningPrefab, target.position, Quaternion.identity);
        FxLightning fxScript = fx.GetComponentInChildren<FxLightning>();
        if (fxScript != null)
        {
            fxScript.SpawnAt(target.position, aoeRange, aoeDamage);
        }
        GameObject.Destroy(fx, 2f); // Destroy FX after 2 seconds
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(lightningBuffDescription, (int) (spawnProbability * 100), aoeDamage));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(lightningBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(lightningBuffDescription, (int) (spawnProbability * 100), aoeDamage));
    }
}
