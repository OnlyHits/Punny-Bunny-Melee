using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;

namespace GMTK
{
    public static class ProjectileUtils
    {
        public static Vector3 GetRandomDirectionInAngle(Vector3 direction, float angle)
        {
            float randomAngle = Random.Range(-angle, angle);
            Vector3 rotated = Quaternion.AngleAxis(randomAngle, Vector3.up) * direction;
            return rotated.normalized;
        }

        public static List<Vector3> GetRandomDirectionsInAngle(Vector3 direction, float angle, int nb_directions)
        {
            List<Vector3> directions = new List<Vector3>(nb_directions);

            direction.y = 0;
            direction.Normalize();

            for (int i = 0; i < nb_directions; i++)
                directions.Add(GetRandomDirectionInAngle(direction, angle));

            return directions;
        }

        public static List<Vector3> GetEvenlyDistributedDirectionsInAngle(Vector3 direction, float angle, int nb_directions)
        {
            List<Vector3> directions = new List<Vector3>(nb_directions);

            if (nb_directions <= 1)
            {
                directions.Add(direction);
                return directions;
            }

            float startAngle = -angle;
            float endAngle = angle;
            float step = (endAngle - startAngle) / (nb_directions - 1);

            for (int i = 0; i < nb_directions; i++)
            {
                float currentAngle = startAngle + step * i;
                Vector3 rotated = Quaternion.AngleAxis(currentAngle, Vector3.up) * direction;
                directions.Add(rotated.normalized);
            }

            return directions;
        }
    }

    public class ProjectileManager : BaseBehaviour
    {
        private AllocationPool<Projectile> m_projectilePool;
        private List<Projectile> m_currentProjectiles;
        [SerializeField] private Transform m_projectileContainer;

        public Player m_player;

        // prefab of your projectiles
        [SerializeField] private GameObject m_testProjPrefab;

        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            if (m_projectilePool != null)
                m_projectilePool.Update(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (m_player.LastDirection == Vector3.zero)
                    return;

                var directions = ProjectileUtils.GetEvenlyDistributedDirectionsInAngle(m_player.LastDirection, 360, 8);

                foreach (var direction in directions)
                {
                    m_projectilePool.AllocateElement(m_player.transform.position + new Vector3(0, 2, 0), direction, 50f);
                }
            }
        }
        public override void LateInit(params object[] parameters)
        {
        }
        public override void Init(params object[] parameters)
        {
            m_projectilePool = new AllocationPool<Projectile>(m_testProjPrefab, m_projectileContainer, 10, SortOrderMethod.Sort_None, null, OnInitProjectile);
        }
        private void OnInitProjectile(Projectile projectile)
        {
            projectile.Init();
        }
    }
}