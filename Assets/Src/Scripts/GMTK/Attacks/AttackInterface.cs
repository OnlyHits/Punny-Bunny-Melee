using CustomArchitecture;
using System.Collections;
using static GMTK.AttackUtils;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing;
using System;
using System.Data.Common;
using Unity.VisualScripting;

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
        public BulletType bullet_type = BulletType.Bullet_Normal;
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
        protected virtual IEnumerator FireCoroutine(Transform tr, Vector3 direction, AttackDatas datas)
        {
            List<Vector3> directions = null;

            if (datas.randomize_distribution)
                directions = ProjectileUtils.GetRandomDirectionsInAngle(direction, datas.angle_range, datas.projectile_number);
            else
                directions = ProjectileUtils.GetEvenlyDistributedDirectionsInAngle(direction, datas.angle_range, datas.projectile_number);

            foreach (var dir in directions)
            {
                m_projectileManager.AllocateProjectile(datas.bullet_type, dir, tr.position, datas.speed, datas.bounce_on_collision);
                if (directions.Count > 1 && !datas.fire_once)
                    yield return new WaitForSeconds(datas.time_between_bullet);
            }

            yield return null;
        }

        public virtual bool TryAttack(Transform tr, Vector3 direction, int index)
        {
            if (index >= m_attackDatas.Count || direction == Vector3.zero)
                return false;

            StartCoroutine(FireCoroutine(tr, direction, m_attackDatas[index]));

            return true;
        }
    }
}