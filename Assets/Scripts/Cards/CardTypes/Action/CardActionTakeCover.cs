using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

    // id: 310
    // name: Take Cover
    // desc: When you hit any object in scene, Probability: 10% to spawn a crate around
    // Amount: 1

public class CardActionTakeCover : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Fragile Wall Settings")]
    public GameObject fragileWallPrefab; // Assign in inspector
    public float spawnRadius = 2f;


    private float lastWallSpawnTime = -10f;
    public void TriggerAction(CardMaster source = null, Transform location = null)
    {
        // location: where to spawn the walls (center)
        if (fragileWallPrefab == null || location == null) return;
        lastWallSpawnTime = Time.time;

        List<GameObject> walls = SpawnObjects(fragileWallPrefab, (int)amount, location.position, Quaternion.identity, spawnRadius);

    }

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall += HandleOnHitWall;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private void HandleOnHitWall(GunBullet bullet, Vector2 hitPosition, GameObject wall)
    {
        if (bullet == null) return;
        if (Time.time - lastWallSpawnTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;

        OnTrigger?.Invoke(this, CreateTempTransformAt(hitPosition));

        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)amount, (int)mana));
    }

}
