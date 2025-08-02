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
        private InputAction m_switchWeapon;
        private InputAction m_counter;

        #endregion ACTIONS

        #region CALLBACKS
        public Action<InputType, Vector2> onMoveAction;
        public Action<InputType, bool> onFire;
        public Action<InputType, float> onSwitchWeapon;
        public Action<InputType, bool> onCounter;

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
            m_counter = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/Counter", true);
            m_switchWeapon = GMTKGameCore.Instance.GetInputAsset().FindAction("Player/SwitchWeapon", true);
        }

        private void InitInputActions()
        {
            InputActionStruct<Vector2> iMove = new InputActionStruct<Vector2>(m_moveAction, onMoveAction, Vector2.zero, true);
            InputActionStruct<bool> iFire = new InputActionStruct<bool>(m_fireAction, onFire, false);
            InputActionStruct<bool> iCounter = new InputActionStruct<bool>(m_counter, onCounter, false);
            InputActionStruct<float> iSwitchWeapon = new InputActionStruct<float>(m_switchWeapon, onSwitchWeapon, 0, true);

            m_inputActionStructsV2.Add(iMove);
            m_inputActionStructsBool.Add(iFire);
            m_inputActionStructsFloat.Add(iSwitchWeapon);
            m_inputActionStructsBool.Add(iCounter);
        }

        public override void Pause(bool pause)
        {
            base.Pause(pause);
        }
    }
}