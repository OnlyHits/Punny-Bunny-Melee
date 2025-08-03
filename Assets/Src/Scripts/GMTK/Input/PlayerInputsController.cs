using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class PlayerInputsController : AInputManager
    {
        #region ACTIONS
        private InputAction m_moveAction;
        private InputAction m_fireAction;
        private InputAction m_switchWeaponAction;
        private InputAction m_pauseAction;

        #endregion ACTIONS

        #region CALLBACKS
        public Action<InputType, Vector2> onMoveAction;
        public Action<InputType, bool> onFireAction;
        public Action<InputType, float> onSwitchWeaponAction;
        public Action<InputType, bool> onPauseAction;

        #endregion CALLBACKS

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void LateInit(params object[] parameters)
        {
            InitInputActions();
        }
        public override void Init(params object[] parameters)
        {
            FindAction();
        }
        #endregion

        private void FindAction()
        {
            m_moveAction = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Move", true);
            m_fireAction = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Fire", true);
            //m_counterAction = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Counter", true);
            m_switchWeaponAction = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/SwitchWeapon", true);
            m_pauseAction = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Pause", true);
        }

        private void InitInputActions()
        {
            InputActionStruct<Vector2> iMove = new InputActionStruct<Vector2>(m_moveAction, onMoveAction, Vector2.zero, true);
            InputActionStruct<bool> iFire = new InputActionStruct<bool>(m_fireAction, onFireAction, false);
            InputActionStruct<float> iSwitchWeapon = new InputActionStruct<float>(m_switchWeaponAction, onSwitchWeaponAction, 0, false);
            InputActionStruct<bool> iPause = new InputActionStruct<bool>(m_pauseAction, onPauseAction, false, false);

            m_inputActionStructsV2.Add(iMove);
            m_inputActionStructsBool.Add(iFire);
            m_inputActionStructsFloat.Add(iSwitchWeapon);
            m_inputActionStructsBool.Add(iPause);
        }

        public override void Pause(bool pause)
        {
            base.Pause(pause);
        }
    }
}