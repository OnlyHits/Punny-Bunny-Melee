using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using CustomArchitecture.ExtensionMethods;
using Sirenix.OdinInspector;

using static GMTK.AttackUtils;

namespace GMTK
{
    public class HudManager : BaseBehaviour
    {
        [Title("Cursor")]
        [SerializeField] private Transform m_cursor;
        // @note: time to complete one rotation
        [SerializeField] public float m_rotationDuration = 3f;
        [SerializeField] private float m_timerSpeedAfterShoot = 0.5f;
        [SerializeField] private float m_speedAfterShoot = 2f;
        private float m_angleCursor;
        private float m_timerAfterShoot;
        float lerpT;

        [Title("Players Stats")]
        [SerializeField] private PlayerStatHUD m_playerStatPrefab;
        [SerializeField] private List<RectTransform> m_statRectRefs;
        [SerializeField, ReadOnly] private List<PlayerStatHUD> m_playerStats;


        #region BaseBehaviour
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

            PlayerAttackInterface playerAtkInterface = gameManager.GetPlayerUser().GetAttackManager() as PlayerAttackInterface;
            playerAtkInterface.RegisterOnShoot(OnPlayerShoot);
        }

        public override void Init(params object[] parameters)
        {
//            Cursor.visible = false;
        }

        protected override void OnFixedUpdate()
        { }

        protected override void OnLateUpdate()
        {
            m_cursor.transform.position = Input.mousePosition;
        }

        protected override void OnUpdate()
        {
            UpdateCursor();

        }
        #endregion

        #region Unity_Cb
        private void OnDestroy()
        {
            Cursor.visible = true;
        }

        #endregion

        private void UpdateCursor()
        {
            float multiplierSpeed = m_timerAfterShoot > 0f ? m_speedAfterShoot : 1f;

            m_angleCursor += 360f / m_rotationDuration * multiplierSpeed * Time.deltaTime;
            m_angleCursor %= 360f;

            float smoothAngle = Mathf.LerpAngle(
                m_cursor.transform.eulerAngles.z,
                m_angleCursor,
                Time.deltaTime * 10f // smoothing factor
            );
            m_cursor.transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

            if (m_timerAfterShoot > 0f)
            {
                m_timerAfterShoot -= Time.deltaTime;
            }
        }

        private void OnPlayerShoot(WeaponType weaponType)
        {
            m_timerAfterShoot = m_timerSpeedAfterShoot;
        }

        private void InstantiatePlayerStat(RectTransform rect, Player playerAssigned)
        {
            PlayerStatHUD stat = Instantiate(m_playerStatPrefab, rect.parent);
            stat.GetComponent<RectTransform>().CopyFullRectTransform(rect);
            stat.Init(playerAssigned);

            m_playerStats.Add(stat);
        }
    }
}