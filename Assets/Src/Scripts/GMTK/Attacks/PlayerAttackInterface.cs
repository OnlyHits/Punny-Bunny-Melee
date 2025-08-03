using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class PlayerAttackInterface : AttackInterface
    {
        private Player m_player = null;
        private string m_projectileLayerName = GmtkUtils.PlayerUserProjectile_Layer;
        [SerializeField] private LayerMask m_layer;

        [SerializeField] private float m_outerRepulsionRadius;
        [SerializeField] private float m_innerRepulsionRadius;
        [SerializeField, ReadOnly] private Collider[] m_hits = new Collider[100];

        public override void Init(params object[] parameters)
        {
            base.Init(parameters);

            if (parameters.Length < 2 || parameters[1] is not Player)
            {
                Debug.LogWarning("wrong parameters");
                return;
            }

            m_player = (Player)parameters[1];

            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire += OnFire;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onSwitchWeapon += OnSwitchWeapon;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onCounter += OnCounter;

            m_layer = LayerMask.NameToLayer(m_projectileLayerName);
        }

        private void OnFire(InputType input, bool b)
        {
            if (m_player.IsRagdoll)
                return;

            if (input == InputType.RELEASED)
            {
                TryAttack(LayerMask.NameToLayer(m_projectileLayerName));
            }
        }

        private void OnSwitchWeapon(InputType input, float b)
        {
            if (m_player.IsRagdoll)
                return;

            if ((int)b > 0)
            {
                ChangeAttack(GetIndex() == 3 ? 0 : GetIndex() + 1);
            }
            else if ((int)b < 0)
            {
                ChangeAttack(GetIndex() == 0 ? 3 : GetIndex() - 1);
            }
        }

        private void OnCounter(InputType input, bool b)
        {
            if (m_player.IsRagdoll)
                return;

            if (input == InputType.PRESSED)
            {
                // consider using OverlapSphereNonAlloc
                m_hits = Physics.OverlapSphere(transform.position, m_outerRepulsionRadius);
                foreach (var hit in m_hits)
                {
                    Vector3 toProjectile = hit.transform.position - transform.position;
                    float distance = toProjectile.magnitude;

                    if (distance < m_innerRepulsionRadius || distance > m_outerRepulsionRadius)
                        continue;

                    if (hit.gameObject.layer == m_layer
                      && hit.TryGetComponent<Projectile>(out var projectile)
                      && projectile.Type == AttackUtils.BulletType.Bullet_Fireball)
                    {

                        projectile.Compute = false;

                        // Compute reflection direction
                        Vector3 incomingDir = projectile.Direction.normalized;
                        Vector3 normal = (hit.transform.position - transform.position).normalized; // crude surface normal
                        Vector3 reflectedDir = Vector3.Reflect(incomingDir, normal);

                        TryAttack(hit.transform.position, reflectedDir, 4, m_layer);
                    }
                }
            }
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_outerRepulsionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_innerRepulsionRadius);
        }
    }
}