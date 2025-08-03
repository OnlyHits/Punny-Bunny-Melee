using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class AIAttackInterface : AttackInterface
    {
        private Player m_player = null;
        private string m_projectileLayerName = GmtkUtils.PlayerAIProjectile_Layer;

        public override void Init(params object[] parameters)
        {
            base.Init(parameters);

            if (parameters.Length < 2 || parameters[1] is not Player)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            m_player = (Player)parameters[1];
        }

        public bool Fire()
        {
            return TryAttack(LayerMask.NameToLayer(m_projectileLayerName));
        }

        public void SwitchWeapon()
        {
            ChangeAttack(GetIndex() == 1 ? 0 : GetIndex() + 1);
        }
    }
}