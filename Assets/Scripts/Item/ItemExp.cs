using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemExp : MonoBehaviour, IColliderHandler
{
    PLayerExp gainExp;
    private GameObject Player;
    private bool inPlayer;
    public float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        gainExp = GameObject.FindGameObjectWithTag("Player").GetComponent<PLayerExp>();
        moveSpeed = 5.0f;
        inPlayer = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inPlayer == true)
        {
            transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Player") == true)
        {
            inPlayer = true;
        }
    }

    public void HandleTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player") == true)
        {
            gainExp.GainExp(10);
            Destroy(gameObject);
        }
        
    }















    public void HandleTriggerExit2D(Collider2D collider)
    {
        // Add logic if needed, or leave empty
    }

    public void HandleCollisionEnter2D(Collision2D collision)
    {
        // Add logic if needed, or leave empty
    }

    public void HandleCollisionExit2D(Collision2D collision)
    {
        // Add logic if needed, or leave empty
    }
}
