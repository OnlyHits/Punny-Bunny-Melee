using System;
using Sirenix.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using CustomArchitecture;

namespace GMTK
{
    // use [OdinSerialize] if you want to serialize a dictionary

    [System.Serializable]
    public class GameConfigData
    {
    }

    [CreateAssetMenu(fileName = "GameConfig", menuName = "Comic/GameConfig")]
    [System.Serializable]
    public class GameConfig : SerializedScriptableObject
    {
        [NonSerialized] private readonly SaveUtils<GameConfigData> m_saveUtilitary;

        [OdinSerialize, ShowInInspector] public GameConfigData m_config;

        public GameConfig()
        {
            m_saveUtilitary = new SaveUtils<GameConfigData>("GameConfig", FileType.ConfigFile);

            Load();
        }

        [Button("Save")]
        private void SaveData()
        {
            Save();
            Debug.Log("Data saved successfully!");
        }

        [Button("Load")]
        private void LoadData()
        {
            Load();
            Debug.Log("Data loaded successfully!");
        }

        public GameConfigData GetConfig()
        {
            return m_config;
        }

        public void Save()
        {
            m_saveUtilitary.Save(m_config);
        }

        public void Load()
        {
            m_config = m_saveUtilitary.Load();
        }
    }
}