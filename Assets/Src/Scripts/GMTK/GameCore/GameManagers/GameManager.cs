using System.Collections;
using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public class GameManager : BaseBehaviour
    {
        //[SerializeField] private GameBackground m_gameBackground;

        //public CinemachineCameraExtended GetBgCinemachineCamera() => m_gameBackground.GetCinemachineCamera();
        //public GameBackground GetGameBackground() => m_gameBackground;

        public IEnumerator Load()
        {
            yield return null;
        }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            //m_gameBackground.LateInit();
        }
        public override void Init(params object[] parameters)
        {
            //m_gameBackground.Init();
        }
        #endregion
    }
}