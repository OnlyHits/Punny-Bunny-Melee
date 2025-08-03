using System;
using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

using static GMTK.AttackUtils;

namespace GMTK
{
    public abstract class Player : BaseBehaviour
    {
        [Title("Dependancies")]
        [SerializeField] protected Rigidbody m_rb;
        [SerializeField] protected Collider m_collider;
        [SerializeField] protected Animator m_animator;
        [SerializeField] protected NavMeshAgent m_agent;
        [SerializeField] protected ArenaTransposer m_transposer;
        [SerializeField] protected RagdollArenaTransposer m_ragdollTransposer;

        [Title("Movement")]
        [SerializeField] protected float m_speed = 1f;
        [SerializeField, ReadOnly] protected Vector3 m_direction;

        [Title("Life")]
        [SerializeField] protected float m_prctDamages = 1f;

        [Title("Ragdoll")]
        [SerializeField] protected float m_dieRepulsionForce = 400f;
        [SerializeField] protected Vector2 m_repulseForceRange = Vector2.zero;
        [SerializeField] protected Vector2 m_ragdollTimeRange = Vector2.zero;
        [SerializeField] protected List<Rigidbody> m_ragdollRb;

        [Title("Low life modifier")]
        [SerializeField] protected float m_fireRate = 5;
        [SerializeField] protected ParticleSystem m_particleSystem;

        [Title("Pistol pivot")]
        [SerializeField] protected Transform m_pistolPivot;

        [Title("Visual")]
        [SerializeField] protected List<Renderer> m_renderers;
        [SerializeField] protected Material m_material;

        protected bool m_isMoving;
        protected Vector3 m_shootDirection = Vector3.zero;
        protected Vector3 m_basePos;
        protected bool m_isRagdoll = false;
        protected bool m_isDead = false;

        public bool IsDead() => m_isDead;
        public float GetPrcntDamages() => m_prctDamages;
        public Color GetMainColor() => m_material.GetColor("_BaseColor");
        public bool IsMoving { get => m_isMoving; protected set { } }
        public Vector3 ShootDirection { get => m_shootDirection; protected set { } }
        public Transform PistolPivot { get => m_pistolPivot; protected set { } }
        public bool IsRagdoll { get => m_isRagdoll; protected set { } }
        private event Action m_onGetHit;

        private readonly string ANIM_RUN = "Run";
        public abstract AttackInterface GetAttackManager();

        #region Unity_Cb
        private void Awake()
        {
            m_basePos = transform.position;
            EnableRagdoll(false);
            BindMaterial();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null) return;
            if (collision.gameObject == null) return;

            if (collision.gameObject.layer == LayerMask.NameToLayer(GmtkUtils.PlayerUserProjectile_Layer))
            {
                if (!IsRagdoll)
                    GetHit(collision);
            }
            if (collision.gameObject.layer == LayerMask.NameToLayer(GmtkUtils.PlayerAIProjectile_Layer))
            {
                if (!IsRagdoll)
                    GetHit(collision);
            }
        }
        #endregion

