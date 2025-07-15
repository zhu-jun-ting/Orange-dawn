using System.Collections;
using UnityEngine;

public class CardActionBulletAoe : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;

    [Header("Bullet AOE Settings")]
    public float AoeDuration = 3f; 
    public float AoeSize = 1f; 


    // --- Common for all action cards ---
    private float lastAoeTime = -10f;
    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall += HandleOnHitWall;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }
    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, damage, (int)mana));
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
    }



    // --- Trigger Action Logic ---
    private GunBullet bullet;
    private void HandleOnHitWall(GunBullet _bullet, Vector2 hitPosition, GameObject wall)
    {
        if (_bullet == null || _bullet.Aoe == null) return;
        if (Time.time - lastAoeTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability) return; // Always triggers, but keep for extensibility

        bullet = _bullet; // Store the bullet reference
        OnTrigger?.Invoke(this, bullet.transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        if (bullet != null && bullet.IsAoeActive()) return; // Already active, do nothing

        lastAoeTime = Time.time;
        if (target != null && target.TryGetComponent<GunBullet>(out var localBullet))
        {
            localBullet.AoeDamage = damage;
            localBullet.SetAoeSize(AoeSize);
            localBullet.SetAoe(true);
            GameEvents.instance.UpdateMana(-(int)mana);
            localBullet.StartCoroutine(DisableAoeAfterDuration(localBullet));
        }
    }

    private IEnumerator DisableAoeAfterDuration(GunBullet bullet)
    {
        yield return new WaitForSeconds(AoeDuration);
        if (bullet != null)
            bullet.SetAoe(false);
    }


}
