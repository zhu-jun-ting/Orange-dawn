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
        onHitEnemy?.Invoke(damage_, enemy_);
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

        bool isTaken = false;

        // calling the reciever's TakeDamage method
        if (reciever_ != null)
        {
            isTaken = reciever_.TakeDamage(damage_, reciever_, instigator_, damage_type_, location_, hit_back_factor_, source_);
        }

        if (!isTaken)
        {
            return; // If the damage was not taken, exit early
        }

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
        onShowNumberUI?.Invoke(damage_, reciever_, damage_type_, location_, prefix);
    }

    public event Action<string, PawnMaster, DamageType, Vector2, string> onShowStringUI;
    public void ShowStringUI(string damage_, PawnMaster reciever_, DamageType damage_type_, Vector2 location_, string prefix = "")
    {
        onShowStringUI?.Invoke(damage_, reciever_, damage_type_, location_, prefix);
    }


    public event Action<PawnMaster, float, GameObject, DamageType, Gun> OnPawnDie;
    public void PawnDie(PawnMaster _pawn, float _killDamage = 0f, GameObject _instigator_ = null, DamageType _damageType = DamageType.Normal, Gun _gun = null)
    {
        OnPawnDie?.Invoke(_pawn, _killDamage, _instigator_, _damageType, _gun);
    }

    public event Action<PawnMaster> OnPawnSpawn;
    public void PawnSpawn(PawnMaster _pawn)
    {
        OnPawnSpawn?.Invoke(_pawn);
    }

    public event Action<int, int> OnUpdateMana;
    public void UpdateMana(int diffMana = 0, int maxMana = -1)
    {
        OnUpdateMana?.Invoke(diffMana, maxMana);
    }

    public event Action OnLevelCleared;
    public void LevelCleared()
    {
        lastLevelStartOrClearTime = Time.time;
        OnLevelCleared?.Invoke();
    }

    public event Action<int> OnUpdateCoins;
    public void UpdateCoins(int diffCoin)
    {
        OnUpdateCoins?.Invoke(diffCoin);
    }

    public event Action<int> OnUpdateHealth;
    public void UpdateHealth(int diffHealth)
    {
        OnUpdateHealth?.Invoke(diffHealth);
    }

    public enum MessageType { FullInfo, FullWarning, LocalInfo, WorldInfo, Banner }
    public event Action<string, MessageType, Vector2> OnShowMessage;
    public void ShowMessage(string message, MessageType type = MessageType.FullInfo, Vector2 position = default(Vector2))
    {
        OnShowMessage?.Invoke(message, type, position);
    }

    public event Action<GunBullet, Vector2, GameObject> OnHitWall;
    public void HitWall(GunBullet bullet, Vector2 hitPosition, GameObject wall)
    {
        OnHitWall?.Invoke(bullet, hitPosition, wall);
    }

    public float lastLevelStartOrClearTime = 0f;
    public event Action OnLevelStart;
    public void LevelStart()
    {
        lastLevelStartOrClearTime = Time.time;
        OnLevelStart?.Invoke();
    }

    /// <summary>
    /// Event to toggle the board visibility. True if board is going active, false if it is going hidden.
    /// </summary>
    public event Action<bool> OnToggleBoard;
    public void ToggleBoard(bool isActive)
    {
        OnToggleBoard?.Invoke(isActive);
    }


    public static bool isPlayingCardAnimation = false;
    /// <summary>
    /// Event to tell if is playing the card animation. True if playing, false if animation is done.
    /// </summary>
    public event Action<bool> OnPlayCardAnimation;
    public void PlayCardAnimation(bool isPlaying)
    {
        isPlayingCardAnimation = isPlaying;
        OnPlayCardAnimation?.Invoke(isPlaying);
    }

    public static int discardedCardsCount = 0;
    public event Action<CardMaster> OnCardDiscarded;
    public void CardDiscarded(CardMaster card)
    {
        discardedCardsCount++;
        OnCardDiscarded?.Invoke(card);
    }

    public event Action<Transform> OnSpawnObject;
    public void SpawnObject(Transform obj)
    {
        OnSpawnObject?.Invoke(obj);
    }

    public event Action<Transform, GunBullet> OnDestroyObject;
    public void DestroyObject(Transform obj, GunBullet bullet = null)
    {
        OnDestroyObject?.Invoke(obj, bullet);
    }

    public event Action<float> OnPlayerMove;
    public void PlayerMove(float distance)
    {
        OnPlayerMove?.Invoke(distance);
    }

    public event Action<NPCMaster> OnNPCCharge;
    public void NPCCharge(NPCMaster npc)
    {
        OnNPCCharge?.Invoke(npc);
    }

    public event Action<Transform> OnPlayerDodge;
    public void PlayerDodge(Transform _player)
    {
        OnPlayerDodge?.Invoke(_player);
        ShowStringUI("DODGE", PlayerController.instance, DamageType.Normal, PlayerController.instance.transform.position);
        SoundManager.PlaySFX("Miss");
    }

    public enum Dir { Up, Down, Left, Right }
    public event Action<Dir> OnPlayerChoseNextRoom;
    public void PlayerChoseNextRoom(Dir direction)
    {
        OnPlayerChoseNextRoom?.Invoke(direction);
    }

    public event Action<int> OnLoadLevel;
    public void LoadLevel(int levelIndex)
    {
        OnLoadLevel?.Invoke(levelIndex);
    }

    public event Action<CardMaster, Transform> OnTriggerActionCard;
    public void TriggerActionCard(CardMaster card, Transform target)
    {
        OnTriggerActionCard?.Invoke(card, target);
        // No matter what card, always show a Popup for notify this card triggered
        // Move the message position up a bit (e.g., by 1 unit on Y axis)
        Vector2 messagePosition = target.position;
        messagePosition.y += 1f;
        GameEvents.instance.ShowMessage($"{GameSettings.LocalizeText(card.card_name)}", GameEvents.MessageType.WorldInfo, messagePosition);
    }

    public event Action<CardMaster, Vector2Int> OnDropCardOnBoard;
    public void DropCardOnBoard(CardMaster card, Vector2Int gridLocation)
    {
        OnDropCardOnBoard?.Invoke(card, gridLocation);
    }

    public event Action OnGameStart;
    public void GameStart()
    {
        OnGameStart?.Invoke();
    }

    public event Action<int> OnLevelUp;
    public void LevelUp(int level)
    {
        GameEvents.instance.ShowMessage(GameSettings.LocalizeText("Levelup"), GameEvents.MessageType.WorldInfo, Vector2.zero);
        OnLevelUp?.Invoke(level);
    }
}