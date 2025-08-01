using CustomArchitecture;
using System.Collections;
using static GMTK.AttackUtils;
using UnityEngine;
using System.Collections.Generic;

namespace GMTK
{
    public class AttackLoader : MonoBehaviour
    {
        private Dictionary<BulletType, GameObject> m_bulletPrefabs = new();

        // It is a cache value, use for instantiation of bullet pool
        public Dictionary<BulletType, GameObject> GetPrefabs() => m_bulletPrefabs;

        public IEnumerator Load()
        {
            int total = s_bulletPath.Count;
            int completed = 0;

            foreach (var kvp in s_bulletPath)
            {
                BulletType bulletType = kvp.Key;
                string address = kvp.Value;

                yield return AddressableFactory.CreatePrefabAsync(address, (prefab) =>
                {
                    if (prefab != null)
                    {
                        m_bulletPrefabs[bulletType] = prefab;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load bullet prefab for {bulletType} at address: {address}");
                    }
                    completed++;
                });
            }

            while (completed < total)
                yield return null;
        }
    }
}