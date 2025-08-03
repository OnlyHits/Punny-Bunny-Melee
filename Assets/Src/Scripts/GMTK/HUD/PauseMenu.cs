using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using CustomArchitecture.ExtensionMethods;
using Sirenix.OdinInspector;

using static GMTK.AttackUtils;
using static CustomArchitecture.CustomArchitecture;
using UnityEngine.UI;
using System;

namespace GMTK
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Button m_bResume;
        [SerializeField] private Button m_bSettings;
        [SerializeField] private Button m_bQuitMainMenu;
        [SerializeField] private Button m_bQuitApp;

        private Action m_onResume;

        public void RegisterOnResume(Action function)
        {
            m_onResume -= function;
            m_onResume += function;
        }

        private void Awake()
        {
            m_bResume.onClick.AddListener(() =>
            {
                EnableAllButtons(false);
                m_onResume?.Invoke();
            });

            m_bSettings.onClick.AddListener(() =>
            {
                EnableAllButtons(false);
                // settings
            });
            m_bQuitMainMenu.onClick.AddListener(() =>
            {
                EnableAllButtons(false);
                GMTK.GMTKGameCore.Instance.StartGameMode<MainMenuGameMode>();
            });
            m_bQuitApp.onClick.AddListener(() =>
            {
                EnableAllButtons(false);
                Application.Quit();
            });
        }

        private void OnEnable()
        {
            EnableAllButtons(true);
        }

        private void EnableAllButtons(bool enable)
        {
            m_bResume.interactable = enable;
            m_bSettings.interactable = enable;
            m_bQuitMainMenu.interactable = enable;
            m_bQuitApp.interactable = enable;
        }
    }
}