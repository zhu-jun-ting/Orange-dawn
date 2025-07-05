using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash : MonoBehaviour
{
    [SerializeField] private float att;
    [SerializeField] private GameObject owner;
    [SerializeField] private float hit_back_factor = 10f;

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
        if(other.tag == "Enemy")
        {
            // Debug.Log("enemy");
            PawnMaster pawnMaster = other.gameObject.GetComponent<PawnMaster>();
            if (pawnMaster != null && att >= 1f) GameEvents.instance.HitPawn(att, pawnMaster, owner.gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back_factor, null);
        }
    }
}
