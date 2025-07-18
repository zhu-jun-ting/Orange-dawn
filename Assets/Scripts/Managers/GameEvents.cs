using System.Runtime.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        instance = this;
    }

    public event Action<float, EnemyMaster> onHitEnemy;
    public void HitEnemy(float damage_, EnemyMaster enemy_)
    {
        if (onHitEnemy != null)
        {
            onHitEnemy(damage_, enemy_);
        }
    }





    public event Action<float, PawnMaster, GameObject, DamageType, Transform, float, Gun> OnHitPawn;
    public static Func<float, float> OnModifyDamage;
    public void HitPawn(float damage_, PawnMaster reciever_, GameObject instigator_ = null, DamageType damage_type_ = DamageType.Normal, Transform location_ = null, float hit_back_factor_ = 0f, Gun source_ = null, string prefix = "", System.Action<float> modifyDamageCallback = null)
    {
        // Modify the damage if a callback is provided
        if (OnModifyDamage != null && modifyDamageCallback != null)
        {
            damage_ = OnModifyDamage(damage_);
            modifyDamageCallback?.Invoke(damage_);
        }

        // calling the reciever's TakeDamage method
        if (reciever_ != null) reciever_.TakeDamage(damage_, reciever_, instigator_, damage_type_, location_, hit_back_factor_, source_);

        if (OnHitPawn != null)
        {
            OnHitPawn(damage_, reciever_, instigator_, damage_type_, location_, hit_back_factor_, source_);
        }
        if (onShowNumberUI != null && location_ != null)
        {
            onShowNumberUI((int)damage_, reciever_, damage_type_, (Vector2)location_.position, prefix);
        }
    }

    public event Action<float, PawnMaster, GameObject, Transform> OnHealPawn;
    public void HealPawn(float _health, PawnMaster _receiver, GameObject _instigator = null, Transform location_ = null, System.Action<float> modifyHealthCallback = null)

    {
        // Modify the health if a callback is provided
        modifyHealthCallback?.Invoke(_health);

        if (_receiver != null)
        {
            bool healed = _receiver.Heal(_health);
            if (healed)
            {
                OnHealPawn?.Invoke(_health, _receiver, _instigator, location_);
                // CombatManager.instance.HandleShowDamageUI((int)_health, _receiver, GameEvents.DamageType.Heal, location_);
                // CombatManager.instance.HandleShowDamageUI((int)_health, _receiver, GameEvents.DamageType.Heal, location_);
                if (onShowNumberUI != null && location_ != null)
                {
                    onShowNumberUI((int)_health, _receiver, GameEvents.DamageType.Heal, (Vector2)location_.position, "");
                }
            }
        }
    }




    public enum DamageType { Normal, Crit, Heal, DotDamage, Aoe }

    public event Action<int, PawnMaster, DamageType, Vector2, string> onShowNumberUI;
    public void ShowNumberUI(int damage_, PawnMaster reciever_, DamageType damage_type_, Vector2 location_, string prefix = "")
    {
        if (onShowNumberUI != null)
        {
            onShowNumberUI(damage_, reciever_, damage_type_, location_, prefix);
        }
    }



    public event Action<int, int> OnUpdateMana;
    public void UpdateMana(int diffMana = 0, int maxMana = -1)
    {
        if (OnUpdateMana != null)
        {
            OnUpdateMana(diffMana, maxMana);
        }
    }

    public event Action OnLevelCleared;
    public void LevelCleared()
    {
        if (OnLevelCleared != null)
        {
            OnLevelCleared();
        }
    }

    public event Action<int> OnUpdateCoins;
    public void UpdateCoins(int diffCoin)
    {
        if (OnUpdateCoins != null)
        {
            OnUpdateCoins(diffCoin);
        }
    }

    public event Action<int> OnUpdateHealth;
    public void UpdateHealth(int diffHealth)
    {
        if (OnUpdateHealth != null)
        {
            OnUpdateHealth(diffHealth);
        }
    }

    public enum MessageType { FullInfo, FullWarning, LocalInfo }
    public event Action<string, MessageType, Vector2> OnShowMessage;
    public void ShowMessage(string message, MessageType type = MessageType.FullInfo, Vector2 position = default(Vector2))
    {
        if (OnShowMessage != null)
        {
            OnShowMessage(message, type, position);
        }
    }

    public event Action<GunBullet, Vector2, GameObject> OnHitWall;
    public void HitWall(GunBullet bullet, Vector2 hitPosition, GameObject wall)
    {
        if (OnHitWall != null)
        {
            OnHitWall(bullet, hitPosition, wall);
        }
    }

    public event Action<int> OnLevelStart;
    public void LevelStart(int levelIndex = 0)
    {
        if (OnLevelStart != null)
        {
            OnLevelStart(levelIndex);
        }
    }

    /// <summary>
    /// Event to toggle the board visibility. True if board is going active, false if it is going hidden.
    /// </summary>
    public event Action<bool> OnToggleBoard;
    public void ToggleBoard(bool isActive)
    {
        if (OnToggleBoard != null)
        {
            OnToggleBoard(isActive);
        }
    }


    public static bool isPlayingCardAnimation = false;
    /// <summary>
    /// Event to tell if is playing the card animation. True if playing, false if animation is done.
    /// </summary>
    public event Action<bool> OnPlayCardAnimation;
    public void PlayCardAnimation(bool isPlaying)
    {
        isPlayingCardAnimation = isPlaying;
        if (OnPlayCardAnimation != null)
        {
            OnPlayCardAnimation(isPlaying);
        }
    }

    public static int discardedCardsCount = 0;
    public event Action<CardMaster> OnCardDiscarded;
    public void CardDiscarded(CardMaster card)
    {
        discardedCardsCount++;
        if (OnCardDiscarded != null)
        {
            OnCardDiscarded(card);
        }
    }

    public event Action<Transform, GunBullet> OnSpawnObject;
    public void SpawnObject(Transform obj, GunBullet bullet = null)
    {
        if (OnSpawnObject != null)
        {
            OnSpawnObject(obj, bullet);
        }
    }

    public event Action<Transform, GunBullet> OnDestroyObject;
    public void DestroyObject(Transform obj, GunBullet bullet = null)
    {
        if (OnDestroyObject != null)
        {
            OnDestroyObject(obj, bullet);
        }
    }

    public event Action<float> OnPlayerMove;
    public void PlayerMove(float distance)
    {
        if (OnPlayerMove != null)
        {
            OnPlayerMove(distance);
        }
    }

    public event Action<NPCMaster> OnNPCCharge;
    public void NPCCharge(NPCMaster npc)
    {
        if (OnNPCCharge != null)
        {
            OnNPCCharge(npc);
        }
    }
}