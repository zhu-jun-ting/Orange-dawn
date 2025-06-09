using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FindScriptReferenceInScene : EditorWindow
{
    private string scriptName = "";
    private Vector2 scrollPos;
    private List<GameObject> foundObjects = new List<GameObject>();

    [MenuItem("Tools/Find Script Reference In Scene")]
    public static void ShowWindow()
    {
        GetWindow<FindScriptReferenceInScene>("Find Script Reference");
    }

    void OnGUI()
    {
        GUILayout.Label("Find Script Reference In Current Scene", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField("Script/Class Name:", scriptName);

        if (GUILayout.Button("Search"))
        {
            FindReferences();
        }

        if (foundObjects.Count > 0)
        {
            GUILayout.Label($"Found {foundObjects.Count} GameObject(s):", EditorStyles.boldLabel);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            foreach (var go in foundObjects)
            {
                if (GUILayout.Button(go.name, GUILayout.ExpandWidth(false)))
                {
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                }
            }
            GUILayout.EndScrollView();
        }
    }

    void FindReferences()
    {
        foundObjects.Clear();
        if (string.IsNullOrEmpty(scriptName)) return;
        var regex = new Regex(scriptName, RegexOptions.IgnoreCase);
        var allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in allObjects)
        {
            var components = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var comp in components)
            {
                if (comp == null) continue;
                var type = comp.GetType();
                if (regex.IsMatch(type.Name) || regex.IsMatch(type.FullName))
                {
                    foundObjects.Add(comp.gameObject);
                }
            }
        }
    }
}
