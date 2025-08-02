using CustomArchitecture;
using UnityEngine;

using static CustomArchitecture.CustomArchitecture;

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

            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onMoveAction += OnMove;
        }

        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate() { }

        protected override void OnUpdate()
        {
            Vector2 mouseDirection = Input.mousePosition;
            player.Rotate(mouseDirection);

            if (!player.IsMoving)
            {
                player.NoMove();
            }
        }
        #endregion BaseBehaviour_Cb

        private void OnMove(InputType input, Vector2 vector)
        {
            switch (input)
            {
                case InputType.PRESSED:
                    player.StartMove();
                    break;
                case InputType.COMPUTED:
                    player.Move(vector);
                    break;
                case InputType.RELEASED:
                    player.StopMove();
                    break;
                default:
                    break;
            }
        }
    }
}