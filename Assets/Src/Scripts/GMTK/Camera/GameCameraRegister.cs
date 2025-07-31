using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using CustomArchitecture;
using UnityEngine.Rendering;

namespace GMTK
{
    public class GameCameraRegister : ACameraRegister
    {
        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            //foreach (var camera in m_cameras)
            //{
            //    camera.nearClipPlane = -10f;
            //    camera.farClipPlane = 5000f;
            //}
        }
        #endregion
    }
}
