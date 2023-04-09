using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Detector : MonoBehaviour
{

    [Header("the collision event handler: must implement IDetectorHandler")]
    // public Transform trigger_target;
    public GameObject collision_handler;
    public List<string> trigger_tags; 
    // all tags included gameobjects will trigger this collider

    public int collider_id;
    // identifier of this collider, use for the handler to know which collider is being triggered

    private CircleCollider2D range_collider;
    private List<Collider2D> colliders_in_range; 

    // private GameObject my_parent;

    void Start()
    {
        // my_parent = transform.parent.gameObject;
        range_collider = GetComponent<CircleCollider2D>();
        if (collision_handler.GetComponent<IDetectorHandler>() == null) {
            Debug.LogError("Detector collision handler does not have a correct handler.");
        }
        colliders_in_range = new List<Collider2D>();
    }

    void FixedUpdate()
    {
        transform.position = collision_handler.transform.position;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (trigger_tags.Contains(other.gameObject.tag)) {
            colliders_in_range.Add(other);
            collision_handler.GetComponent<IDetectorHandler>().HandleOnTriggerEnter2D(collider_id, gameObject, other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (trigger_tags.Contains(other.gameObject.tag)) {
            colliders_in_range.Remove(other);
            collision_handler.GetComponent<IDetectorHandler>().HandleOnTriggerExit2D(collider_id, gameObject, other.gameObject);

        }
    }

    public void ChangeColliderRadius(float r) {
        range_collider.radius = r;
    }

    public bool IsEmptyWithinCollider() {
        return colliders_in_range.Count == 0;
    }

    public GameObject GetRandomGameObjectInRange() {
        int index = Random.Range(0, colliders_in_range.Count);
        // Debug.Log(colliders_in_range.Count);
        return colliders_in_range[index].gameObject;
    }
}
