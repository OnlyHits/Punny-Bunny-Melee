using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using CustomArchitecture.ExtensionMethods;
using Sirenix.OdinInspector;

using static GMTK.AttackUtils;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class LoseMenu : EndMenu
    {
        private List<string> m_strs = new List<string>()
        {
            "You Lose",
        };

        private void OnEnable()
        {
            m_text.text = GetRandomSentence();
        }

        private string GetRandomSentence()
        {
            return m_strs[Random.Range(0, m_strs.Count)];
        }
    }
}
