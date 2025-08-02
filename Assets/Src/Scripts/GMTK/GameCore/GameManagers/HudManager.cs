using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using CustomArchitecture.ExtensionMethods;

namespace GMTK
{
    public class HudManager : BaseBehaviour
    {
        [SerializeField] private PlayerStatHUD m_playerStatPrefab;
        [SerializeField] private List<RectTransform> m_statRectRefs;
        [SerializeField, ReadOnly] private List<PlayerStatHUD> m_playerStats;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }

        public override void LateInit(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not GameManager)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            GameManager gameManager = parameters[0] as GameManager;

            for (int i = 0; i < gameManager.GetPlayerCount(); ++i)
            {
                InstantiatePlayerStat(m_statRectRefs[i], gameManager.GetPlayers()[i]);
            }

        }

        public override void Init(params object[] parameters) { }

        #endregion

        private void InstantiatePlayerStat(RectTransform rect, Player playerAssigned)
        {
            PlayerStatHUD stat = Instantiate(m_playerStatPrefab, rect.parent);
            stat.GetComponent<RectTransform>().CopyFullRectTransform(rect);
            stat.Init(playerAssigned);

            m_playerStats.Add(stat);
        }
    }
}