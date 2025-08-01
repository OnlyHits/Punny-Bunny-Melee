using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GMTK
{
    public class Player : BaseBehaviour
    {
        [Title("Dependancies")]
        [SerializeField] private Rigidbody m_rb;
        [SerializeField] private Collider m_collider;
        [SerializeField] private Animator m_animator;

        [Title("Ragdoll")]
        [SerializeField] private List<Rigidbody> m_ragdollRb;

        [Title("Data")]
        [SerializeField] private float m_speed = 1f;
        [SerializeField, ReadOnly] private Vector3 m_direction;
        private Vector3 m_basePos;

        private readonly string ANIM_RUN = "Run";

        #region Unity_Cb
        private void Awake()
        {
            m_basePos = transform.position;
            EnableRagdoll(false);
        }
        #endregion

        #region BaseBehaviour_Cb
        public override void Init(params object[] parameters)
        {
        }

        public override void LateInit(params object[] parameters)
        {
        }

        protected override void OnFixedUpdate()
        {
        }

        protected override void OnLateUpdate()
        {

        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnGetHit();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                StartMove();
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                StopMove();
            }


            if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
            {
                Move();
            }
            else
            {
                NoMove();
            }
        }
        #endregion BaseBehaviour_Cb

        #region Movement
        private void StartMove()
        {
            m_animator.SetBool(ANIM_RUN, true);
        }
        private void StopMove()
        {
            m_animator.SetBool(ANIM_RUN, false);

            m_rb.linearVelocity = Vector3.zero;
            m_rb.angularVelocity = Vector3.zero;
            m_direction = Vector3.zero;
        }
        private void NoMove()
        {
            m_rb.linearVelocity = Vector3.zero;
            m_rb.angularVelocity = Vector3.zero;
            m_direction = Vector3.zero;
        }
        private void Move()
        {
            m_direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            m_direction = m_direction.normalized;

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

        public void OnGetHit()
        {
            EnableRagdoll(true);
            StartCoroutine(CoroutineUtils.InvokeOnDelay(2f, DisableRagdoll));
        }

    }
}