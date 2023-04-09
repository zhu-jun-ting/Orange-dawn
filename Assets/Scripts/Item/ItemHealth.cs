using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHealth : MonoBehaviour
{
    PlayerController gainhealth;
    private GameObject Player;
    private bool inPlayer;
    public float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        gainhealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        moveSpeed = 5.0f;
        inPlayer = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.position - Player.transform.position).magnitude <= 0.3)
        {
            gainhealth.GainHealth(5);
            Destroy(gameObject);
        }

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
}
