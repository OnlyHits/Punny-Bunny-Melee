using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AI;
using UnityEngine;
using Unity.VisualScripting;
using Unity.AI.Navigation;
using UnityEditor.Build.Pipeline;
using CustomArchitecture;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GMTK
{
    public class LevelDesign : MonoBehaviour
    {
        [SerializeField] private BoxCollider m_planeBoxCollider;
        
        public BoxCollider PlaneBoxCollider { get => m_planeBoxCollider; protected set { } }
        public Vector3 GetPlaneSize()
        {
            Vector3 size = m_planeBoxCollider.size;
            size.Scale(m_planeBoxCollider.transform.lossyScale);
            return size;
        }
    }
}