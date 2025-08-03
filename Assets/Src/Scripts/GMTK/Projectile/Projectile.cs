using CustomArchitecture;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI.Extensions.CasualGame;

namespace GMTK
{
    public class Projectile : APoolElement
    {
        private Vector3 m_direction = Vector3.zero;
        private float m_speed = 0.0f;
        private float m_maxDistanceSqr = 60.0f;
        private int m_maxBounces = 1;
        private bool m_bounceOnCollide = false;

        private Rigidbody m_rigidbody;
        private ParticleSystem m_particleSystem;
        private Vector3 m_lastVelocity = Vector3.zero;
        private Vector3 m_lastPosition = Vector3.zero;
        private float m_distanceSinceAllocate = 0;
        private int m_bounces = 0;

        private AttackUtils.BulletType m_type;

        public Vector3 Direction { get => m_direction; set => m_direction = value; }
        public float Speed { get => m_speed; set => m_speed = value; }
        public AttackUtils.BulletType Type { get => m_type; set => m_type = value; }
        public float MaxDistanceSqr { get => m_maxDistanceSqr; set => m_maxDistanceSqr = value; }
        public int MaxBounces { get => m_maxBounces; set => m_maxBounces = value; }

        public override void Init(params object[] parameter)
        {
            m_particleSystem = GetComponent<ParticleSystem>();

            if (TryGetComponent<ArenaTransposer>(out var arenaTransposer))
            {
                arenaTransposer.RegisterOnTeleport(OnTeleport);
            }

            m_rigidbody = GetComponent<Rigidbody>();
        }

        private void OnTeleport(Vector3 position)
        {
            m_lastPosition = position;

            m_particleSystem.Stop();
            m_particleSystem.Play(true);
        }

        protected void Impulse(float speed)
        {
            m_rigidbody.AddForce(speed * m_direction, ForceMode.Impulse);

            m_lastVelocity = m_direction * speed;
        }

        protected override void OnFixedUpdate()
        {
            Vector3 currentPosition = transform.position;
            float movedDistance = (currentPosition - m_lastPosition).sqrMagnitude;

            m_distanceSinceAllocate += movedDistance;
            m_lastPosition = currentPosition;

            if (m_distanceSinceAllocate > m_maxDistanceSqr)
            {
                Compute = false;
            }
        }

        protected override void OnUpdate()
        {
            //if (m_rigidbody.linearVelocity.sqrMagnitude < m_minBulletMagnitudeSqr)
            //    Compute = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!m_bounceOnCollide)
            {
                Compute = false;
                return;
            }

            m_bounces += 1;

            if (m_bounces >= m_maxBounces)
            {
                Compute = false;
                m_bounces = 0;
                return;
            }

            ContactPoint contact = collision.contacts[0];
            m_direction = Vector3.Reflect(m_direction.normalized, contact.normal);

            m_particleSystem.Stop();
            m_particleSystem.Play(true);

            m_rigidbody.linearVelocity = Vector3.zero;

            Impulse(m_lastVelocity.magnitude);
        }

        public override void OnAllocate(params object[] parameter)
        {
            if (parameter.Length < 1 || parameter[0] is not Vector3)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }
            if (parameter.Length < 2 || parameter[1] is not Vector3)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }
            if (parameter.Length < 3 || parameter[2] is not AttackDatas)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            transform.position = (Vector3)parameter[0];
            m_direction = (Vector3)parameter[1];
            AttackDatas datas = (AttackDatas)parameter[2];

            m_speed = datas.speed;
            m_bounceOnCollide = datas.bounce_on_collision;
            m_maxDistanceSqr = datas.max_distance_sqr;
            m_maxBounces = datas.max_bounces;
            m_type = datas.bullet_type;

            m_distanceSinceAllocate = 0.0f;

            m_rigidbody.linearVelocity = Vector2.zero;

            m_lastPosition = transform.position;

            m_particleSystem.Stop();
            m_particleSystem.Play(true);

            Impulse(m_speed);

            Compute = true;
        }
        public override void OnDeallocate()
        {
            m_rigidbody.linearVelocity = Vector2.zero;
            m_distanceSinceAllocate = 0.0f;
            m_lastVelocity = Vector3.zero;
            m_lastPosition = Vector3.zero;
            Compute = false;
        }
    }
}