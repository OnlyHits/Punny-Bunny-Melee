using UnityEngine;

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