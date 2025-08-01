using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GMTK
{
    public class PlayerController : BaseBehaviour
    {
        public Player player;
        private PlayerAttackInterface m_attackInterface;

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
            if (ComponentUtils.GetOrCreateComponent<PlayerAttackInterface>(gameObject, out m_attackInterface))
            {
                m_attackInterface.Init(parameters[0], player);
            }
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                player.OnGetHit();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                player.StartMove();
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                player.StopMove();
            }


            if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
            {
                player.Move();
            }
            else
            {
                player.NoMove();
            }
        }
        #endregion BaseBehaviour_Cb
    }
}