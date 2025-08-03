using CustomArchitecture;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using static GMTK.AttackUtils;
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
        public float max_distance_sqr = 50.0f;
        public BulletType bullet_type = BulletType.Bullet_Normal;
        public WeaponType weapon_type = WeaponType.Pistol;

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
                bullet_type = this.bullet_type,
                weapon_type = this.weapon_type
            };
        }
    }

    public class AttackInterface : BaseBehaviour
    {
        [SerializeField] protected List<AttackDatas>  m_attackDatas = new();
        [SerializeField] protected float              m_fireRate = 2.0f; // 1 / firerate
        [SerializeField] protected Transform          m_weaponParent;

        [SerializeField] protected List<WeaponType> m_weapons = new();
        protected Dictionary<WeaponType, Weapon> m_currentWeapons = new();

        protected Weapon                m_currentWeapon = null;
        protected AttackDatas           m_equipedAttack = null;
        protected List<Coroutine>       m_fireCoroutines = new();
        protected ProjectileManager     m_projectileManager;
        protected int                   m_attackIndex = 0;
        protected float                 m_cooldownTimer = 0.0f;

        public int GetIndex() => m_attackIndex;
        public List<AttackDatas> AttackDatas() => m_attackDatas;
        public Weapon CurrentWeapon() => m_currentWeapon;
        public AttackDatas EquipedAttack() => m_equipedAttack;
        public bool IsFiring() => m_fireCoroutines.Count > 0;

        private Action<WeaponType> m_onChangeWeapon;
        private Action<WeaponType> m_onShoot;

        public WeaponType GetWeaponType() => m_currentWeapon == null ? default : m_currentWeapon.GetWeaponType();


        public IEnumerator Load()
        {
            int total = m_weapons.Count;
            int completed = 0;

            foreach (var kvp in m_weapons)
            {
                WeaponType bulletType = kvp;
                string address = s_weaponPath[bulletType];

                yield return AddressableFactory.CreateAsync(address, m_weaponParent, (go) =>
                {
                    if (go != null)
                    {
                        m_currentWeapons[bulletType] = go.GetComponent<Weapon>();
                        go.SetActive(false);
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

        public void RegisterOnSwitchWeapon(Action<WeaponType> function)
        {
            m_onChangeWeapon -= function;
            m_onChangeWeapon += function;
        }

        public void RegisterOnShoot(Action<WeaponType> function)
        {
            m_onShoot -= function;
            m_onShoot += function;
        }

        #region BaseBehaviour

        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            m_cooldownTimer = Mathf.Max(m_cooldownTimer - Time.deltaTime, 0.0f);
        }
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

            foreach (var weapon in m_currentWeapons)
            {
                weapon.Value.Init();
            }

            m_cooldownTimer = 0f;
            ChangeAttack(0);
        }
        #endregion BaseBehaviour

        #region Attack management
        private bool ChangeWeapon(AttackDatas datas)
        {
            Weapon old_weapon = m_currentWeapon;
            if (m_currentWeapons.TryGetValue(datas.weapon_type, out m_currentWeapon))
            {
                if (old_weapon != null)
                {
                    old_weapon.gameObject.SetActive(false);
                }
                m_currentWeapon.gameObject.SetActive(true);
                m_onChangeWeapon?.Invoke(m_currentWeapon.GetWeaponType());
                return true;
            }

            return false;
        }

        public void ChangeAttack(int index)
        {
            if (m_attackDatas.Count == 0 || index >= m_attackDatas.Count)
                return;

            m_equipedAttack = m_attackDatas[index];
            m_attackIndex = index;

            if (!ChangeWeapon(m_equipedAttack))
            {
                Debug.LogWarning("You try to equip an unexistant weapon");
                return;
            }

            StopAllAttacks();
        }

        #endregion Attack management

        #region Attack_Couroutines
        public void StopAllAttacks()
        {
            foreach (var coroutine in m_fireCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            m_fireCoroutines.Clear();
        }

        protected virtual IEnumerator FireCoroutine(Vector3 position, Vector3 direction, AttackDatas datas, int projectile_layer)
        {
            List<float> steps = null;

            if (datas.randomize_distribution)
                steps = ProjectileUtils.GetRandomStepsInAngle(datas.angle_range, datas.projectile_number);
            else
                steps = ProjectileUtils.GetEvenlyDistributedStepsInAngle(datas.angle_range, datas.projectile_number);

            foreach (var angle in steps)
            {
                Vector3 rotated = Quaternion.AngleAxis(angle, Vector3.up) * direction;

                if (UnityEngine.Random.Range(0, 100) <= 1)
                {
                    AttackDatas d = datas.Clone();
                    d.bullet_type = BulletType.Bullet_Fireball;

                    m_projectileManager.AllocateProjectile(rotated, position, d, projectile_layer);
                }
                else
                    m_projectileManager.AllocateProjectile(rotated, position, datas, projectile_layer);

                if (steps.Count > 1 && !datas.fire_once)
                    yield return new WaitForSeconds(datas.time_between_bullet);
            }

            m_fireCoroutines.RemoveAll(c => c == null);
        }

        protected virtual IEnumerator FireCoroutine(AttackDatas datas, int projectile_layer)
        {
            List<float> steps = null;

            if (datas.randomize_distribution)
                steps = ProjectileUtils.GetRandomStepsInAngle(datas.angle_range, datas.projectile_number);
            else
                steps = ProjectileUtils.GetEvenlyDistributedStepsInAngle(datas.angle_range, datas.projectile_number);

            foreach (var angle in steps)
            {
                Vector3 rotated = Quaternion.AngleAxis(angle, Vector3.up) * m_currentWeapon.GetFireDirection();

                if (UnityEngine.Random.Range(0, 100) <= 1)
                {
                    AttackDatas d = datas.Clone();
                    d.bullet_type = BulletType.Bullet_Fireball;

                    m_projectileManager.AllocateProjectile(rotated, m_currentWeapon.GetFirePosition(), d, projectile_layer);
                }
                else
                    m_projectileManager.AllocateProjectile(rotated, m_currentWeapon.GetFirePosition(), datas, projectile_layer);

                if (steps.Count > 1 && !datas.fire_once)
                    yield return new WaitForSeconds(datas.time_between_bullet);
            }

            m_fireCoroutines.RemoveAll(c => c == null);
        }

        #endregion

        // use this if you want to use the current attack 
        public virtual bool TryAttack(int projectile_layer)
        {
            if (m_cooldownTimer != 0f)
                return false;

            if (m_currentWeapon == null
                || m_currentWeapon.GetFireDirection() == Vector3.zero
                || m_attackDatas.Count == 0)
                return false;

            Coroutine wrapper = StartCoroutine(
                FireCoroutine(m_equipedAttack, projectile_layer)
            );

            m_fireCoroutines.Add(wrapper);
            m_cooldownTimer = 1f / m_fireRate;
            m_onShoot?.Invoke(m_currentWeapon.GetWeaponType());
            return true;
        }

        // use this if you want to land a projectile from m_attackDatas[index]
        public virtual bool TryAttack(Vector3 position, Vector3 direction, int index, int projectile_layer)
        {
            if (m_cooldownTimer != 0f)
                return false;
            if (direction == Vector3.zero || index >= m_attackDatas.Count)
                return false;

            Coroutine wrapper = StartCoroutine(
                FireCoroutine(position, direction, m_attackDatas[index], projectile_layer)
            );

            m_fireCoroutines.Add(wrapper);
            m_cooldownTimer = 1f / m_fireRate;
            m_onShoot?.Invoke(m_currentWeapon.GetWeaponType());
            return true;
        }
    }
}