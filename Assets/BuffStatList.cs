using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffStatList : MonoBehaviour
{
    public List<StatMaster> buffstats;
    public static BuffStatList instance;

    void Awake()
    {
        instance = this;
    }
}