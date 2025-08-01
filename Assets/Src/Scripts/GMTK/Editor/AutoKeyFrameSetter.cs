using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace GMTK
{
    public class AddTransformKeyframesToCurrentClip
    {
        [MenuItem("Tools/Animations/Add Keyframes (Position, Rotation, Scale) to All Children in Current Clip")]
        static void AddKeyframes()
        {
            AnimationClip clip = GetCurrentAnimationClip();
            if (clip == null)
            {
                Debug.LogWarning("Error: None AnimationClip active. Open the Animation window & select a GameObject with an animation on it.");
                return;
            }

            GameObject root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogWarning("Error: None GameObject selected.");
                return;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in children)
            {
                string path = AnimationUtility.CalculateTransformPath(t, root.transform);

                Vector3 pos = t.localPosition;
                Vector3 rot = t.localEulerAngles;
                Vector3 scale = t.localScale;

                // Position
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0, 0, pos.x));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Constant(0, 0, pos.y));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Constant(0, 0, pos.z));

                // Rotation
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), AnimationCurve.Constant(0, 0, rot.x));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0, 0, rot.y));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0, 0, rot.z));

                // Scale
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.x"), AnimationCurve.Constant(0, 0, scale.x));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.y"), AnimationCurve.Constant(0, 0, scale.y));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.z"), AnimationCurve.Constant(0, 0, scale.z));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Keyframes added for {children.Length} objets in {clip.name}");
        }

        // Récupère le clip actif dans la fenêtre Animation via réflexion
        static AnimationClip GetCurrentAnimationClip()
        {
            Type animationWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
            EditorWindow animWindow = EditorWindow.GetWindow(animationWindowType);
            if (animWindow == null) return null;

            var animEditorField = animationWindowType.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (animEditorField == null) return null;

            var animEditor = animEditorField.GetValue(animWindow);
            if (animEditor == null) return null;

            var stateProperty = animEditor.GetType().GetProperty("state", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var state = stateProperty?.GetValue(animEditor, null);
            if (state == null) return null;

            var clipProperty = state.GetType().GetProperty("activeAnimationClip", BindingFlags.Public | BindingFlags.Instance);
            return clipProperty?.GetValue(state, null) as AnimationClip;
        }
    }
}
