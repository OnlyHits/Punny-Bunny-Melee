using System;
using System.Collections.Generic;
using System.Linq;
using CustomArchitecture;
using CustomArchitecture.ExtensionMethods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static GMTK.AttackUtils;

namespace GMTK
{
    public class PlayerStatHUD : BaseBehaviour
    {
        [Serializable]
        public class WeaponDataByType
        {
            public WeaponType type;
            public string name;
            public Sprite sprite;
            public Sprite spriteOutline;
        }

        [Serializable]
        public class WeaponData
        {
            public List<WeaponDataByType> weaponDataByType;

            public WeaponDataByType this[WeaponType type]
            {
                get
                {
                    if (weaponDataByType.IsNullOrEmpty())
                    {
                        return null;
                    }

                    return weaponDataByType?.FirstOrDefault(w => w.type == type);
                }
            }

        }
        [SerializeField, ReadOnly] private Player m_player;
        [SerializeField] private WeaponData m_weaponDatas;

        [SerializeField] private TextMeshProUGUI m_tLife;
        [SerializeField] private Image m_iPreview;
        [SerializeField] private Image m_iBgColor;
        [SerializeField] private TextMeshProUGUI m_tWeapon;
        [SerializeField] private Image m_iWeapon;
        [SerializeField] private Image m_iWeaponOutline;

        private string m_textLifeTemplate = "{v}<size=15>%</size>";


        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not Player)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            m_player = parameters[0] as Player;
            m_player.RegisterOnGetHit(OnPlayerGetHit);

            AttackInterface playerAttackManager = m_player.GetAttackManager();
            playerAttackManager?.RegisterOnSwitchWeapon(OnPlayerSwitchWeapon);
            m_iBgColor.color = m_player.GetMainColor();

            UpdateWeapon(playerAttackManager?.GetWeaponType() ?? default);
            UpdateText();
        }

        public override void LateInit(params object[] parameters) { }

        protected override void OnFixedUpdate() { }

        protected override void OnLateUpdate() { }

        protected override void OnUpdate() { }

        #endregion

        private void OnPlayerGetHit()
        {
            UpdateText();
        }

        private void OnPlayerSwitchWeapon(WeaponType weaponType)
        {
            UpdateWeapon(weaponType);
        }

        private void UpdateWeapon(WeaponType weaponType)
        {
            m_iWeapon.sprite = m_weaponDatas[weaponType]?.sprite;
            m_iWeaponOutline.sprite = m_weaponDatas[weaponType]?.spriteOutline;

            m_tWeapon.text = m_weaponDatas[weaponType]?.name;
        }

        private void UpdateText()
        {
            string str = m_textLifeTemplate;

            str = str.Replace("{v}", m_player.GetPrcntDamages().ToString("0"));
            m_tLife.text = str;
        }
    }
}
