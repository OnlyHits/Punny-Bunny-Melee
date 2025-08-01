using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

namespace GMTK
{
    public class ArenaTransposer : MonoBehaviour
    {
        private Vector3 m_lastPosition;
        private Action<Vector3> m_onTeleport;

        public void RegisterOnTeleport(Action<Vector3> callback)
        {
            m_onTeleport -= callback;
            m_onTeleport += callback;
        }

        private void Awake()
        {
            m_lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null)
                return;

            Vector3 pos = transform.position;
            (Vector3 center, Vector3 size) datas = GMTKGameCore.Instance.MainGameMode.GetArenaTransposerDatas();

            float halfWidth = datas.size.x / 2f;
            float halfLength = datas.size.z / 2f;

            Vector3 center = datas.center;

            float minX = center.x - halfWidth;
            float maxX = center.x + halfWidth;
            float minZ = center.z - halfLength;
            float maxZ = center.z + halfLength;

            if (pos.x < minX)
                pos.x = maxX;
            else if (pos.x > maxX)
                pos.x = minX;

            if (pos.z < minZ)
                pos.z = maxZ;
            else if (pos.z > maxZ)
                pos.z = minZ;

            if (transform.position != pos)
                m_onTeleport?.Invoke(pos);

            transform.position = pos;
        }
    }
}