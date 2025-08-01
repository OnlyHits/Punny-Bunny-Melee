using CustomArchitecture;
using System.Collections;
using static GMTK.AttackUtils;
using UnityEngine;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using System.Runtime.CompilerServices;

namespace GMTK
{
    public class PlayerAttackInterface : AttackInterface
    {
        private Player m_player = null;
        private string m_projectileLayer = "PlayerProjectile";

        public override void Init(params object[] parameters)
        {
            base.Init(parameters);

            if (parameters.Length < 2 || parameters[1] is not Player)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            m_player = (Player)parameters[1];

            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire1 += OnFire1;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire2 += OnFire2;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire3 += OnFire3;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire4 += OnFire4;
        }

        private void OnFire1(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.LastDirection, 0, LayerMask.NameToLayer(m_projectileLayer));
            }
        }

        private void OnFire2(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.LastDirection, 1, LayerMask.NameToLayer(m_projectileLayer));
            }
        }

        private void OnFire3(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.LastDirection, 2, LayerMask.NameToLayer(m_projectileLayer));
            }
        }

        private void OnFire4(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.LastDirection, 3, LayerMask.NameToLayer(m_projectileLayer));
            }
        }
    }
}