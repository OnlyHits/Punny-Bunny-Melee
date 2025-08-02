using UnityEngine;
using CustomArchitecture;

namespace GMTK
{
    public class GMTKGameCore : AGameCore<GMTKGameCore>
    {

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
