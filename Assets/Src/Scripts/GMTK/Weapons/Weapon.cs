using CustomArchitecture;
using CustomArchitecture.ExtensionMethods;

namespace GMTK
{
    public class Weapon : BaseBehaviour
    {
        private Player m_player;

        #region Unity_Cb
        private void Awake()
        {
            gameObject.TryGetComponentInParent(out m_player);
        }

        #endregion

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters) { }
        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate() { }
        protected override void OnUpdate()
        {
            transform.forward = m_player.transform.forward;
        }
        #endregion BaseBehaviour_Cb

    }
}