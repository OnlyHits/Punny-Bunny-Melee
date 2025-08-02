using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
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
        [SerializeField] private ArenaTransposer m_transposer;
        [SerializeField] private RagdollArenaTransposer m_ragdollTransposer;

        [Title("Movement")]
        [SerializeField] protected float m_speed = 1f;
        [SerializeField, ReadOnly] protected Vector3 m_direction;

        [Title("Ragdoll")]
        [SerializeField] protected float m_explostionForce = 5f;
        [SerializeField] protected float m_ragdollTime = 2f;
        [SerializeField] protected List<Rigidbody> m_ragdollRb;

        [Title("Pistol pivot")]
        [SerializeField] protected Transform m_pistolPivot;

        [Title("Visual")]
        [SerializeField] protected List<Renderer> m_renderers;
        [SerializeField] protected Material m_material;

        protected bool m_isMoving;
        protected Vector3 m_shootDirection = Vector3.zero;
        protected Vector3 m_basePos;

        public bool IsMoving { get => m_isMoving; protected set { } }
        public Vector3 ShootDirection { get => m_shootDirection; protected set { } }
        public Transform PistolPivot { get => m_pistolPivot; protected set { } }

        private readonly string ANIM_RUN = "Run";

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
                OnGetHit(collision);
            }
            if (collision.gameObject.layer == LayerMask.NameToLayer(GmtkUtils.PlayerAIProjectile_Layer))
            {
                OnGetHit(collision);
            }
        }
        #endregion

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters) { }
        public override void LateInit(params object[] parameters) { }
        protected override void OnFixedUpdate() { }
        protected override void OnLateUpdate()
        {
            if (m_agent.gameObject.activeSelf)
            {
                transform.position = m_agent.transform.position;
                m_agent.transform.localPosition = Vector3.zero;
            }
            else
            {
                //transform.position = m_agent.transform.position;
                //m_agent.transform.position = transform.position;
            }
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

        #region Rotation
        public void Rotate(Vector3 mouseScreenPos)
        {
            //Camera mainCam = GMTKGameCore.Instance.MainGameMode.;
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
        #endregion

        #region Movement
        public void Move(Vector2 direction)
        {
            m_direction = new Vector3(direction.x, 0, direction.y);
            m_direction = m_direction.normalized;

            if (m_direction == Vector3.zero) return;

            Vector3 destVel = Vector3.zero;
            Vector3 expVel = Vector3.zero;

            //transform.forward = m_direction;
            destVel = m_direction * m_speed;
            expVel = destVel - m_rb.linearVelocity;

            m_rb.AddForce(expVel, ForceMode.VelocityChange);
            m_rb.position = new Vector3(m_rb.position.x, m_basePos.y, m_rb.position.z);
        }
        public void StartMove()
        {
            m_isMoving = true;
            m_animator.SetBool(ANIM_RUN, true);
        }
        public void StopMove()
        {
            m_isMoving = false;
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
        #endregion

        #region Ragdoll
        public void EnableRagdoll(bool enable)
        {
            m_ragdollTransposer.enabled = enable;
            m_transposer.enabled = !enable;

            // @note: reset the player pos on stop ragdoll
            if (!enable)
            {
                transform.position = m_ragdollRb[0].position;
                m_agent.transform.position = transform.position;
                m_ragdollRb[0].transform.localPosition = Vector3.zero;
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
        public void OnGetHit(Collision collision = null)
        {
            EnableRagdoll(true);

            if (collision != null)
            {
                Vector3 playerPos = new Vector3(transform.position.x, 0f, transform.position.z);
                Vector3 hitPos = new Vector3(collision.transform.position.x, 0f, collision.transform.position.z);

                Vector3 explulsionDir = playerPos - hitPos;

                m_ragdollRb[0].AddForce(explulsionDir * m_explostionForce, ForceMode.Impulse);
            }

            StartCoroutine(CoroutineUtils.InvokeOnDelay(m_ragdollTime, DisableRagdoll));
        }
        #endregion

    }
}