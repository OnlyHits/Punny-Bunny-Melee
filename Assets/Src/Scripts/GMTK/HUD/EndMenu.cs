using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using System;

namespace GMTK
{
    public class EndMenu : BaseBehaviour
    {
        [SerializeField] protected TextMeshProUGUI m_text;

        [Space]
        [SerializeField] private Button m_bRetry;
        [SerializeField] private Button m_bMainMenu;

        [Title("Decal")]
        [SerializeField] private Image m_decalPrefab;
        private List<Image> m_decals = new List<Image>();


        private Action m_onQuitMainMenu;
        private Action m_onRetry;

        public void RegisterOnQuitMainMenu(Action function)
        {
            m_onQuitMainMenu -= function;
            m_onQuitMainMenu += function;
        }

        public void RegisterOnRestart(Action function)
        {
            m_onRetry -= function;
            m_onRetry += function;
        }

        private void Awake()
        {
            m_bRetry.onClick.AddListener(() =>
            {
                m_onRetry?.Invoke();
                SpawnDecal(m_bRetry);
                StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                {
                    //GMTK.GMTKGameCore.Instance.DestroyGameMode<MainGameMode>();
                    //GMTK.GMTKGameCore.Instance.StartGameMode<MainGameMode>();
                    GMTK.GMTKGameCore.Instance.MainGameMode.ReplayGame();
                }));
            });

            m_bMainMenu.onClick.AddListener(() =>
            {
                m_onQuitMainMenu?.Invoke();
                SpawnDecal(m_bRetry);
                StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                {
                    //GMTK.GMTKGameCore.Instance.DestroyGameMode<MainGameMode>();
                    //GMTK.GMTKGameCore.Instance.StartGameMode<MainMenuGameMode>();
                    GMTK.GMTKGameCore.Instance.MainGameMode.GoMainMenu();
                }));
            });
        }

        public override void Init(params object[] parameters)
        {
        }

        public override void LateInit(params object[] parameters)
        {
        }

        protected override void OnFixedUpdate()
        {
        }

        protected override void OnLateUpdate()
        {
        }

        protected override void OnUpdate()
        {
        }

        private void OnEnable()
        {
            EnableAllButtons(true);
        }

        private void OnDisable()
        {
            RemoveDecals();
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

        private void EnableAllButtons(bool enable)
        {
            m_bRetry.interactable = enable;
            m_bMainMenu.interactable = enable;
        }

        private void OnDestroy()
        {
            m_onRetry = null;
            m_onQuitMainMenu = null;
        }

    }
}