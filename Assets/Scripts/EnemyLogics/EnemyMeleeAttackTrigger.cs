using UnityEngine;

public class EnemyMeleeAttackTrigger : MonoBehaviour
{
    public EnemyMaster enemyMaster; // Assign in inspector or via code

    void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyMaster != null)
        {
            enemyMaster.OnMeleeAttackTriggerEnter(other);
        }
    }
}