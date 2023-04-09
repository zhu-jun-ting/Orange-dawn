using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebriPrefabController : MonoBehaviour
{

    [Header("game objects")]
    private Transform mainObject;
    private SpriteRenderer mainSR;
    private SpriteRenderer thisSR;

    [Header("time control (don't edit this)")]
    public float startAlpha;
    private float curAlpha;
    public float alphaMultiplier;

    public float duration;
    private float startTime;

    private Color curColor;

    [Header("for debri control")]
    [Range(0, 2)]
    public float colorTintFactorMin;
    [Range(0, 2)]
    public float colorTintFactorMax;

    public float rotationForceMax;
    public float rotationForceMin;

    [Range(0, 2)]
    public float scaleTintFactorMin;
    [Range(0, 2)]
    public float scaleTintFactorMax;

    public float displacementMax;
    public float displacementMin;

    [Header("for physics system")]
    public float rotationFrictionMultiplier;

    // internal variables
    private float r;
    private float g;
    private float b;
    private float rotationForce;
    private float scaleTintFactor;
    private float displacement;
    

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
        r = Random.Range(colorTintFactorMin, colorTintFactorMax);
        g = Random.Range(colorTintFactorMin, colorTintFactorMax);
        b = Random.Range(colorTintFactorMin, colorTintFactorMax);

        rotationForce = Random.Range(rotationForceMax, rotationForceMin);
        scaleTintFactor = Random.Range(scaleTintFactorMin, scaleTintFactorMax);
        displacement = Random.Range(displacementMin, displacementMax);

        transform.position += new Vector3(displacement, displacement, 0);
    }

    // Update is called once per frame
    void Update()
    {
        curAlpha = curAlpha * alphaMultiplier;
        curColor = new Color(r, g, b, curAlpha);
        thisSR.color = curColor;

        if (Time.time >= startTime + duration)
        {
            Debug.Log("game object should be destroyed");
            ShadowPool.instance.ReturnPool(gameObject);
        }

        transform.Rotate(new Vector3(0f, 0f, rotationForce));
        rotationForce *= rotationFrictionMultiplier;
    }
}
