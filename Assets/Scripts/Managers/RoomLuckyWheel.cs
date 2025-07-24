using UnityEngine;
using System.Collections.Generic;

public class RoomLuckyWheel : RoomGrid


{
    [Header("Lucky Wheel Triggers")]
    public List<LuckyWheelTrigger> wheelTriggers = new List<LuckyWheelTrigger>();

    public void OnBulletStopped(Vector2 position)
    {
        foreach (var trigger in wheelTriggers)
        {
            if (trigger != null && trigger.IsPositionInside(position))
            {
                trigger.OnBulletLanded();
                break;
            }
        }
    }

    private void HandleLevelCleared()
    {
        foreach (var trigger in wheelTriggers)
        {
            if (trigger != null)
                trigger.gameObject.SetActive(false);
        }
    }

    public override void OnLevelStart()
    {
        PlayerController.instance.ActivatePistol(true);
    }

    public override void OnLevelCleared()
    {
        PlayerController.instance.ActivatePistol(false);
        HandleLevelCleared();
    }
}

