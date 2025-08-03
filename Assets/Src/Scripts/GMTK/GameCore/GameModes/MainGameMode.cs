using System;
using Sirenix.OdinInspector;
using CustomArchitecture;
using UnityEngine;
using System.Collections;

using static CustomArchitecture.CustomArchitecture;
using static GMTK.GmtkUtils;

namespace GMTK
{
    public class MainGameMode : AGameMode<GMTKGameCore>
    {
#if UNITY_EDITOR && !DEVELOPMENT_BUILD
        private bool m_playStartAnimation_DEBUG = false;
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
            }
#else
#endif
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