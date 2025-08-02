using CustomArchitecture;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

using static GMTK.AttackUtils;

namespace GMTK
{
    [Serializable]
    public class AttackDatas
    {
        public int projectile_number = 1;
        public int speed = 30;
        public float angle_range = 0.0f;
        public bool randomize_distribution = false;
        public bool fire_once = true;
        public bool bounce_on_collision = false;
        public float time_between_bullet = 0.1f;
        public float max_distance_sqr = 50.0f;
        public BulletType bullet_type = BulletType.Bullet_Normal;

        public AttackDatas Clone()
        {
            return new AttackDatas
            {
                projectile_number = this.projectile_number,
                speed = this.speed,
                angle_range = this.angle_range,
                randomize_distribution = this.randomize_distribution,
                fire_once = this.fire_once,
                bounce_on_collision = this.bounce_on_collision,
                time_between_bullet = this.time_between_bullet,
                max_distance_sqr = this.max_distance_sqr,
                bullet_type = this.bullet_type
            };
        }
    }

    public class AttackInterface : BaseBehaviour
    {
        [SerializeField] private List<AttackDatas> m_attackDatas = new();
        [SerializeField] private float m_fireRate; // 1 / fire rate as scd

        private ProjectileManager m_projectileManager;

        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not ProjectileManager)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            m_projectileManager = (ProjectileManager)parameters[0];
        }

        protected virtual IEnumerator FireCoroutine(Transform tr, Vector3 direction, AttackDatas datas, int projectile_layer)
        {
            List<Vector3> directions = null;

            if (datas.randomize_distribution)
                directions = ProjectileUtils.GetRandomDirectionsInAngle(direction, datas.angle_range, datas.projectile_number);
            else
                directions = ProjectileUtils.GetEvenlyDistributedDirectionsInAngle(direction, datas.angle_range, datas.projectile_number);

            foreach (var dir in directions)
            {
                if (UnityEngine.Random.Range(0, 100) <= 1)
                {
                    AttackDatas d = datas.Clone();
                    d.bullet_type = BulletType.Bullet_Fireball;

                    m_projectileManager.AllocateProjectile(dir, tr.position, d, projectile_layer);
                }
                else
                    m_projectileManager.AllocateProjectile(dir, tr.position, datas, projectile_layer);
                if (directions.Count > 1 && !datas.fire_once)
                    yield return new WaitForSeconds(datas.time_between_bullet);
            }

            yield return null;
        }

        public virtual bool TryAttack(Transform tr, Vector3 direction, int index, int projectile_layer)
        {
            if (index >= m_attackDatas.Count || direction == Vector3.zero)
                return false;

            StartCoroutine(FireCoroutine(tr, direction, m_attackDatas[index], projectile_layer));

            return true;
        }
    }
}