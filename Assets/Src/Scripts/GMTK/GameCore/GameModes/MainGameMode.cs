using System;
using Sirenix.OdinInspector;
using CustomArchitecture;
using UnityEngine;
using System.Collections;

using static CustomArchitecture.CustomArchitecture;
using static GMTK.GmtkUtils;
using DG.Tweening;
using Unity.Cinemachine;
using System.Linq;

namespace GMTK
{
    public class MainGameMode : AGameMode<GMTKGameCore>
    {
        public enum GameState
        {
            None,
            Running,
            Win,
            Lose
        }

#if UNITY_EDITOR && !DEVELOPMENT_BUILD
        private bool m_playStartAnimation_DEBUG = true;
#endif

        // ---- Config & Save ____
        private GameConfig m_gameConfig;
        private AttackLoader m_attackLoader; // cached datas

        // ---- Input ----
        private PlayerInputsController m_playerInput;

        // ---- Local datas ----
        private HudManager m_hudManager;
        private GameManager m_gameManager;
        private ArenaManager m_arenaManager;

        private GameState m_gameState = GameState.None;

        // ---- Managers ----
        public GameManager GetGameManager() => m_gameManager;
        public HudManager GetHudManager() => m_hudManager;
        public GameConfig GetGameConfig() => m_gameConfig;
        public PlayerInputsController GetPlayerInput() => m_playerInput;
        public ArenaManager ArenaManager() => m_arenaManager;

        public (Vector3, Vector3) GetArenaTransposerDatas() => m_arenaManager.GetArenaTransposerDatas();

        public override void InitGameMode(params object[] parameters)
        {
            base.InitGameMode(parameters);

            m_gameSceneName = GameSceneName;
            m_uiSceneName = HudSceneName;

            m_gameConfig = SerializedScriptableObject.CreateInstance<GameConfig>();

            m_gameCore.GetGlobalInput().onPause += OnPause;
        }

        // todo : check if resources.Load is done on one frame or multiple
        // Resource.Load is obsolete, now pass by AddressableFactory
        // Add Load function to ABehaviour, or make a inheritance
        public override IEnumerator LoadGameMode()
        {
            m_hudManager = ComponentUtils.FindObjectAcrossScenes<HudManager>();
            m_gameManager = ComponentUtils.FindObjectAcrossScenes<GameManager>();
            m_arenaManager = ComponentUtils.FindObjectAcrossScenes<ArenaManager>();

            ComponentUtils.GetOrCreateComponent<PlayerInputsController>(gameObject, out m_playerInput);

            if (ComponentUtils.GetOrCreateMonoBehavior<AttackLoader>(gameObject, out m_attackLoader))
                yield return StartCoroutine(m_attackLoader.Load());

            if (m_gameManager != null)
                yield return StartCoroutine(m_gameManager.Load());
            if (m_hudManager != null)
                yield return StartCoroutine(m_gameManager.Load());

            m_playerInput?.Init();
            m_gameManager?.Init(m_attackLoader);
            m_hudManager?.Init();

            m_playerInput.LateInit();
            m_gameManager?.LateInit();
            m_hudManager?.LateInit(m_gameManager);

            Compute = true;

#if UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (!m_playStartAnimation_DEBUG)
            {
            }
#endif

            yield return new WaitForEndOfFrame();

            m_gameCore.OnGameModeLoaded();
        }

        public override void StartGameMode()
        {
#if UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (m_playStartAnimation_DEBUG)
            {
                m_arenaManager.GetArenaCamera().PlayStartAnim(() => { m_gameState = GameState.Running; });
            }
#else
                m_arenaManager.GetArenaCamera().PlayStartAnim(() => { ; });
#endif
        }

        public void ReplayGame()
        {
            Destroy(GMTK.GMTKGameCore.Instance.GetComponent<PlayerInputsController>());
            Destroy(GMTK.GMTKGameCore.Instance.GetComponent<AttackLoader>());
            //Destroy(GMTK.GMTKGameCore.Instance.GetComponent<MainGameMode>());

            GMTK.GMTKGameCore.Instance.StartGameMode<MainGameMode>();
        }

        public void GoMainMenu()
        {
            Destroy(GMTK.GMTKGameCore.Instance.GetComponent<PlayerInputsController>());
            Destroy(GMTK.GMTKGameCore.Instance.GetComponent<AttackLoader>());
            //Destroy(GMTK.GMTKGameCore.Instance.GetComponent<MainGameMode>());

            GMTK.GMTKGameCore.Instance.StartGameMode<MainMenuGameMode>();
        }

        private void OnWin()
        {
            m_hudManager.Win();
        }

        private void OnLose()
        {
            m_hudManager.Lose();
        }

        private void Update()
        {
            if (!Compute)
                return;

            GameState state;

            if (m_gameManager.PlayerController.IsDead())
                state = GameState.Lose;
            else if (m_gameManager.AiControllers.All<AIController>(e => e.IsDead()))
                state = GameState.Win;
            else
                state = GameState.Running;

            if (state != m_gameState)
            {
                if (state == GameState.Lose)
                    OnLose();
                else if (state == GameState.Win)
                    OnWin();

                m_gameState = state;
            }
        }

        #region Pause
        private void OnPause(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
            }
        }
        #endregion Pause

        // destroy all managed objects
        public override void StopGameMode()
        {
            Compute = false;
        }

        // restart all managed gameObject or destroy & instantiate
        public override void RestartGameMode()
        {
            Compute = true;
        }
    }
}