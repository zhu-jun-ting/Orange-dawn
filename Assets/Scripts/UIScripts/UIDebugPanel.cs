using UnityEngine;
using UnityEngine.UI;

public class UIDebugPanel : MonoBehaviour
{
    // Assign these in the Inspector or find them at runtime
    public Button button1;
    public Button button2;
    public Button button3;

    public GameObject debugCardMasterPrefab;

    void Start()
    {
        button1.onClick.AddListener(CallMethod1);
        button2.onClick.AddListener(CallMethod2);
        // button3.onClick.AddListener(CallMethod3);
    }

    // Example methods to call
    public void CallMethod1()
    {
        GameEvents.instance.LevelCleared();
    }

    public void CallMethod2()
    {
        HandArea.instance.AddCardObject(debugCardMasterPrefab, null);
    }

    public void CallMethod3()
    {
        Debug.Log("Method 3 called!");
        // Your logic here
    }
}