// using System.Collections;
// using UnityEngine;

// public class CardActionATemplate : CardMaster, ICardAction
// {
//     public event System.Action<CardMaster, Transform> OnTrigger;

//     public override void OnCardEnable()
//     {
//         if (GameEvents.instance != null)
//             GameEvents.instance.OnLevelStart += HandleOnLevelStart;
//         OnTrigger -= TriggerAction;
//         OnTrigger += TriggerAction;
//         base.OnCardEnable();
//     }

//     public override void Reset()
//     {
//         base.Reset();
//         if (GameEvents.instance != null)
//             GameEvents.instance.OnLevelStart -= HandleOnLevelStart;
//         OnTrigger -= TriggerAction;
//     }

//     public override string GetDescription()
//     {
//         return GameSettings.AddIcon(string.Format(card_description, damage, health, mana));
//     }

//     private void HandleOnLevelStart(int levelIndex)
//     {
//         if (Time.time - lastActionTime < actionCooldown) return;
//         if (UnityEngine.Random.value > probability) return; // Always triggers, but keep for extensibility
//         lastActionTime = Time.time; // Update last action time
//         OnTrigger?.Invoke(this, transform);
//     }

//     public void TriggerAction(CardMaster card, Transform target)
//     {
//         if (!ManaBar.CanCostMana(-(int)mana)) return;
//         // Use parent variables: damage, health, mana, etc.
//         // Example: Summon skeletons using damage, health, mana
//         SummonSkeletons(damage, health, mana);
//         GameEvents.instance.UpdateMana(-(int)mana);
//     }

//     private void SummonSkeletons(float dmg, float hp, float mp)
//     {
//         // Implement skeleton summoning logic here using only parent variables
//         // ...existing code...
//     }
// }