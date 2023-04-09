using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDetectorHandler
{
    public void HandleOnTriggerEnter2D(int collider_id, GameObject self, GameObject other);
    public void HandleOnTriggerExit2D(int collider_id, GameObject self, GameObject other);
}