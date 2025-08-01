using System.Collections;
using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public class GameManager : BaseBehaviour
    {
        ProjectileManager       m_projectileManager;

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
            m_projectileManager?.LateInit();
        }
        public override void Init(params object[] parameters)
        {
            if (TryGetComponent<ProjectileManager>(out m_projectileManager))
            {
                m_projectileManager.Init();
            }
        }
        #endregion
    }
}