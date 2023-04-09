using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSpriteController : MonoBehaviour
{

    [Header("game objects")]
    private Transform mainObject;
    private SpriteRenderer mainSR;
    private SpriteRenderer thisSR;

    [Header("time control")]
    public float startAlpha;
    private float curAlpha;
    public float alphaMultiplier;

    public float duration;
    private float startTime;

    private Color curColor;

    private void OnEnable()
    {
        mainObject = ShadowPool.instance.trans;
        mainSR = mainObject.GetComponent<SpriteRenderer>();
        thisSR = GetComponent<SpriteRenderer>();

        startAlpha = ShadowPool.instance.startAlpha;
        alphaMultiplier = ShadowPool.instance.alphaMultiplier;
        duration = ShadowPool.instance.duration;

        //thisSR.sprite = mainSR.sprite;
        transform.position = mainObject.position;
        transform.localScale = mainObject.localScale;
        transform.rotation = mainObject.rotation;

        startTime = Time.time;
        curAlpha = startAlpha;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        curAlpha = curAlpha * alphaMultiplier;
        curColor = new Color(1, 1, 1, curAlpha);
        thisSR.color = curColor;

        if (Time.time >= startTime + duration)
        {
            Debug.Log("game object should be destroyed");
            ShadowPool.instance.ReturnPool(gameObject);
        }
    }
}
