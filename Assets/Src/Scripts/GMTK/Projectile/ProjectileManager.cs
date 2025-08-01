using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;

namespace GMTK
{
    public static class ProjectileUtils
    {
        public static Vector3 GetRandomDirectionInAngle(Vector3 direction, float angle)
        {
            float randomAngle = Random.Range(-angle * .5f, angle * .5f);
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

            float startAngle = -angle * .5f;
            float endAngle = angle * .5f;
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
        // Pools of bullets
        private AllocationPool<Projectile> m_normalBulletPool = null;
        private AllocationPool<Projectile> m_bouncyBulletPool = null;

        private List<Projectile> m_currentProjectiles;

        [SerializeField] private Transform m_projectileContainer;

        public Player m_player;

        public void AllocateProjectile(AttackUtils.BulletType type, Vector3 direction, Vector3 position, float speed, bool bounce, int projectile_layer)
        {
            switch (type)
            {
                case AttackUtils.BulletType.Bullet_Normal:
                    m_normalBulletPool?.AllocateElement(position, direction, speed, bounce, projectile_layer);
                    break;
                case AttackUtils.BulletType.Bullet_Bouncy:
                    m_bouncyBulletPool?.AllocateElement(position, direction, speed, bounce, projectile_layer);
                    break;
            }
        }

        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            m_normalBulletPool?.Update(Time.deltaTime);
            m_bouncyBulletPool?.Update(Time.deltaTime);
        }
        public override void LateInit(params object[] parameters)
        {
        }
        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not AttackLoader)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            AttackLoader attacks = (AttackLoader)parameters[0];

            if (attacks.GetPrefabs().TryGetValue(AttackUtils.BulletType.Bullet_Normal, out var normal))
            {
                m_normalBulletPool = new AllocationPool<Projectile>(normal, m_projectileContainer, 100, SortOrderMethod.Sort_None, null, OnInitProjectile);
            }

            if (attacks.GetPrefabs().TryGetValue(AttackUtils.BulletType.Bullet_Bouncy, out var bouncy))
            {
                m_bouncyBulletPool = new AllocationPool<Projectile>(bouncy, m_projectileContainer, 100, SortOrderMethod.Sort_None, null, OnInitProjectile);
            }
        }
        private void OnInitProjectile(Projectile projectile)
        {
            projectile.Init();
        }
    }
}