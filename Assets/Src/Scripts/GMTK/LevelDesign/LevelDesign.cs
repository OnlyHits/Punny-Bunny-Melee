using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GMTK
{
    public class LevelDesign : MonoBehaviour
    {
        [SerializeField] private BoxCollider m_planeBoxCollider;

        public BoxCollider PlaneBoxCollider { get => m_planeBoxCollider; protected set { } }

#if UNITY_EDITOR
        public Vector3 GetPlaneSize()
        {
            Vector3 size = m_planeBoxCollider.size;
            size.Scale(m_planeBoxCollider.transform.lossyScale);
            return size;
        }
#endif
    }
}