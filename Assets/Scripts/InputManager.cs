using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action OnYKeyPressed;
    public event Action<Vector2> OnMove; // WASD movement
    public event Action OnFire; // Mouse left click
    public event Action OnPause; // ESC
    public event Action OnBuffSelectionToggle; // KeyCode.T

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // WASD movement
        Vector2 move = new Vector2(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)
        );
        if (move != Vector2.zero)
        {
            OnMove?.Invoke(move.normalized);
        }

        // Mouse left click (fire)
        if (Input.GetMouseButtonDown(0))
        {
            OnFire?.Invoke();
        }

        // ESC for pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPause?.Invoke();
        }

        // KeyCode.T for buff selection
        if (Input.GetKeyDown(KeyCode.T))
        {
            OnBuffSelectionToggle?.Invoke();
        }

        // Retain Y key event for backward compatibility
        if (Input.GetKeyDown(KeyCode.Y))
        {
            OnYKeyPressed?.Invoke();
        }
    }
}
