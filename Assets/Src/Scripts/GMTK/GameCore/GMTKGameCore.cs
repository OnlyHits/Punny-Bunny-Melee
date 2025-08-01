using UnityEngine;
using CustomArchitecture;

namespace GMTK
{
    public class GMTKGameCore : AGameCore<GMTKGameCore>
    {

#if UNITY_EDITOR
        [Tooltip("If you init your game or hud scene and don't want to start the main scene, check this")]
        [SerializeField] private bool m_debugStandaloneScene = false;
#endif

        public MainGameMode MainGameMode
        {
            get { return GetGameMode<MainGameMode>(); }
            protected set { }
        }

        protected override void InstantiateGameModes()
        {
            Application.targetFrameRate = 60;
            CreateGameMode<MainGameMode>();
            SetStartingGameMode<MainGameMode>();
        }
    }
}
