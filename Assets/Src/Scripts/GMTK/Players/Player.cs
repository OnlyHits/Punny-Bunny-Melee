using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace GMTK
{
    public class Player : BaseBehaviour
    {
        [Title("Dependancies")]
        [SerializeField] private Rigidbody m_rb;
        [SerializeField] private Collider m_collider;
        [SerializeField] private Animator m_animator;
        [SerializeField] private NavMeshAgent m_agent;

        [Title("Pistol pivot")]
        [SerializeField] private Transform m_pistolPivot; // waiting for weapon

        [Title("Visual")]
        [SerializeField] private List<Renderer> m_renderers;
        [SerializeField] private Material m_material;

        [Title("Ragdoll")]
        [SerializeField] private List<Rigidbody> m_ragdollRb;

        [Title("Data")]
        [SerializeField] private float m_speed = 1f;
        [SerializeField, ReadOnly] private Vector3 m_direction;
        private Vector3 m_lastDirection = Vector3.zero;
        private Vector3 m_basePos;

        private PlayerAttackInterface m_attackInterface;

        public Vector3 LastDirection { get => m_lastDirection; protected set { } }
        public Transform PistolPivot { get => m_pistolPivot; protected set { } }

        private readonly string ANIM_RUN = "Run";

        #region Unity_Cb
        private void Awake()
        {
            m_basePos = transform.position;
            EnableRagdoll(false);
            BindMaterial();
        }
        #endregion

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
            if (ComponentUtils.GetOrCreateComponent<PlayerAttackInterface>(gameObject, out m_attackInterface))
            {
                m_attackInterface.Init(parameters[0], this);
            }
        }

        public override void LateInit(params object[] parameters)
        {
        }

        protected override void OnFixedUpdate()
        {
        }

        protected override void OnLateUpdate()
        {
            transform.position = m_agent.transform.position;
            m_agent.transform.localPosition = Vector3.zero;
        }

        protected override void OnUpdate()
        {

        }
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

        #region Movement
        public void StartMove()
        {
            m_animator.SetBool(ANIM_RUN, true);
        }
        public void StopMove()
        {
            m_animator.SetBool(ANIM_RUN, false);

            m_rb.linearVelocity = Vector3.zero;
            m_rb.angularVelocity = Vector3.zero;
            m_direction = Vector3.zero;
        }
        public void NoMove()
        {
            m_rb.linearVelocity = Vector3.zero;
            m_rb.angularVelocity = Vector3.zero;
            m_direction = Vector3.zero;
        }
        public void Move()
        {
            m_direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            m_direction = m_direction.normalized;
            m_lastDirection = m_direction == Vector3.zero ? m_lastDirection : m_direction;

            if (m_direction == Vector3.zero) return;

            Vector3 destVel = Vector3.zero;
            Vector3 expVel = Vector3.zero;

            transform.forward = m_direction;
            destVel = transform.forward * m_speed;
            expVel = destVel - m_rb.linearVelocity;

            m_rb.AddForce(expVel, ForceMode.VelocityChange);
            m_rb.position = new Vector3(m_rb.position.x, m_basePos.y, m_rb.position.z);
        }
        #endregion

        #region Ragdoll
        public void EnableRagdoll(bool enable)
        {
            m_rb.useGravity = enable;
            m_collider.enabled = !enable;
            m_animator.enabled = !enable;
            if (!enable)
            {
                m_animator.Rebind();
            }
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
        public void OnGetHit()
        {
            EnableRagdoll(true);
            StartCoroutine(CoroutineUtils.InvokeOnDelay(2f, DisableRagdoll));
        }
        #endregion

    }
}