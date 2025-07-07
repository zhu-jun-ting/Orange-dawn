using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float fadeDuration = 0.3f;
    public int maxPositions = 12;
    public float minDistance = 0.05f;

    private LineRenderer line;
    private List<Vector3> positions = new List<Vector3>();
    private float fadeTimer = 0f;
    private bool fading = false;
    private Color startColor;
    private Color endColor;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (line == null) Debug.LogError("BulletTracer: No LineRenderer found!");
        startColor = line.startColor;
        endColor = line.endColor;
    }

    private void OnEnable()
    {
        positions.Clear();
        fadeTimer = 0f;
        fading = false;
        if (line != null)
        {
            line.positionCount = 0;
            line.startColor = startColor;
            line.endColor = endColor;
        }
    }

    private void Update()
    {
        if (!fading)
        {
            AddPosition(transform.position);
        }
        else
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeDuration);
            if (line != null)
            {
                Color fadedStart = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0, t));
                Color fadedEnd = new Color(endColor.r, endColor.g, endColor.b, Mathf.Lerp(endColor.a, 0, t));
                line.startColor = fadedStart;
                line.endColor = fadedEnd;
            }
            if (t >= 1f)
            {
                // ObjectPool.Instance.PushObject(gameObject);
                gameObject.SetActive(false);
            }
        }
    }

    public void AddPosition(Vector3 pos)
    {
        if (positions.Count == 0 || Vector3.Distance(positions[positions.Count - 1], pos) > minDistance)
        {
            positions.Add(pos);
            if (positions.Count > maxPositions)
                positions.RemoveAt(0);
            if (line != null)
            {
                line.positionCount = positions.Count;
                line.SetPositions(positions.ToArray());
            }
        }
    }

    // Call this when the bullet is destroyed or tracer should fade
    public void StartFade()
    {
        fading = true;
        fadeTimer = 0f;
    }

    void OnDestroy()
    {
        StartFade();
    }
}
