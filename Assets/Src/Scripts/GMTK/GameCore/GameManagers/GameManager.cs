using System.Collections;
using System.Collections.Generic;
using CustomArchitecture;
using CustomArchitecture.ExtensionMethods;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GMTK
{
    public class GameManager : BaseBehaviour
    {
        private ProjectileManager m_projectileManager;

        [Title("Level")]
        [SerializeField] private Transform m_arenaTransform;
        [SerializeField] private Vector3 m_mapBounds = new Vector3(10f, 1f, 10f);

        [Title("Players")]
        [SerializeField] private PlayerController m_playerController;
        [SerializeField] private List<AIController> m_aiControllers;

        public ProjectileManager GetProjectileManager() => m_projectileManager;
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
            if (parameters.Length < 1 || parameters[0] is not AttackLoader)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            if (TryGetComponent<ProjectileManager>(out m_projectileManager))
            {
                m_projectileManager.Init(parameters[0]);
            }

            if (m_playerController != null)
            {
                m_playerController.Init(m_projectileManager);
            }

            if (!m_aiControllers.IsNullOrEmpty())
            {
                foreach (var ai in m_aiControllers)
                    ai.Init(m_projectileManager);
            }
        }
        #endregion
    }
}