using CustomArchitecture;
using UnityEngine;

namespace GMTK
{
    public enum BonusType
    {
        Heal,
        Shield,
        Bullet
    }

    public class Bonus : APoolElement
    {
        [SerializeField] private BonusType m_type = BonusType.Heal;
        [SerializeField] private ParticleSystem m_particleSystem = null;

        public BonusType GetBonusType() => m_type;

        protected override void OnUpdate()
        {
            if (!Compute)
                return;

            if (!m_particleSystem.IsAlive(true))
            {
                this.m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

            m_particleSystem.Play();

            Compute = true;
        }

        public override void OnDeallocate()
        {
            m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            Compute = false;
        }
    }
}