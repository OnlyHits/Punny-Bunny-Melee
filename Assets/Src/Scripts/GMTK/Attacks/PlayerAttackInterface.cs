using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace GMTK
{
    public class PlayerAttackInterface : AttackInterface
    {
        private Player m_player = null;
        private string m_projectileLayerName = GmtkUtils.PlayerUserProjectile_Layer;
        [SerializeField] private LayerMask m_layer;

        [SerializeField] private SphereCollider m_collider;
        [SerializeField] private float m_outerRepulsionRadius;
        [SerializeField] private float m_innerRepulsionRadius;
        [SerializeField, ReadOnly] private int m_attackIndex = 0;
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

            //GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire1 += OnFire1;
            //GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire2 += OnFire2;
            //GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire3 += OnFire3;
            //GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire4 += OnFire4;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onFire += OnFire;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onSwitchWeapon += onSwitchWeapon;
            GMTKGameCore.Instance.MainGameMode.GetPlayerInput().onCounter += OnCounter;

            m_layer = LayerMask.NameToLayer(m_projectileLayerName);
        }

        private void onSwitchWeapon(InputType input, float f)
        {
            if (f == 0) return;

            m_attackIndex = (f >= 1) ? m_attackIndex - 1 : m_attackIndex;
            m_attackIndex = (f <= -1) ? m_attackIndex + 1 : m_attackIndex;

            if (m_attackIndex >= m_attackDatas.Count)
            {
                m_attackIndex = 0;
            }
            if (m_attackIndex < 0)
            {
                m_attackIndex = m_attackDatas.Count - 1;
            }

            Debug.Log("ATTACK INDEX = " + m_attackIndex.ToString());
        }

        private void OnFire(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.ShootDirection, m_attackIndex, LayerMask.NameToLayer(m_projectileLayerName));
            }
        }

        /*
        private void OnFire1(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.ShootDirection, 0, LayerMask.NameToLayer(m_projectileLayerName));
            }
        }

        private void OnFire2(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.ShootDirection, 1, LayerMask.NameToLayer(m_projectileLayerName));
            }
        }

        private void OnFire3(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.ShootDirection, 2, LayerMask.NameToLayer(m_projectileLayerName));
            }
        }

        private void OnFire4(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                TryAttack(m_player.PistolPivot, m_player.ShootDirection, 3, LayerMask.NameToLayer(m_projectileLayerName));
            }
        }
        */

        private void OnCounter(InputType input, bool b)
        {
            if (input == InputType.PRESSED)
            {
                // consider using OverlapSphereNonAlloc
                m_hits = Physics.OverlapSphere(transform.position, m_outerRepulsionRadius);

                Debug.Log(m_hits.Length);

                foreach (var hit in m_hits)
                {
                    Debug.Log("Presses space");

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

                        TryAttack(hit.transform, reflectedDir, 4, m_layer);
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