using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoGun : Gun, IDetectorHandler
{
    public List<string> trigger_tags;
    // all tags included gameobjects will trigger this gun to shoot

    private List<GameObject> targets_in_range = new List<GameObject>();
    private GameObject currentTarget = null;



	public void HandleOnTriggerEnter2D(int id, GameObject sender, GameObject other)
    {
        if (trigger_tags.Contains(other.tag) && !targets_in_range.Contains(other))
        {
            targets_in_range.Add(other);
            PickNewTarget();
        }
    }

	public void HandleOnTriggerExit2D(int id, GameObject sender, GameObject other)
	{
        if (targets_in_range.Contains(other))
        {
            targets_in_range.Remove(other);
            if (other == currentTarget)
            {
                PickNewTarget();
            }
        }
	}

    private void PickNewTarget()
    {
        // Clean up any nulls
        targets_in_range = targets_in_range.Where(t => t != null).ToList();
        if (targets_in_range.Count > 0)
        {
            currentTarget = targets_in_range[Random.Range(0, targets_in_range.Count)];
        }
        else
        {
            currentTarget = null;
        }
    }

    protected override void Update()
    {
        // Clean up destroyed targets
        if (currentTarget == null && targets_in_range.Count > 0)
        {
            PickNewTarget();
        }
        if (currentTarget != null)
        {
            Vector2 targetPos = currentTarget.transform.position;
            direction = (targetPos - (Vector2)transform.position).normalized;
            transform.right = direction;

            // Shooting frequency logic (same as Gun)
            if (timer != 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                    timer = 0;
            }

            if (timer == 0)
            {
                timer = interval;
                Fire();
            }
        }
        // else do nothing
    }
}
