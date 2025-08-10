using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using CustomArchitecture;
using System;
using Unity.VisualScripting;

namespace GMTK
{
    public class AIDecision
    {
        public string Name;
        public Func<bool> Action;
        public float BaseWeight = 0f;
        public float Scale = 1f;

        public float EffectivScale { get => BaseWeight * Scale; protected set { } }

        public AIDecision(string name, Func<bool> action, float baseWeight)
        {
            Name = name;
            Action = action;
            BaseWeight = baseWeight;
        }
    }

    public class AIController : Player
    {
        [SerializeField] private float m_cornerReachThreshold;

        private List<AIDecision> m_decisionMaking = null;

        private List<Player> m_enemies = null;
        private Player m_targetEnemy = null;

        private Vector3[] m_currentPath = null;
        private int m_currentPathIndex = 0;

        private bool m_justTeleport = false;
        private int m_teleportIndex = -1;

        private AIAttackInterface m_attackInterface = null;
        public override AttackInterface GetAttackManager() => m_attackInterface;

        bool m_isStuck = false;
        bool m_isInit = false;
        Vector3 m_lastPosition = Vector3.zero;
        float m_lastMoveTime = 0f;
        float m_stuckCheckInterval = 1f;
        float m_minMoveDistance = 0.05f;


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

            m_decisionMaking = new()
            {
                new AIDecision("MoveToRandomPosition", MoveToRandomPlayer, .3f),
                new AIDecision("MoveToRandomEnemy", MoveToClosestEnemy, .8f),
                new AIDecision("MoveToClosestEnemy", MoveToClosestEnemy, .5f),
                new AIDecision("FleeFromClosest", FleeFromClosestEnemy, 1f),
            };

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
                    UnityEngine.Random.Range(center.x - size.x / 2f, center.x + size.x / 2f),
                    center.y,
                    UnityEngine.Random.Range(center.z - size.z / 2f, center.z + size.z / 2f)
                );

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            return false;
        }
        public bool MoveToRandomPoint()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null)
                return false;

            var datas = GMTKGameCore.Instance.MainGameMode.GetArenaTransposerDatas();

            if (GetRandomPointInBounds(datas.Item1, datas.Item2, out var result))
            {
                if (ComputeShortestWrappedPath(transform.position, result))
                {
                    StartMove();
                    return true;
                }
            }

            return false;
        }

        public bool MoveToRandomPlayer()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null
                || m_enemies.Count == 0)
                return false;

            var datas = GMTKGameCore.Instance.MainGameMode.GetArenaTransposerDatas();

            int index = UnityEngine.Random.Range(0, m_enemies.Count - 1);

            Vector3 position = m_enemies[index].transform.position;
            Player enemy = m_enemies[index];

            if (enemy == null || !enemy.IsRagdoll)
                return false;

            if (ComputeShortestWrappedPath(transform.position, position))
            {
                StartMove();
                return true;
            }

            return false;
        }

        public bool FleeFromClosestEnemy()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null || m_enemies.Count == 0)
                return false;

            Player closestEnemy = null;
            float closestSqrDistance = float.MaxValue;
            Vector3 myPosition = transform.position;

            foreach (var enemy in m_enemies)
            {
                if (enemy == null || !enemy.IsRagdoll)
                    continue;

                float sqrDistance = (enemy.transform.position - myPosition).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy == null)
                return false;

            // Calculate flee direction (away from closest enemy)
            Vector3 fleeDirection = (myPosition - closestEnemy.transform.position).normalized;

            // Define a flee distance (you can tweak this)
            float fleeDistance = 5f;

            // Get arena bounds
            var (center, size) = GMTKGameCore.Instance.MainGameMode.GetArenaTransposerDatas();

            // Try to find a valid flee point in the flee direction
            for (int i = 0; i < 5; i++) // Try multiple distances if needed
            {
                Vector3 fleeTarget = myPosition + fleeDirection * (fleeDistance * (1 + i * 0.5f));

                //// Clamp to arena bounds
                //fleeTarget = ClampToBounds(fleeTarget, center, size);

                if (ComputeShortestWrappedPath(myPosition, fleeTarget))
                {
                    StartMove();
                    return true;
                }
            }

            return false;
        }

        public bool MoveToClosestEnemy()
        {
            if (GMTKGameCore.Instance.MainGameMode.GetGameManager() == null || m_enemies.Count == 0)
                return false;

            Player closestEnemy = null;
            float closestSqrDistance = float.MaxValue;
            Vector3 myPosition = transform.position;

            foreach (var enemy in m_enemies)
            {
                if (enemy == null || !enemy.IsRagdoll)
                    continue;

                float sqrDistance = (enemy.transform.position - myPosition).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy == null)
                return false;

            if (ComputeShortestWrappedPath(myPosition, closestEnemy.transform.position))
            {
                StartMove();
                return true;
            }

            return false;
        }

        protected override void OnUpdate()
        {
            if (!m_isInit || m_isRagdoll) return;

            m_enemies.RemoveAll(enemy => enemy.IsDead());

            if (!IsMoving)
            {
                NoMove();
                if (!MoveToRandomPlayer())
                    MoveToRandomPoint();

                //MovementDecisionMaking();
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

        private void MovementDecisionMaking()
        {
            // Compute total weight
            float totalWeight = 0f;
            foreach (var option in m_decisionMaking)
            {
                if (option.Name == "FleeFromClosest")
                    option.Scale = m_prctDamages / 100f;
                else
                    option.Scale = 1f;
                totalWeight += option.EffectivScale;
            }

            if (totalWeight <= 0f)
                return;

            // Roll a weighted random selection
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float accum = 0f;

            foreach (var option in m_decisionMaking)
            {
                accum += option.EffectivScale;
                if (roll <= accum)
                {
                    option.Action.Invoke();
                    break;
                }
            }

            //MoveToRandomPlayer();
            //MoveToRandomPoint();
            //MoveToClosestEnemy();
            //FleeFromClosestEnemy();
        }

        private void CheckIfStuck()
        {
            if (!IsMoving || m_currentPath == null)
            {
                m_isStuck = false;
                return;
            }

            if (Time.time - m_lastMoveTime > m_stuckCheckInterval)
            {
                float distanceMoved = (transform.position - m_lastPosition).sqrMagnitude;
                if (distanceMoved < m_minMoveDistance * m_minMoveDistance)
                {
                    if (!m_isStuck)
                    {
                        m_isStuck = true;
                        Debug.LogWarning($"{gameObject.name} seems to be stuck!");

                        // Try to fix by recomputing path
                        ClearPath();
                        MoveToRandomPoint(); // or MoveToRandomPlayer() based on your logic
                    }
                }
                else
                {
                    m_isStuck = false;
                }

                m_lastPosition = transform.position;
                m_lastMoveTime = Time.time;
            }
        }


        protected override void GetHit(Collision collision)
        {
            if (GetAttackManager().UseMinigun())
                return;

            m_attackInterface.StopAllAttacks();
            ClearPath();

            base.GetHit(collision);
        }

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

            float distance = m_attackInterface.EquipedAttack().max_distance_sqr;
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