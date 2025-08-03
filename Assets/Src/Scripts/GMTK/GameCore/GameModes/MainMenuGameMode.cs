using CustomArchitecture;
using UnityEngine;
using System.Collections;

using static CustomArchitecture.CustomArchitecture;
using static GMTK.GmtkUtils;

namespace GMTK
{
    public class MainMenuGameMode : AGameMode<GMTKGameCore>
    {
        private MainMenuManager m_mainMenuManager;

        public override void InitGameMode(params object[] parameters)
        {
            base.InitGameMode(parameters);

            m_gameSceneName = MainMenuSceneName;
        }
        public override IEnumerator LoadGameMode()
        {
            m_mainMenuManager = ComponentUtils.FindObjectAcrossScenes<MainMenuManager>();

            m_mainMenuManager.Init();
            m_mainMenuManager.LateInit();

            Compute = true;

            yield return new WaitForEndOfFrame();

            m_gameCore.OnGameModeLoaded();
        }

        public override void RestartGameMode()
        {
        }

        public override void StartGameMode()
        {
        }

        public override void StopGameMode()
        {
        }
    }
}
