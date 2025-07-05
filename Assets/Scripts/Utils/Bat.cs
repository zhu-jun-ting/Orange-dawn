using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour
{
    [Header("Bat Settings")]
    [SerializeField] public float attackPower;
    [SerializeField] private GameObject owner;
    [SerializeField] private float hitBackEnemy = 10f;
    [SerializeField] private float hitBackBullet = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            // Debug.Log("enemy");
            PawnMaster pawnMaster = other.gameObject.GetComponent<PawnMaster>();
            if (pawnMaster != null && attackPower >= 1f) GameEvents.instance.HitPawn(attackPower, pawnMaster, owner.gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hitBackEnemy, null);
        }
        else if (other.tag == "Bullet")
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (other.transform.position - transform.position).normalized;
                rb.linearVelocity = direction * hitBackBullet;
            }
        }
    }
}
