using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class AoeController : MonoBehaviour
{
    private Animator animator;
    private List<Collider2D> pawns = new List<Collider2D>();

    [Header("triggering tags")]
    public List<string> trigger_tags; 

    [Header("for AOE DOT test")]
    public DOTStat dot_stat;

    // public Collider2D collider;
    
    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        // print(animator);
        // animator.SetTrigger("start_anim");
        // Reset();
    }

    public void DealAOEDamage() {
        // Debug.Log("AOE damage timestamp : " + Time.time);
        foreach(Collider2D pawn in pawns) {
            if (pawn != null) {
                IBuffable ibuffable = pawn.gameObject.GetComponent<IBuffable>();
                // BuffMaster buff = ScriptableObject.CreateInstance(typeof(BuffMaster)) as BuffMaster;
                Buff buff = new Buff(dot_stat);
                ibuffable.Damage(10, GameEvents.DamageType.Normal, 0f, transform);
                ibuffable.ApplyBuff(buff); 
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Y)) {
            animator.SetTrigger("start_anim");
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

    //Since we use editor calls we omit t$$anonymous$$s function on build time
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void Reset()
    {
        AudioSource source = GetComponent<AudioSource>();
        Light light = GetComponent<Light>();

        if( source == null && light == null ) {
            if( UnityEditor.EditorUtility.DisplayDialog( "Choose a Component", "You are missing one of the required componets. Please choose one to add", "AudioSource", "Light" ) ) {
                gameObject.AddComponent<AudioSource>();
            } else {
                gameObject.AddComponent<Light>();
            }
        }
    }

    public void nothing() {
        
    }
}
