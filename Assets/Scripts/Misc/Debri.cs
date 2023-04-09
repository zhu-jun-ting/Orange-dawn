using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Debri : MonoBehaviour
{
	public GameObject debriPrefab;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
	private ShadowPool SP;

    [Header("parameters")]
    public float rotationSpeed;
    public float rotationFrictionMultiplier;

    [Header("manager")]
    public float lifeTime;
    //public float alphaMultiplier;

    private float curAlphaPercent;
    private float startTime;

	// internal variables
    private int frameCount;

    // Use this for initialization
    void Start()
	{
		rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
		SP = ShadowPool.instance;


        rb.velocity = new Vector2(0.5f, 0.5f);
		frameCount = 0;
        curAlphaPercent = 1;
        startTime = Time.time;

        // end lifetime countdown
        Destroy(gameObject, lifeTime);
    }

	// Update is called once per frame
	void Update()
	{
        curAlphaPercent = 1 - (Time.time - startTime) / lifeTime;
        transform.Rotate(new Vector3(0f, 0f, rotationSpeed));
        rotationSpeed *= rotationFrictionMultiplier;

        Color c = sr.color;
        c.a = curAlphaPercent;
        sr.color = c;

    }

    private void FixedUpdate()
    {
        frameCount++;
        //sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a * alphaMultiplier);

        if (frameCount % SP.framePerShadow == 0)
        {
            SP.startAlpha = sr.color.a;
            SP.SetPrefabAndTransform(debriPrefab, transform);
            ShadowPool.instance.GetFromPool();
        }
    }
}

