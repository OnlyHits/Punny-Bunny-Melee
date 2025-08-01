using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public class Projectile : APoolElement
    {
        private Vector3         m_direction = Vector3.zero;
        private float           m_speed = 0.0f;
        private float           m_maxDistanceSqr = 300.0f;
        private float           m_currentDistance = 0.0f;
        private ParticleSystem  m_particleSystem;
        private bool            m_bounceOnCollide = false;

        public Vector3 Direction { get => m_direction; set => m_direction = value; }
        public float Speed { get => m_speed; set => m_speed = value; }
        public float MaxDistance { get => m_maxDistanceSqr; set => m_maxDistanceSqr = value; }

        public override void Init(params object[] parameter)
        {
            m_particleSystem = GetComponent<ParticleSystem>();

            if (TryGetComponent<ArenaTransposer>(out var arenaTransposer))
            {
                arenaTransposer.RegisterOnTeleport(OnTeleport);
            }
        }

        private void OnTeleport()
        {
            m_particleSystem.Stop();
            m_particleSystem.Play(true);
        }

        protected override void OnUpdate()
        {
            Vector3 offset = m_speed * Time.deltaTime * m_direction;

            transform.position += offset;
            m_currentDistance += offset.sqrMagnitude;

            if (m_currentDistance > m_maxDistanceSqr)
                Compute = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!m_bounceOnCollide)
                return;

            ContactPoint contact = collision.contacts[0];
            m_direction = Vector3.Reflect(m_direction.normalized, contact.normal);
            m_particleSystem.Stop();
            m_particleSystem.Play(true);
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
            if (parameter.Length < 3 || parameter[2] is not float)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }
            if (parameter.Length < 4 || parameter[3] is not bool)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            transform.position = (Vector3)parameter[0];
            m_direction = (Vector3)parameter[1];
            m_speed = (float)parameter[2];
            m_bounceOnCollide = (bool)parameter[3];
            m_currentDistance = 0.0f;

            m_particleSystem.Stop();
            m_particleSystem.Play(true);

            Compute = true;
        }
        public override void OnDeallocate()
        {
            m_currentDistance = 0.0f;
            Compute = false;
        }
    }
}