        private void OnCollisionWithWall(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(GmtkUtils.ObstacleLayer))
            {
                if (IsRagdoll && m_prctDamages == 100f)
                    Die(collision);
            }
        }

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
            m_transposer.RegisterOnTeleport(OnTeleport);

            m_ragdollRb[0].AddComponent<ColliderWrapper>();
            m_ragdollRb[0].GetComponent<ColliderWrapper>().LayerToCollide = LayerMask.GetMask(GmtkUtils.ObstacleLayer);
            m_ragdollRb[0].GetComponent<ColliderWrapper>().onCollisionEnter += OnCollisionWithWall;
        }

        protected virtual void OnTeleport(Vector3 position)
        {
            transform.position = position;

            if (m_agent.gameObject.activeSelf && m_agent.enabled && m_agent.isOnNavMesh)
            {
                m_agent.Warp(position);
            }
        }

        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate()
        {
            if (m_agent.gameObject.activeSelf)
            {
                transform.position = m_agent.transform.position;
                m_agent.transform.localPosition = Vector3.zero;
            }
        }

        protected override void OnUpdate() { }
        #endregion BaseBehaviour_Cb

        #region Visual
        private void BindMaterial()
        {
            foreach (Renderer rd in m_renderers)
            {
                rd.materials = new Material[1] { m_material };
            }
        }
        #endregion

        #region Rotation
        protected void Rotate(Vector3 mouseScreenPos)
        {
            Camera mainCam = Camera.main;
            float distanceToGround = mainCam.transform.position.y;
            Vector3 worldMousePosition = Vector3.zero;

            mouseScreenPos.z = distanceToGround;
            worldMousePosition = mainCam.ScreenToWorldPoint(mouseScreenPos);
            worldMousePosition.y = 0f;

            Vector3 playerPos = new Vector3(transform.position.x, 0f, transform.position.z);

            m_shootDirection = worldMousePosition - playerPos;
            m_shootDirection = m_shootDirection.normalized;

            if (m_shootDirection == Vector3.zero) return;

            transform.forward = m_shootDirection;
        }
        public void Rotate()
        {
            Vector3 velocity = m_rb.linearVelocity;
            velocity.y = 0f;

            if (velocity.sqrMagnitude < 0.001f)
                return;

            m_shootDirection = velocity.normalized;
            transform.forward = m_shootDirection;
        }

        #endregion

        #region Movement
        protected void Move(Vector2 direction)
        {
            m_direction = new Vector3(direction.x, 0, direction.y);
            m_direction = m_direction.normalized;

            if (m_direction == Vector3.zero) return;

            Vector3 destVel = Vector3.zero;
            Vector3 expVel = Vector3.zero;

            destVel = m_direction * m_speed;
            expVel = destVel - m_rb.linearVelocity;

            m_rb.AddForce(expVel, ForceMode.VelocityChange);
            m_rb.position = new Vector3(m_rb.position.x, m_basePos.y, m_rb.position.z);
        }
        protected void StartMove()
        {
            m_isMoving = true;
            m_animator.SetBool(ANIM_RUN, true);
        }
        protected void StopMove()
        {
            m_isMoving = false;
            m_animator.SetBool(ANIM_RUN, false);

            m_rb.linearVelocity = Vector3.zero;
            m_rb.angularVelocity = Vector3.zero;
            m_direction = Vector3.zero;
        }
        protected void NoMove()
        {
            m_rb.linearVelocity = Vector3.zero;
            m_rb.angularVelocity = Vector3.zero;
            m_direction = Vector3.zero;
        }
        #endregion

        #region Ragdoll
        protected void EnableRagdoll(bool enable)
        {
            m_isRagdoll = enable;

            m_ragdollTransposer.enabled = enable;
            m_transposer.enabled = !enable;

            // @note: reset the player pos on stop ragdoll
            if (!enable)
            {
                transform.position = m_ragdollRb[0].position;
                m_ragdollRb[0].transform.localPosition = Vector3.zero;
                m_agent.Warp(transform.position);
            }

            m_rb.useGravity = enable;
            m_collider.enabled = !enable;
            m_animator.enabled = !enable;
            if (!enable)
            {
                m_animator.Rebind();
            }
            m_agent.gameObject.SetActive(!enable);

            foreach (Rigidbody rb in m_ragdollRb)
            {
                rb.isKinematic = !enable;
                if (rb.TryGetComponent(out Collider col))
                {
                    col.enabled = enable;
                }
            }
        }

        private void EnableRagdoll() => EnableRagdoll(true);
        private void DisableRagdoll() => EnableRagdoll(false);
        #endregion

        #region Hit
        public void RegisterOnGetHit(Action function)
        {
            m_onGetHit -= function;
            m_onGetHit += function;
        }
        protected virtual void Die(Collision collision)
        {
            StopMove();

            EnableRagdoll(true);

            foreach (var item in m_ragdollRb)
            {
                item.linearVelocity = Vector3.zero;
                item.GetComponent<Collider>().enabled = false;
            }

            GMTKGameCore.Instance.MainGameMode.GetGameManager()
                .GetProjectileManager().AllocateExplosion(m_ragdollRb[0].transform.position);

            m_isDead = true;
            gameObject.SetActive(false);
        }

        protected virtual void GetHit(Collision collision)
        {
            StopMove();

            EnableRagdoll(true);

            m_prctDamages = Mathf.Min(m_prctDamages * 1.1f + 5f, 100f);

            float repulseForce = ScaleByPercent(m_prctDamages, m_repulseForceRange);
            float ragdollTime = ScaleByPercent(m_prctDamages, m_ragdollTimeRange);

            if (m_prctDamages > 80f)
                GetAttackManager().SetBerserk(true);

            // @note: throw ragdoll
            if (collision != null)
            {
                Vector3 playerPos = new Vector3(transform.position.x, 0f, transform.position.z);
                Vector3 hitPos = new Vector3(collision.transform.position.x, 0f, collision.transform.position.z);

                Vector3 explulsionDir = (playerPos - hitPos).normalized;
                explulsionDir.y = 0f;

                m_ragdollRb[0].AddForce(explulsionDir * repulseForce, ForceMode.Impulse);
            }

            // @note: call after update stats like health to update ui properly
            m_onGetHit?.Invoke();

            // @note: reset ragdoll in x seconds
            StartCoroutine(CoroutineUtils.InvokeOnDelay(ragdollTime, DisableRagdoll));
        }
        #endregion
        protected float ScaleByPercent(float percent, Vector2 range)
        {
            percent = Mathf.Clamp01(percent / 100f);
            return Mathf.Lerp(range.x, range.y, percent);
        }

    }
}