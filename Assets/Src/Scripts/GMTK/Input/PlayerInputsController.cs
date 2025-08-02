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
        private InputAction m_fireAction_1;
        private InputAction m_fireAction_2;
        private InputAction m_fireAction_3;
        private InputAction m_fireAction_4;
        private InputAction m_counter;
        private InputAction m_switchWeapon;

        #endregion ACTIONS

        #region CALLBACKS
        public Action<InputType, Vector2> onMoveAction;

        public Action<InputType, bool> onFire1;
        public Action<InputType, bool> onFire2;
        public Action<InputType, bool> onFire3;
        public Action<InputType, bool> onFire4;
        public Action<InputType, bool> onCounter;
        public Action<InputType, float> onSwitchWeapon;

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
            m_fireAction_1 = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Fire_1", true);
            m_fireAction_2 = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Fire_2", true);
            m_fireAction_3 = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Fire_3", true);
            m_fireAction_4 = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Fire_4", true);
            m_counter = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Counter", true);
            m_switchWeapon = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/SwitchWeapon", true);
        }

        private void InitInputActions()
        {
            InputActionStruct<Vector2> iMove = new InputActionStruct<Vector2>(m_moveAction, onMoveAction, Vector2.zero, true);
            InputActionStruct<bool> iFire1 = new InputActionStruct<bool>(m_fireAction_1, onFire1, false);
            InputActionStruct<bool> iFire2 = new InputActionStruct<bool>(m_fireAction_2, onFire2, false);
            InputActionStruct<bool> iFire3 = new InputActionStruct<bool>(m_fireAction_3, onFire3, false);
            InputActionStruct<bool> iFire4 = new InputActionStruct<bool>(m_fireAction_4, onFire4, false);
            InputActionStruct<bool> iCounter = new InputActionStruct<bool>(m_counter, onCounter, false);
            InputActionStruct<float> iSwitchWeapon = new InputActionStruct<float>(m_switchWeapon, onSwitchWeapon, 0, true);

            m_inputActionStructsV2.Add(iMove);
            m_inputActionStructsBool.Add(iFire1);
            m_inputActionStructsBool.Add(iFire2);
            m_inputActionStructsBool.Add(iFire3);
            m_inputActionStructsBool.Add(iFire4);
            m_inputActionStructsBool.Add(iCounter);
            m_inputActionStructsFloat.Add(iSwitchWeapon);
        }

        public override void Pause(bool pause)
        {
            base.Pause(pause);
        }
    }
}