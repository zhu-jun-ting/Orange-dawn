using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardAction
{
    void TriggerAction(CardMaster cardCondition= null, Transform location = null);
}
