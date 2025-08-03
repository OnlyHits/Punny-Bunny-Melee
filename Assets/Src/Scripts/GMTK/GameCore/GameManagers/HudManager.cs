using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using CustomArchitecture.ExtensionMethods;
using Sirenix.OdinInspector;

using static GMTK.AttackUtils;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class HudManager : BaseBehaviour
    {
        [Title("Pause")]
        [SerializeField] private PauseMenu m_pauseMenu;

        [Title("Win & Lose")]
        [SerializeField] private WinMenu m_winMenu;
        [SerializeField] private LoseMenu m_loseMenu;

        [Title("Cursor")]
        [SerializeField] private Transform m_cursor;
        // @note: time to complete one rotation
        [SerializeField] public float m_rotationDuration = 3f;
        [SerializeField] private float m_timerSpeedAfterShoot = 0.5f;
        [SerializeField] private float m_speedAfterShoot = 2f;
        private float m_angleCursor;
        private float m_timerAfterShoot;
        private float lerpT;

        [Title("Players Stats")]
        [SerializeField] private PlayerStatHUD m_playerStatPrefab;
        [SerializeField] private List<RectTransform> m_statRectRefs;
        [SerializeField, ReadOnly] private List<PlayerStatHUD> m_playerStats;
        public bool Paused { get { return m_paused; } private set { m_paused = value; } }
        private bool m_paused = false;
        private bool m_hasWin = false;
        private bool m_hasLose = false;


        #region BaseBehaviour
        public override void Init(params object[] parameters)
        {
            Cursor.visible = false;

            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onPauseAction += OnPause;

            m_pauseMenu.RegisterOnResume(() => { TogglePause(); });
            m_pauseMenu.RegisterOnQuitMainMenu(() => { Time.timeScale = 1; });

            m_loseMenu.RegisterOnQuitMainMenu(() => { Time.timeScale = 1; });
            m_winMenu.RegisterOnQuitMainMenu(() => { Time.timeScale = 1; });

            m_loseMenu.RegisterOnRestart(() => { Time.timeScale = 1; });
            m_winMenu.RegisterOnRestart(() => { Time.timeScale = 1; });

            m_pauseMenu.gameObject.SetActive(false);
            m_loseMenu.gameObject.SetActive(false);
            m_winMenu.gameObject.SetActive(false);
        }

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


        protected override void OnFixedUpdate() { }
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
            Cursor.visible = false;
        }

        #endregion

        #region Win_Lose
        public void Win()
        {
            if (m_hasWin || m_hasLose)
            {
                return;
            }
            m_hasWin = true;
            StartCoroutine(CoroutineUtils.InvokeOnDelay(1f, () =>
            {
                Time.timeScale = 0;
                m_winMenu.gameObject.SetActive(true);
            }));
        }

        public void Lose()
        {
            if (m_hasWin || m_hasLose)
            {
                return;
            }
            m_hasLose = true;
            StartCoroutine(CoroutineUtils.InvokeOnDelay(1f, () =>
            {
                Time.timeScale = 0;
                m_loseMenu.gameObject.SetActive(true);
            }));
        }
        #endregion

        #region Pause
        public void TogglePause()
        {
            m_paused = !m_paused;

            GMTK.GMTKGameCore.Instance.GetGameMode<MainGameMode>().GetGameManager().Pause(m_paused);
            m_pauseMenu.gameObject.SetActive(m_paused);
            Time.timeScale = m_paused ? 0 : 1f;
            //Cursor.visible = m_paused;
            //m_cursor.gameObject.SetActive(!m_paused);

            if (m_paused)
            {
                m_cursor.transform.localScale = Vector3.one * 0.6f;
                m_cursor.transform.localEulerAngles = new Vector3(0, 0, 45f);
            }
            else
            {
                m_cursor.transform.localScale = Vector3.one;
            }
        }

        private void OnPause(InputType inputType, bool b)
        {
            if (inputType == InputType.PRESSED)
            {
                TogglePause();
            }
        }
        #endregion

        #region Cursor
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
        #endregion

        #region Player_Stats
        private void InstantiatePlayerStat(RectTransform rect, Player playerAssigned)
        {
            PlayerStatHUD stat = Instantiate(m_playerStatPrefab, rect.parent);
            stat.GetComponent<RectTransform>().CopyFullRectTransform(rect);
            stat.Init(playerAssigned);

            m_playerStats.Add(stat);
        }
        #endregion
    }
}