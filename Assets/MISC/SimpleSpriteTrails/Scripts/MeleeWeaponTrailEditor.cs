#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSpriteTrails.Scripts
{
    /// <summary>
    /// Add action buttons to LayerManager script
    /// </summary>
    [CustomEditor(typeof(MeleeWeaponTrail))]
    public class MeleeWeaponTrailEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (MeleeWeaponTrail) target;

            if (GUILayout.Button("Build"))
            {
                script.Build();
            }
        }
    }
}

#endif