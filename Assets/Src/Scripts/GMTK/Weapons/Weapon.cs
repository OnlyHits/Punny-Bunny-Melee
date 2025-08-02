using CustomArchitecture;
using CustomArchitecture.ExtensionMethods;
using static GMTK.AttackUtils;
using UnityEngine;

namespace GMTK
{
    public class Weapon : BaseBehaviour
    {
        [SerializeField] private WeaponType m_type;
        [SerializeField] private Transform  m_firePosition;
        private Player m_player = null;

        public Vector3 GetFirePosition() => m_firePosition.position;
        public Vector3 GetFireDirection() => m_player.ShootDirection;
        public WeaponType GetWeaponType() => m_type;

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
            gameObject.TryGetComponentInParent(out m_player);
        }
        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate() { }
        protected override void OnUpdate()
        {
            // not loaded yet
            if (m_player == null)
                return;

            transform.forward = m_player.transform.forward;
        }
        #endregion BaseBehaviour_Cb

    }
}