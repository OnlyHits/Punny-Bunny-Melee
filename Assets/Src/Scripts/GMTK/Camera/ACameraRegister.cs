using UnityEngine;
using System.Collections.Generic;
using CustomArchitecture;

namespace GMTK
{
    public abstract class ACameraRegister : BaseBehaviour
    {
        public List<Camera> m_cameras;

        public List<Camera> GetCameras() => m_cameras;
    }
}
