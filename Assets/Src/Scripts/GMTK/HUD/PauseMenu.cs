using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

using UnityEngine.UI;
using System;

namespace GMTK
{
    public class PauseMenu : MonoBehaviour
    {
        [Title("Buttons")]
        [SerializeField] private Button m_bResume;
        [SerializeField] private Button m_bSettings;
        [SerializeField] private Button m_bQuitMainMenu;
        [SerializeField] private Button m_bQuitApp;

        [Title("Decal")]
        [SerializeField] private Image m_decalPrefab;
        private List<Image> m_decals = new List<Image>();

        private Action m_onResume;
        private Action m_onQuitMainMenu;

        public void RegisterOnResume(Action function)
        {
            m_onResume -= function;
            m_onResume += function;
        }

        public void RegisterOnQuitMainMenu(Action function)
        {
            m_onQuitMainMenu -= function;
            m_onQuitMainMenu += function;
        }

        private void Awake()
        {
            m_bResume.onClick.AddListener(() =>
            {
                EnableAllButtons(false);
                //Time.timeScale = 1f;
                SpawnDecal(m_bResume);

                //StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                //{
                m_onResume?.Invoke();
                //}));
            });

            m_bSettings.onClick.AddListener(() =>
            {
                SpawnDecal(m_bSettings);
                //EnableAllButtons(false);

                //StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                //{
                //    // settings
                //}));
            });

            m_bQuitMainMenu.onClick.AddListener(() =>
            {
                SpawnDecal(m_bQuitMainMenu);
                EnableAllButtons(false);

                StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                {
                    m_onQuitMainMenu?.Invoke();
                    GMTK.GMTKGameCore.Instance.MainGameMode.GoMainMenu();
                }));
            });

            m_bQuitApp.onClick.AddListener(() =>
            {
                SpawnDecal(m_bQuitApp);
                EnableAllButtons(false);

                StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                {
                    Application.Quit();
                }));

            });
        }

        private void SpawnDecal(Button button)
        {
            Image decal = Instantiate(m_decalPrefab, button.transform);
            decal.transform.position = Input.mousePosition;
            m_decals.Add(decal);
        }

        private void RemoveDecals()
        {
            foreach (Image image in m_decals)
            {
                Destroy(image.gameObject);
            }
            m_decals.Clear();
        }

        private void OnEnable()
        {
            EnableAllButtons(true);
        }

        private void OnDisable()
        {
            RemoveDecals();
        }

        private void EnableAllButtons(bool enable)
        {
            m_bResume.interactable = enable;
            m_bSettings.interactable = enable;
            m_bQuitMainMenu.interactable = enable;
            m_bQuitApp.interactable = enable;
        }

        private void OnDestroy()
        {
            m_onResume = null;
            m_onQuitMainMenu = null;
        }
    }
}