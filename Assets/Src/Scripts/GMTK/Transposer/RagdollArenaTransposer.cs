using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMTK
{
    public class RagdollArenaTransposer : MonoBehaviour
    {
        // @note: Has to be the parent ragdoll
        public Transform m_mainTransfrom;

        // @note: does not container mainTransform
        public List<Transform> m_ragdollTransfroms;
        private List<Vector3> m_offsetTmps = new List<Vector3>();
        private Vector3 m_lastPosition;
        private Action<Vector3> m_onTeleport;

        public void RegisterOnTeleport(Action<Vector3> callback)
        {
            m_onTeleport -= callback;
            m_onTeleport += callback;
        }
        private void Awake()
        {
            m_lastPosition = m_mainTransfrom.transform.position;
        }

        private void LateUpdate()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null)
            {
                return;
            }

            Vector3 pos = m_mainTransfrom.transform.position;
            (Vector3 center, Vector3 size) datas = GMTKGameCore.Instance.MainGameMode.GetArenaTransposerDatas();

            float halfWidth = datas.size.x / 2f;
            float halfLength = datas.size.z / 2f;

            Vector3 center = datas.center;

            float minX = center.x - halfWidth;
            float maxX = center.x + halfWidth;
            float minZ = center.z - halfLength;
            float maxZ = center.z + halfLength;

            if (pos.x < minX)
            {
                pos.x = maxX;
            }
            else if (pos.x > maxX)
            {
                pos.x = minX;
            }

            if (pos.z < minZ)
            {
                pos.z = maxZ;
            }
            else if (pos.z > maxZ)
            {
                pos.z = minZ;
            }

            if (m_mainTransfrom.transform.position != pos)
            {
                m_onTeleport?.Invoke(pos);
            }


            m_offsetTmps.Clear();
            foreach (Transform tr in m_ragdollTransfroms)
            {
                Vector3 mainPos = m_mainTransfrom.transform.position;
                Vector3 offset = tr.position - m_mainTransfrom.transform.position;
                m_offsetTmps.Add(offset);
            }

            m_mainTransfrom.transform.position = pos;

            for (int i = 0; i < m_ragdollTransfroms.Count; ++i)
            {
                Transform tr = m_ragdollTransfroms[i];
                Vector3 destPos = pos;
                tr.position = destPos + m_offsetTmps[i];
            }

        }
    }
}