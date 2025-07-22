using UnityEngine;

public class PlayerChoice : MonoBehaviour
{
    [Header("Direction for this trigger")]
    public GameEvents.Dir direction;

    [Header("Wall to raise when triggered")]
    public GameObject wallToRaise;

    [Header("All PlayerChoice triggers in scene")]
    public PlayerChoice[] allChoices;

    private void Start()
    {
        GameEvents.instance.OnLevelCleared += ActivateSelf;
        gameObject.SetActive(false); // Start inactive
    }

    private void OnDestroy()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelCleared -= ActivateSelf;
    }

    private void ActivateSelf()
    {
        gameObject.SetActive(true);
    }

    private bool triggered = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            OnPlayerChoiceTrigger();
        }
    }

    // This should be called by a trigger (e.g. OnTriggerEnter2D/3D or UI button)
    public void OnPlayerChoiceTrigger()
    {
        // Also find and deactivate all PlayerChoice in the scene
        foreach (var choice in FindObjectsByType<PlayerChoice>(FindObjectsSortMode.None))
        {
            if (choice != null)
                choice.gameObject.SetActive(false);
        }

        // Raise the wall
        if (wallToRaise != null)
        {
            wallToRaise.SetActive(true);
            var floorDoor = wallToRaise.GetComponent<FloorDoor>();
            if (floorDoor != null)
                floorDoor.alwaysActive = true;
        }
        // Notify game event
        GameEvents.instance.PlayerChoseNextRoom(direction);
    }
}
