using System.Collections;
using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public class GameManager : BaseBehaviour
    {
        private ProjectileManager m_projectileManager;
        [SerializeField] private Transform m_arenaTransform;
        [SerializeField] private Vector3 m_mapBounds = new Vector3(10f, 1f, 10f);

        public (Vector3, Vector3) GetArenaTransposerDatas() => (m_arenaTransform.position, Vector3.Scale(m_arenaTransform.localScale, m_mapBounds));

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