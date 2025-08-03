using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Unity.VisualScripting;
using CustomArchitecture;
using UnityEngine.UIElements;

namespace GMTK
{
    public class AIController : Player
    {
        [SerializeField] private float m_cornerReachThreshold;

        private List<Player> m_enemies = null;
        private Player m_targetEnemy = null;

        private Vector3[] m_currentPath = null;
        private int m_currentPathIndex = 0;



        private bool m_justTeleport = false;
        private int m_teleportIndex = -1;

        private AIAttackInterface m_attackInterface = null;
        public override AttackInterface GetAttackManager() => m_attackInterface;

        bool m_isInit = false;


        #region BaseBehaviour_Cb
        public IEnumerator Load()
        {
            if (ComponentUtils.GetOrCreateComponent<AIAttackInterface>(gameObject, out m_attackInterface))
            {
                yield return StartCoroutine(m_attackInterface.Load());
            }
        }

        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not List<Player>)
            {
                UnityEngine.Debug.LogWarning("wrong parameters");
                return;
            }

            base.Init();

            m_transposer.RegisterOnTeleport(OnTeleport);

            m_attackInterface.Init(parameters[1], this);

            m_enemies = (List<Player>)parameters[0];
            m_isInit = true;
        }

        public override void LateInit(params object[] parameters)
        {
        }

        protected override void OnFixedUpdate()
        {
            if (m_currentPath != null)
                FollowPath();

            //DrawPathDebug();
        }

        protected override void OnLateUpdate()
        {
            if (m_agent.gameObject.activeSelf)
            {
                transform.position = m_agent.transform.position;
                m_agent.transform.localPosition = Vector3.zero;
            }
            else
            {
                transform.position = m_agent.transform.position;
                m_agent.transform.position = transform.position;
            }
        }

        public static bool GetRandomPointInBounds(Vector3 center, Vector3 size, out Vector3 result, int maxAttempts = 1)
        {
            result = Vector3.zero;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 randomPoint = new Vector3(
                    Random.Range(center.x - size.x / 2f, center.x + size.x / 2f),
                    center.y,
                    Random.Range(center.z - size.z / 2f, center.z + size.z / 2f)
                );

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            return false;
        }
        public void MoveToRandomPoint()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null)
                return;

            var datas = GMTKGameCore.Instance.MainGameMode.GetArenaTransposerDatas();

            if (GetRandomPointInBounds(datas.Item1, datas.Item2, out var result))
            {
                if (ComputeShortestWrappedPath(transform.position, result))
                    StartMove();
            }
        }

        protected override void OnUpdate()
        {
            if (!m_isInit) return;

            if (!IsMoving)
            {
                NoMove();
                MoveToRandomPoint();
            }

            if (GetInRangeEnemy(out m_targetEnemy))
            {
                RotateToTargetEnemy();
                m_attackInterface.Fire();
            }

            if (!m_attackInterface.IsFiring())
                m_targetEnemy = null;

            if (m_targetEnemy != null)
                RotateToTargetEnemy();
            else
                Rotate();
        }
        #endregion BaseBehaviour_Cb

        private void RotateToTargetEnemy()
        {
            m_shootDirection = (m_targetEnemy.transform.position - m_attackInterface.CurrentWeapon().GetFirePosition()).normalized;
            transform.forward = m_shootDirection;
        }

        protected override void OnTeleport(Vector3 position)
        {
            base.OnTeleport(position);
            m_justTeleport = true;
        }

        private float ComputePathLength(NavMeshPath path)
        {
            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return length;
        }

        private void ClearPath()
        {
            m_currentPathIndex = 0;
            m_currentPath = null;
        }

        public bool ComputeShortestWrappedPath(Vector3 fromPos, Vector3 toPos)
        {
            if (!GMTKGameCore.Instance.MainGameMode.ArenaManager().ComputeShortestPath(fromPos, toPos, out var rawPath, out var tp_index))
            {
                StopMove();
                ClearPath();
                return false;
            }

            m_currentPath = rawPath;
            m_teleportIndex = tp_index;
            m_currentPathIndex = 1;
            return true;
        }

        private void FollowPath()
        {
            if (m_currentPath == null || m_currentPath.Length == 0)
                return;

            if (m_currentPathIndex >= m_currentPath.Length)
            {
                StopMove();
                ClearPath();
                return;
            }

            if (m_justTeleport)
            {
                m_justTeleport = false;
                ++m_currentPathIndex;
                return;
            }

            Vector3 currentTarget = m_currentPath[m_currentPathIndex];
            Vector3 toTarget = currentTarget - transform.position;
            toTarget.y = 0;

            if (toTarget.magnitude < m_cornerReachThreshold)
            {
                if (m_currentPathIndex == m_teleportIndex)
                {
                    return;
                }
                else
                {
                    m_currentPathIndex++;
                    return;
                }
            }

            // Move AI toward current target corner
            Vector2 direction = new Vector2(toTarget.x, toTarget.z).normalized;
            Move(direction);
        }

        private bool GetInRangeEnemy(out Player target)
        {
            target = null;

            if (m_enemies == null
                || m_enemies.Count == 0
                || m_attackInterface.CurrentWeapon() == null)
                return false;

            float distance = Mathf.Sqrt(m_attackInterface.EquipedAttack().max_distance_sqr);
            Vector3 position = m_attackInterface.CurrentWeapon().GetFirePosition();

            foreach (var enemy in m_enemies)
            {
                if (enemy == null)
                    continue;

                Vector3 direction = (enemy.transform.position - position).normalized;

                if (Physics.Raycast(position, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.gameObject == enemy.gameObject)
                    {
                        target = enemy;
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            if (m_currentPath == null || m_currentPath.Length < 2)
                return;

            Gizmos.color = Color.yellow;

            int breakIndex = Mathf.Clamp(m_teleportIndex, 0, m_currentPath.Length);

            // Draw path up to teleportIndex (excluded)
            for (int i = 0; i < breakIndex; i++)
            {
                Gizmos.DrawLine(m_currentPath[i], m_currentPath[i + 1]);
                Gizmos.DrawSphere(m_currentPath[i], 0.5f);
            }

            // Draw teleport corner sphere if valid
            if (m_teleportIndex >= 0 && m_teleportIndex < m_currentPath.Length)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawSphere(m_currentPath[m_teleportIndex], 1f);
            }

            Gizmos.color = Color.yellow;

            // Draw path after teleportIndex
            for (int i = breakIndex + 1; i < m_currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(m_currentPath[i], m_currentPath[i + 1]);
                Gizmos.DrawSphere(m_currentPath[i], 0.5f);
            }

            // Draw last corner sphere
            Gizmos.DrawSphere(m_currentPath[m_currentPath.Length - 1], 0.5f);
        }
    }
}