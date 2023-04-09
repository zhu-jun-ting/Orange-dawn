using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : EnemyMaster
{

    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        FollowTarget(target);

        
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player") {
            // Debug.Log("TODO: MeleeEnemy: deal damage to player.");
            HurtPlayer(target.gameObject, 10f); // TODO: update parameter
        }
    }
}
