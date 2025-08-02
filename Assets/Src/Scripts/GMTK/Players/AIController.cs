using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AI;
using UnityEngine;

namespace GMTK
{
    public class AIController : Player
    {
        private List<Player> m_enemies = null;
        private int currentCornerIndex = 0;

        private Vector3[] m_currentPath;
        private int m_currentPathIndex = 0;

        [SerializeField] private Transform  m_target;
        [SerializeField] private float      m_cornerReachThreshold;
        private NavMeshPath m_navPath;

        #region BaseBehaviour_Cb
        public IEnumerator Load()
        {
            yield return null;
        }

        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not List<Player>)
            {
                UnityEngine.Debug.LogWarning("wrong parameters");
                return;
            }

            m_enemies = (List<Player>)parameters[0];
        }

        public override void LateInit(params object[] parameters)
        {
        }

        protected override void OnFixedUpdate()
        {
            //FollowPath();
        }

        protected override void OnLateUpdate()
        {
        }

        protected override void OnUpdate()
        {
            //if (!IsMoving)
            //{
            //    NoMove();
            //}

            if (Input.GetKeyDown(KeyCode.F1))
                ComputePathTo(m_target.position);
        }
        #endregion BaseBehaviour_Cb

        private void FollowPath()
        {
            if (m_currentPathIndex >= m_currentPath.Length)
                return;

            Vector3 currentTarget = m_currentPath[m_currentPathIndex];
            Vector3 toTarget = currentTarget - transform.position;
            toTarget.y = 0;

            if (toTarget.magnitude < m_cornerReachThreshold)
            {
                m_currentPathIndex++;
                return;
            }

            Vector2 direction = new Vector2(toTarget.x, toTarget.z).normalized;
            Move(direction);
        }

        private void ComputePathTo(Vector3 targetPosition)
        {
            m_navPath ??= new NavMeshPath();

            if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, m_navPath))
            {
                m_currentPath = m_navPath.corners;
                m_currentPathIndex = 1;
            }
        }
    }
}