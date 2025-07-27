using System;
using UnityEngine;

// id: custom
// name: Prime
// desc: If your killing damage is a prime number, Probability: 60% shoot Amount: 2 copies of the pinball. Cost Mana: 2
public class CardActionPrime : CardMaster, ICardAction
{
    private Gun lastGun = null;
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Prime Settings")]
    public GameObject defaultPinballPrefab; // Assign pinball prefab in inspector
    public float spawnRadius = 0.5f;

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnPawnDie += HandleOnPawnDie;
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
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
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
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnPawnDie(PawnMaster pawn, float killDamage, GameObject instigator, GameEvents.DamageType damageType, Gun gun)
    {
        if (pawn == null || !pawn.isEnemy) return;
        if (!IsPrime((int)killDamage)) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        lastActionTime = Time.time;
        lastGun = gun;
        if (gun != null)
        {
            defaultPinballPrefab = gun.bulletPrefab;
        }
        OnTrigger?.Invoke(this, pawn.transform);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        // If gun is available, use its shooting pattern
        if (lastGun != null)
        {
            var gun = lastGun;
            int bulletNum = (int)amount < 0 ? 1 : (int)amount;
            float bulletAngle = gun.bulletAngle;
            float damage = gun.damage + gun.tempDamage;
            float speed = gun.speed + gun.tempSpeed;
            float hit_back = gun.hit_back;
            float critChance = gun.critChance;
            float critDamage = gun.critDamage;
            int penetrate = gun.penetrate;
            Vector3 shootPos = target != null ? target.position : Vector3.zero;
            Vector2 direction = (PlayerController.instance != null)
                ? (shootPos - PlayerController.instance.transform.position).normalized
                : Vector2.right;
            int median = bulletNum / 2;
            for (int i = 0; i < bulletNum; i++)
            {
                GameObject bullet = ObjectPool.Instance.GetObject(defaultPinballPrefab);
                bullet.transform.position = shootPos;
                var gunBullet = bullet.GetComponent<GunBullet>();
                if (gunBullet != null)
                {
                    gunBullet.trigger_tags.Add("Enemy");
                    gunBullet.att = damage;
                    gunBullet.hit_back = hit_back;
                    gunBullet.SetOwner(PlayerController.instance != null ? PlayerController.instance.gameObject : null);
                    gunBullet.gun = gun;
                    gunBullet.penetrate = penetrate;
                    gunBullet.critChance = critChance;
                    gunBullet.critDamage = critDamage;
                    // gunBullet.AddIgnore("Player, NPC"); // Ignore self
                    if (bulletNum % 2 == 1)
                    {
                        gunBullet.SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median), Vector3.forward) * direction, speed);
                    }
                    else
                    {
                        gunBullet.SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median) + bulletAngle / 2, Vector3.forward) * direction, speed);
                    }
                }
            }
            lastGun = null; // Reset after use
        }
        else
        {
            // Fallback: spawn amount pinballs in a circle
            SpawnObjects(
                _prefab: defaultPinballPrefab,
                _count: (int)amount < 0 ? 1 : (int)amount,
                _position: target != null ? target.position : Vector3.zero,
                _radius: spawnRadius,
                _modifyObject: (obj) => { /* Optionally set pinball properties here */ }
            );
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)amount, (int)mana));
    }
}
