using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GMTK
{
    public class AIController : BaseBehaviour
    {
        public Player player;

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
        }

        public override void LateInit(params object[] parameters)
        {
        }

        protected override void OnFixedUpdate()
        {
        }

        protected override void OnLateUpdate()
        {
        }

        protected override void OnUpdate()
        {
            if (!player.IsMoving)
            {
                player.NoMove();
            }
        }
        #endregion BaseBehaviour_Cb
    }
}