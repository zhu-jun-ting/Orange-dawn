using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// CardGunGoldenGun drops additional gold when killing enemies
// damage use gun.damage
// probability determines chance to drop additional coin (5%)
// Uses GameEvents.PawnDie to detect enemy deaths and drops coin via CombatManager.SpawnDrop

public class CardGunGoldenGun : CardMaster
{
    public GameObject gunPrefab;
    public Sprite bulletSprite;

    // Gold drop parameters
    public int goldDropAmount = 1; // Amount of gold to drop

    private PlayerController player;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
        player = PlayerController.instance;
        if (player == null || gunPrefab == null) return;

        // Instantiate and parent under player, set inactive
        var go = Instantiate(gunPrefab, player.transform);
        go.SetActive(false);
        current_gun = go.GetComponent<Gun>();
        current_gun.owner = player.gameObject;

        if (!PlayerController.instance.guns.Contains(current_gun.gameObject)) PlayerController.instance.guns.Add(current_gun.gameObject);
        if (bulletSprite != null && current_gun != null)
        {
            current_gun.SetBulletSprite(bulletSprite);
        }
    }

    public override void OnCardEnable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(true);
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
        CardMaster.OnApplyValuesToGuns += HandleOnApplyValuesToGuns;
        
        // Subscribe to pawn die event
        GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        GameEvents.instance.OnPawnDie += HandleOnPawnDie;
        
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(false);
        
        // Unsubscribe from pawn die event
        GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        
        base.OnCardDisable();
    }

    public override bool OnCardDestroyed()
    {
        if (current_gun != null)
        {
            Destroy(current_gun.gameObject);
            current_gun = null;
            return true;
        }
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        if (current_gun != null)
        {
            return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), (int)damage, probability));
        }
        return "";
    }

    public void HandleOnApplyValuesToGuns()
    {
        // apply the value changes to player
        current_gun.damage += (int)damage;
    }

    public void HandleOnPawnDie(PawnMaster deadPawn_, float damageDealt_, GameObject instigator_, GameEvents.DamageType damage_type_, Gun source_)
    {
        // Check if the dead pawn is an enemy
        EnemyMaster enemy = deadPawn_ as EnemyMaster;
        if (enemy == null) return;

        // Check if this kill was from our gun
        if (source_ != current_gun) return;

        // Check probability to drop additional gold
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > probability) return;

        // Drop additional gold at enemy position
        if (CombatManager.instance != null)
        {
            CombatManager.instance.SpawnDrop(CombatManager.DropItem.Coin, enemy.transform, goldDropAmount);
        }
    }

    public override void Reset()
    {
        if (current_gun != null) current_gun.Reset();
        if (current_gun != null) current_gun.gameObject.SetActive(false);
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
        GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        base.Reset(); // Call the base reset method to reset other properties
    }
}
