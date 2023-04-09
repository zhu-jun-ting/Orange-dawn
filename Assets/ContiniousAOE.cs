using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class ContiniousAOE : MonoBehaviour
{
    private List<Collider2D> pawns = new List<Collider2D>();

    [Header("triggering tags")]
    public List<string> trigger_tags; 
    public float damage_interval;
    public float lifetime;
    public float damage = 10f;

    // [Header("for AOE DOT test")]
    // public DOTStat dot_stat;

    // public Collider2D collider;
    
    // Use this for initialization
    void Start () {

        if (lifetime != 0f) {
            Destroy(gameObject, lifetime);
        }
    }

    public void DealAOEDamage() {
        // Debug.Log("AOE damage timestamp : " + Time.time);
        foreach(Collider2D pawn in pawns) {
            if (pawn != null) {
                IBuffable ibuffable = pawn.gameObject.GetComponent<IBuffable>();

                ibuffable.Damage(damage, GameEvents.DamageType.Normal, 0f, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CombatManager.instance.GetCurrentFrame() % (int)(damage_interval * 60) == 0) {
            DealAOEDamage();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        //if the object is not already in the list
        if(!pawns.Contains(other) && trigger_tags.Contains(other.tag)) {
            //add the object to the list
            pawns.Add(other);
            // Debug.Log(other + " added");
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        //if the object is not already in the list
        if(pawns.Contains(other)  && trigger_tags.Contains(other.tag)) {
            //add the object to the list
            pawns.Remove(other);
            // Debug.Log(other + " removeed. ");
        }
    }



    public void nothing() {
        
    }
}
