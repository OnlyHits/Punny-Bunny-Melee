using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public class Explosion : APoolElement
    {
        [SerializeField] private ParticleSystem m_explosionParticles;

        protected override void OnUpdate()
        {
            if (!Compute)
                return;

            if (!m_explosionParticles.IsAlive(true))
            {
                this.m_explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Compute = false;
            }
        }

        public override void OnAllocate(params object[] parameter)
        {
            if (parameter.Length < 1 || parameter[0] is not Vector3)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            transform.position = (Vector3)parameter[0];

            m_explosionParticles.Play();

            Compute = true;
        }
        public override void OnDeallocate()
        {
            m_explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            Compute = false;
        }
    }
}