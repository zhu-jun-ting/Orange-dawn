using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// CardGunGlueGun applies slow effect to enemies hit
// damage use gun.damage
// probability determines chance to apply slow effect (5%)
// Uses GameEvents.OnHitPawn to detect hits and applies slow via EnemyMaster.AddDot

public class CardGunGlueGun : CardMaster
{
    public GameObject gunPrefab;
    public Sprite bulletSprite;

    // Slow effect parameters
    public float slowDotDamage = 0f; // Slow doesn't deal damage
    public float slowDotInterval = 0.5f;
    public float slowDotDuration = 1f;

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
        
        // Subscribe to hit pawn event
        GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        GameEvents.instance.OnHitPawn += HandleOnHitPawn;
        
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(false);
        
        // Unsubscribe from hit pawn event
        GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        
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

    public void HandleOnHitPawn(float damage_, PawnMaster reciever_, GameObject instigator_, GameEvents.DamageType damage_type_, Transform location_, float hit_back_factor_, Gun source_)
    {
        // Check if this hit was from our gun
        if (source_ != current_gun) return;

        // Check if the receiver is an enemy
        EnemyMaster enemy = reciever_ as EnemyMaster;
        if (enemy == null) return;

        // Check probability to apply slow effect
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > probability) return;

        // Apply slow effect via AddDot
        enemy.AddDot(
            EnemyMaster.DotType.Slow,
            slowDotDamage,
            slowDotInterval,
            slowDotDuration,
            null, // Use default FX name
            false // Not stackable
        );
    }

    public override void Reset()
    {
        if (current_gun != null) current_gun.Reset();
        if (current_gun != null) current_gun.gameObject.SetActive(false);
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
        GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        base.Reset(); // Call the base reset method to reset other properties
    }
}
