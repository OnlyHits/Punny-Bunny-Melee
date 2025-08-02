using System.Collections;
using CustomArchitecture;
using UnityEngine;

using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class PlayerController : Player
    {
        private PlayerAttackInterface m_attackInterface = null;

        #region BaseBehaviour_Cb
        public IEnumerator Load()
        {
            if (ComponentUtils.GetOrCreateComponent<PlayerAttackInterface>(gameObject, out m_attackInterface))
            {
                yield return StartCoroutine(m_attackInterface.Load());
            }
        }

        public override void Init(params object[] parameters)
        {
            m_attackInterface?.Init(parameters[0], this);

            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onMoveAction += OnMove;
        }

        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate() { }

        protected override void OnUpdate()
        {
            Vector2 mouseDirection = Input.mousePosition;
            Rotate(mouseDirection);

            if (!IsMoving)
            {
                NoMove();
            }
        }
        #endregion BaseBehaviour_Cb

        private void OnMove(InputType input, Vector2 vector)
        {
            switch (input)
            {
                case InputType.PRESSED:
                    StartMove();
                    break;
                case InputType.COMPUTED:
                    Move(vector);
                    break;
                case InputType.RELEASED:
                    StopMove();
                    break;
                default:
                    break;
            }
        }
    }
}