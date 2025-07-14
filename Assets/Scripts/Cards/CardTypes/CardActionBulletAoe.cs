using System.Collections;
using UnityEngine;

public class CardActionBulletAoe : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;

    [Header("Bullet AOE Settings")]
    public float AoeDamage = 5f; // Damage
    public float AoeDuration = 3f; // Time (seconds)
    public float AoeSize = 1f; // Probability (100% = 1.0)
    public int manaCost = 2; // Mana

    // Store initial values for reset
    private float _initAoeDamage;
    private float _initAoeDuration;
    private float _initAoeSize;
    private int _initManaCost;

    private float lastAoeTime = -10f;

    protected override void Awake()
    {
        base.Awake();
        _initAoeDamage = AoeDamage;
        _initAoeDuration = AoeDuration;
        _initAoeSize = AoeSize;
        _initManaCost = manaCost;
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

    private GunBullet bullet;
    private void HandleOnHitWall(GunBullet _bullet, Vector2 hitPosition, GameObject wall)
    {
        if (_bullet == null || _bullet.Aoe == null) return;
        if (Time.time - lastAoeTime < actionCooldown) return;
        if (UnityEngine.Random.value > triggerProbability) return; // Always triggers, but keep for extensibility

        bullet = _bullet; // Store the bullet reference
        OnTrigger?.Invoke(this, bullet.transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        if (!ManaBar.CanCostMana(-manaCost)) return;
        if (bullet != null && bullet.IsAoeActive()) return; // Already active, do nothing

        lastAoeTime = Time.time;
        if (target != null && target.TryGetComponent<GunBullet>(out var localBullet))
        {
            localBullet.AoeDamage = AoeDamage;
            localBullet.SetAoeSize(AoeSize);
            localBullet.SetAoe(true);
            GameEvents.instance.UpdateMana(-manaCost);
            localBullet.StartCoroutine(DisableAoeAfterDuration(localBullet));
        }
    }

    private IEnumerator DisableAoeAfterDuration(GunBullet bullet)
    {
        yield return new WaitForSeconds(AoeDuration);
        if (bullet != null)
            bullet.SetAoe(false);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, AoeDamage, AoeDuration, AoeSize, manaCost));
    }

    public override bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Damage)
        {
            AoeDamage += value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Probability)
        {
            AoeSize += value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Mana)
        {
            manaCost += (int)value;
            return true;
        }
        return false;
    }

    public override void Reset()
    {
        base.Reset();
        // Reset all public parameters to their initial values
        AoeDamage = _initAoeDamage;
        AoeDuration = _initAoeDuration;
        AoeSize = _initAoeSize;
        manaCost = _initManaCost;
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
    }
}
