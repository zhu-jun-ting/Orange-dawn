using UnityEngine;

public class LevelStartTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            if (GameEvents.instance != null)
            {
                if (CombatManager.instance != null && CombatManager.instance.currentLevel != null)
                {
                    GameEvents.instance.LevelStart();
                }
            }
            Destroy(gameObject);
        }
    }
}
