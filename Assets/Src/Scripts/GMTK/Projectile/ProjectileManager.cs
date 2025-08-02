using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;

namespace GMTK
{
    public static class ProjectileUtils
    {
        public static List<float> GetRandomStepsInAngle(float angle, int nb_steps)
        {
            List<float> steps = new List<float>(nb_steps);

            for (int i = 0; i < nb_steps; i++)
                steps.Add(Random.Range(-angle * .5f, angle * .5f));

            return steps;
        }

        public static List<float> GetEvenlyDistributedStepsInAngle(float angle, int nb_steps)
        {
            List<float> steps = new List<float>(nb_steps);

            if (nb_steps <= 1)
            {
                steps.Add(0.0f);
                return steps;
            }

            float startAngle = -angle * .5f;
            float endAngle = angle * .5f;
            float step = (endAngle - startAngle) / (nb_steps - 1);

            for (int i = 0; i < nb_steps; i++)
            {
                steps.Add(startAngle + step * i);
                //Vector3 rotated = Quaternion.AngleAxis(currentAngle, Vector3.up) * direction;
                //directions.Add(rotated.normalized);
            }

            return steps;
        }
    }

    public class ProjectileManager : BaseBehaviour
    {
        // Pools of bullets
        private AllocationPool<Projectile> m_normalBulletPool = null;
        private AllocationPool<Projectile> m_bubbleBulletPool = null;
        private AllocationPool<Projectile> m_fireballBulletPool = null;
        private AllocationPool<Projectile> m_fireballBigBulletPool = null;

        private List<Projectile> m_currentProjectiles;

        [SerializeField] private Transform m_projectileContainer;

        public Player m_player;

        public void AllocateProjectile(Vector3 direction, Vector3 position, AttackDatas datas, int projectile_layer)
        {
            switch (datas.bullet_type)
            {
                case AttackUtils.BulletType.Bullet_Normal:
                    m_normalBulletPool?.AllocateElement(position, direction, datas, projectile_layer);
                    break;
                case AttackUtils.BulletType.Bullet_Bubble:
                    m_bubbleBulletPool?.AllocateElement(position, direction, datas, projectile_layer);
                    break;
                case AttackUtils.BulletType.Bullet_Fireball:
                    m_fireballBulletPool?.AllocateElement(position, direction, datas, projectile_layer);
                    break;
                case AttackUtils.BulletType.Bullet_Fireball_Big:
                    m_fireballBigBulletPool?.AllocateElement(position, direction, datas, projectile_layer);
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
            m_bubbleBulletPool?.Update(Time.deltaTime);
            m_fireballBulletPool?.Update(Time.deltaTime);
            m_fireballBigBulletPool?.Update(Time.deltaTime);
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
            if (attacks.GetPrefabs().TryGetValue(AttackUtils.BulletType.Bullet_Bubble, out var bubble))
            {
                m_bubbleBulletPool = new AllocationPool<Projectile>(bubble, m_projectileContainer, 100, SortOrderMethod.Sort_None, null, OnInitProjectile);
            }
            if (attacks.GetPrefabs().TryGetValue(AttackUtils.BulletType.Bullet_Fireball, out var fireball))
            {
                m_fireballBulletPool = new AllocationPool<Projectile>(fireball, m_projectileContainer, 100, SortOrderMethod.Sort_None, null, OnInitProjectile);
            }
            if (attacks.GetPrefabs().TryGetValue(AttackUtils.BulletType.Bullet_Fireball_Big, out var fireballBig))
            {
                m_fireballBigBulletPool = new AllocationPool<Projectile>(fireballBig, m_projectileContainer, 100, SortOrderMethod.Sort_None, null, OnInitProjectile);
            }
        }
        private void OnInitProjectile(Projectile projectile)
        {
            projectile.Init();
        }
    }
}