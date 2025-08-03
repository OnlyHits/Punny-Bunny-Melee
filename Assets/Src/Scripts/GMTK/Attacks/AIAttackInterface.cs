using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class AIAttackInterface : AttackInterface
    {
        public override void Init(params object[] parameters)
        {
            base.Init(parameters);
        }

        public bool Fire()
        {
            return TryAttack();
        }

        public void SwitchWeapon()
        {
            ChangeAttack(GetIndex() == 1 ? 0 : GetIndex() + 1);
        }
    }
}