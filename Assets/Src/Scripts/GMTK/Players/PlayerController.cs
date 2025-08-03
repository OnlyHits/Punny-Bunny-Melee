using System.Collections;
using CustomArchitecture;
using UnityEngine;

using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class PlayerController : Player
    {
        private PlayerAttackInterface m_attackInterface = null;

        public override AttackInterface GetAttackManager() => m_attackInterface;

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
            base.Init();
            m_attackInterface?.Init(parameters[0], this);

            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onMoveAction += OnMove;
        }

        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { base.OnFixedUpdate(); }
        protected override void OnLateUpdate() { base.OnLateUpdate(); }

        protected override void OnUpdate()
        {
            if (IsRagdoll)
                return;

            base.OnUpdate();

            Vector2 mouseDirection = Input.mousePosition;
            Rotate(mouseDirection);

            if (!IsMoving)
            {
                NoMove();
            }
        }
        #endregion BaseBehaviour_Cb

        protected override void GetHit(Collision collision)
        {
            m_attackInterface.StopAllAttacks();

            base.GetHit(collision);
        }

        private void OnMove(InputType input, Vector2 vector)
        {
            if (IsRagdoll)
                return;

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