using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ShootRangeDetector : MonoBehaviour
{

    [Header("game objects")]
    public Transform target;
    private CircleCollider2D range_collider;

    private GameObject my_parent;

    void Start()
    {
        my_parent = transform.parent.gameObject;
        range_collider = GetComponent<CircleCollider2D>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == target.tag) {
            my_parent.GetComponent<EnemyShooter>().ChangeState(EnemyShooter.State.shooting);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == target.tag) {
            my_parent.GetComponent<EnemyShooter>().ChangeState(EnemyShooter.State.walking);
        }
    }

    public void ChangeColliderRadius(float r) {
        range_collider.radius = r;
    }
}
