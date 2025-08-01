using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public class oui : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Collide");
        }
    }
}