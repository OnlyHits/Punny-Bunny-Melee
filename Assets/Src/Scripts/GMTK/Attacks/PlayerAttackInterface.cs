using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class PlayerAttackInterface : AttackInterface
    {
        [SerializeField] private float m_outerRepulsionRadius;
        [SerializeField] private float m_innerRepulsionRadius;
        [SerializeField, ReadOnly] private Collider[] m_hits = new Collider[100];

        public override void Init(params object[] parameters)
        {
            base.Init(parameters);


            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFireAction += OnFire;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onSwitchWeaponAction += OnSwitchWeapon;
        }

        private void OnFire(InputType input, bool b)
        {
            if (m_player.IsRagdoll)
                return;

            if (input == InputType.RELEASED)
            {
                TryAttack();
            }
        }

        private void OnSwitchWeapon(InputType input, float b)
        {
            if (m_player.IsRagdoll)
                return;

            if ((int)b > 0)
            {
                ChangeAttack(GetIndex() == 3 ? 0 : GetIndex() + 1);
            }
            else if ((int)b < 0)
            {
                ChangeAttack(GetIndex() == 0 ? 3 : GetIndex() - 1);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_outerRepulsionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_innerRepulsionRadius);
        }
    }
}