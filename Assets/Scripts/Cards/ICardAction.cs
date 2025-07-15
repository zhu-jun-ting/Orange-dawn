using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardAction
{
    public float actionCooldown { get; set; }
    public event Action<CardMaster, Transform> OnTrigger;
    void TriggerAction(CardMaster cardCondition= null, Transform location = null);
}
