using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardDatabase))]
public class CardDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CardDatabase db = (CardDatabase)target;
        if (GUILayout.Button("Sort Cards by Card ID"))
        {
            db.cards.Sort((a, b) => a.cardId.CompareTo(b.cardId));
            EditorUtility.SetDirty(db);
        }
    }
}
