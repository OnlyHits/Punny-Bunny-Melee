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
        private AllocationPool<Explosion> m_nukeExplosionPool = null;
        private AllocationPool<Bonus> m_bulletBonusPool = null;
        private AllocationPool<Bonus> m_healBonusPool = null;
        private AllocationPool<Bonus> m_shieldBonusPool = null;

        [SerializeField] private GameObject m_nukePrefab = null;
        [SerializeField] private GameObject m_bulletBonusPrefab = null;
        [SerializeField] private GameObject m_healBonusPrefab = null;
        [SerializeField] private GameObject m_shieldBonusPrefab = null;

        private List<Projectile> m_currentProjectiles;

        [SerializeField] private Transform m_projectileContainer;

        public Player m_player;

        public void AllocateExplosion(Vector3 position)
        {
            m_nukeExplosionPool?.AllocateElement(position);
        }

        public void AllocateBonus(Vector3 position, BonusType type)
        {
            switch (type)
            {
                case BonusType.Bullet:
                    m_bulletBonusPool?.AllocateElement(position);
                    break;
                case BonusType.Heal:
                    m_healBonusPool?.AllocateElement(position);
                    break;
                case BonusType.Shield:
                    m_shieldBonusPool?.AllocateElement(position);
                    break;
            }
        }

        public void AllocateProjectile(Vector3 direction, Vector3 position, AttackDatas datas, Collider collider)
        {
            Projectile projectile = null;

            switch (datas.bullet_type)
            {
                case AttackUtils.BulletType.Bullet_Normal:
                    projectile =  m_normalBulletPool?.AllocateElement(position, direction, datas);
                    break;
                case AttackUtils.BulletType.Bullet_Bubble:
                    projectile = m_bubbleBulletPool?.AllocateElement(position, direction, datas);
                    break;
                case AttackUtils.BulletType.Bullet_Fireball:
                    projectile = m_fireballBulletPool?.AllocateElement(position, direction, datas);
                    break;
                case AttackUtils.BulletType.Bullet_Fireball_Big:
                    projectile = m_fireballBigBulletPool?.AllocateElement(position, direction, datas);
                    break;
            }

            if (projectile != null)
            {
                Collider projectileCollider = projectile.GetComponent<Collider>();

                if (collider != null && projectileCollider != null)
                {
                    Physics.IgnoreCollision(collider, projectileCollider);
                }
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
            m_nukeExplosionPool?.Update(Time.deltaTime);
            m_bulletBonusPool?.Update(Time.deltaTime);
            m_healBonusPool?.Update(Time.deltaTime);
            m_shieldBonusPool?.Update(Time.deltaTime);
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
            m_nukeExplosionPool = new AllocationPool<Explosion>(m_nukePrefab, m_projectileContainer, 4, SortOrderMethod.Sort_None, null, null);
            m_bulletBonusPool = new AllocationPool<Bonus>(m_bulletBonusPrefab, m_projectileContainer, 5, SortOrderMethod.Sort_None, null, null);
            m_healBonusPool = new AllocationPool<Bonus>(m_healBonusPrefab, m_projectileContainer, 5, SortOrderMethod.Sort_None, null, null);
            m_shieldBonusPool = new AllocationPool<Bonus>(m_shieldBonusPrefab, m_projectileContainer, 5, SortOrderMethod.Sort_None, null, null);
        }
        private void OnInitProjectile(Projectile projectile)
        {
            projectile.Init();
        }
    }
}