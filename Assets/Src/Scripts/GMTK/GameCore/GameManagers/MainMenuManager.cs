using CustomArchitecture;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CustomArchitecture.ExtensionMethods;
using Sirenix.OdinInspector;
using DG.Tweening;

using static GMTK.AttackUtils;

namespace GMTK
{
    public class MainMenuManager : BaseBehaviour
    {
        [Title("Cursor")]
        [SerializeField] private Transform m_cursor;

        [Title("Buttons")]
        [SerializeField] private Button m_bPlay;
        [SerializeField] private Button m_bSettings;
        [SerializeField] private Button m_bQuit;

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
            //Cursor.visible = false;
        }
        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate()
        {
            m_cursor.transform.position = Input.mousePosition;
        }
        protected override void OnUpdate()
        {
            if (Input.GetMouseButtonUp(0))
            {
                m_cursor.DOKill();
                m_cursor.transform.DOScale(Vector3.one * 0.85f, 0.075f)
                    .From(Vector3.one)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }

        #endregion

        #region Unity_Cb
        private void Awake()
        {
            m_bPlay.onClick.AddListener(() =>
            {
                EnableAllButtons(false); StartCoroutine(CoroutineUtils.InvokeOnDelay(0.3f, () =>
                {
                    EnableAllButtons(true);
                    Debug.Log("Play");
                    GMTK.GMTKGameCore.Instance.StartGameMode<MainGameMode>();
                }));
            });
            m_bSettings.onClick.AddListener(() =>
            {
                EnableAllButtons(false); StartCoroutine(CoroutineUtils.InvokeOnDelay(0.3f, () =>
                {
                    EnableAllButtons(true);
                    Debug.Log("Settings");
                }));
            });
            m_bQuit.onClick.AddListener(() =>
            {
                EnableAllButtons(false); StartCoroutine(CoroutineUtils.InvokeOnDelay(0.3f, () =>
                {
                    EnableAllButtons(true);
                    Application.Quit();
                }));
            });
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
        }

        #endregion

        private void EnableAllButtons(bool enable)
        {
            m_bPlay.interactable = enable;
            m_bSettings.interactable = enable;
            m_bQuit.interactable = enable;
        }

    }
}