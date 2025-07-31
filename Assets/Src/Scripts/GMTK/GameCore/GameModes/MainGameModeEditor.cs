#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace GMTK
{
    [CustomEditor(typeof(MainGameMode))]
    public class MainGameModeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MainGameMode game_mode = (MainGameMode)target;

            // Create a button in the Inspector
            if (GUILayout.Button("Init game"))
            {
//                game_mode.InitGame();
            }

            // Create a button in the Inspector
            if (GUILayout.Button("Init hud"))
            {
//                game_mode.InitHud();
            }

            DrawDefaultInspector();
        }
    }
}
#endif