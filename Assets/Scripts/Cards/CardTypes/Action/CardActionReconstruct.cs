using System;
using System.Collections.Generic;
using UnityEngine;

// id: custom
// name: Reconstruct
// desc: When object in scene destroyed, Probability: 30% to create a bullet Accelerator or Cloner. Cost Health: 5
public class CardActionReconstruct : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Reconstruct Settings")]
    public List<GameObject> itemPrefabs; // Assign Accelerator and Cloner item prefabs in inspector
    public float spawnRadius = 1.5f;

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject += HandleOnDestroyObject;
            isSubscribed = true;
        }
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    public override void Reset()
    {
        base.Reset();
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnDestroyObject(Transform obj, GunBullet bullet)
    {
        if (obj == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!HealthBar.CanCostHealth(-(int)health)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, obj);
        GameEvents.instance.UpdateHealth(-(int)health);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0) return;
        int idx = UnityEngine.Random.Range(0, itemPrefabs.Count);
        GameObject prefab = itemPrefabs[idx];
        SpawnObjects(
            _prefab: prefab,
            _count: 1,
            _position: target != null ? target.position : Vector3.zero,
            _radius: spawnRadius,
            _modifyObject: (obj) => { /* Optionally set properties here */ }
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)health));
    }
}
