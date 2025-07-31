using UnityEngine;
using CustomArchitecture;
using UnityEngine.Rendering.Universal;

namespace Comic
{
    public class GMTKCinemachineMgr : CinemachineMgr<GMTKCinemachineMgr>
    {
        [SerializeField, Space] private Camera m_mainCamera;
        public Camera MainCamera { get { return m_mainCamera; } }

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
        }
        #endregion

        #region Camera stacking
        public void RegisterCameraStack(Camera camera)
        {
            var baseData = m_mainCamera.GetUniversalAdditionalCameraData();

            if (!baseData.cameraStack.Contains(camera))
            {
                baseData.cameraStack.Add(camera);
            }
        }
        #endregion
    }
}