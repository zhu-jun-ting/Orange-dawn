using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public Transform lookAt;
    [Tooltip("How far the camera zooms out. 1 = default, >1 = zoom out, <1 = zoom in")]
    public float cameraZoom = 1f;

    // Start is called before
    // the first frame update

    private void LateUpdate()
    {
        if (lookAt == null) return;
        // Center camera on player
        Vector3 newPos = new Vector3(lookAt.position.x, lookAt.position.y, transform.position.z);
        transform.position = newPos;

        // Adjust camera zoom (orthographic size)
        Camera cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = cameraZoom;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
