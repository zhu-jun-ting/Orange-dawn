using System;
using UnityEngine;
using System.Collections.Generic;

// id: custom
// name: Revolver
// desc: When you dodge, Probability: 40% shoot Amount: 1 additional pinball. Cost Health: 5
public class CardActionRevolver : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnPlayerDodge += HandleOnPlayerDodge;
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
            GameEvents.instance.OnPlayerDodge -= HandleOnPlayerDodge;
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
            GameEvents.instance.OnPlayerDodge -= HandleOnPlayerDodge;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnPlayerDodge(Transform player)
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!HealthBar.CanCostHealth(-(int)health)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, player);
        GameEvents.instance.UpdateHealth(-(int)health);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        // Select a random gun from PlayerController.instance.guns and fire with modified bulletNum
        if (PlayerController.instance == null || PlayerController.instance.guns == null || PlayerController.instance.guns.Count == 0) return;
        var gunObjs = PlayerController.instance.guns;
        GameObject selectedGunObj = gunObjs[UnityEngine.Random.Range(0, gunObjs.Count)];
        Gun selectedGun = selectedGunObj.GetComponent<Gun>();
        if (selectedGun == null) return;
        int originalBulletNum = selectedGun.bulletNum;
        selectedGun.bulletNum = (int)amount;
        selectedGun.Fire();
        selectedGun.bulletNum = originalBulletNum;
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)amount, (int)health));
    }
}
