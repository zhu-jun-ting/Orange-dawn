using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPool : MonoBehaviour
{

    public static ShadowPool instance;

    // variables
    [Header("dynamically controlled by scripts")]
    public int shadowCount;
    public GameObject prefab;
    public Transform trans;
    public int framePerShadow; // how many FPS a shadow should show (e.g. 5 means a shadow per 5 frames)

    [Header("parameters of time and duration")]
    public float startAlpha;
    public float alphaMultiplier;
    public float duration;

    // queue of shadows
    private Queue<GameObject> shadows = new Queue<GameObject>();

    private void Awake()
    {
        instance = this;

        FillPool();
    }

    public void SetAlphaMultiplierAndDuration(float _startAlpha, float _alphaMultiplier, float _duration)
    {
        startAlpha = _startAlpha;
        alphaMultiplier = _alphaMultiplier;
        duration = _duration;
    }

    public void SetPrefabAndTransform(GameObject p, Transform t)
    {
        prefab = p;
        trans = t;
    }

    public void FillPool()
    {
        for (int i = 0; i < shadowCount; i++)
        {
            var newShadow = Instantiate(prefab);
            newShadow.transform.SetParent(gameObject.transform);
            ReturnPool(newShadow);
        }
    }

    public void ReturnPool(GameObject GO)
    {
        GO.SetActive(false);
        shadows.Enqueue(GO);

    }

    public GameObject GetFromPool()
    {
        if (shadows.Count == 0) FillPool();
        var newShadow = shadows.Dequeue();
        newShadow.SetActive(true);
        return newShadow;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
