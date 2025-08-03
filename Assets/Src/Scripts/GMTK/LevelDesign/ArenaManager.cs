using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
using CustomArchitecture;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace GMTK
{
    [CustomEditor(typeof(ArenaManager))]
    public class ArenaManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ArenaManager arenaManager = (ArenaManager)target;

            if (GUILayout.Button("Bake 3x3 Wrapped NavMesh In Scene"))
            {
                Bake3x3WrappedNavMeshInScene(arenaManager);
            }
        }

        private void Bake3x3WrappedNavMeshInScene(ArenaManager arenaManager)
        {
            if (arenaManager.LevelDesignPrefab == null || arenaManager.AiUnwrapPathNavMeshSurface == null)
            {
                Debug.LogError("Missing prefab or NavMeshSurface.");
                return;
            }

            var prefab = arenaManager.LevelDesignPrefab;
            var surface = arenaManager.AiUnwrapPathNavMeshSurface;

            // Get arena size
            var levelDesign = prefab.GetComponent<LevelDesign>();
            if (levelDesign == null)
            {
                Debug.LogError("LevelDesignPrefab must contain LevelDesign component.");
                return;
            }

            Vector3 arenaSize = levelDesign.GetPlaneSize();
            GameObject container = new GameObject("NavMeshBake_ClonesContainer");
            container.transform.SetParent(arenaManager.transform);

            List<GameObject> clones = new List<GameObject>();
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 offset = new Vector3(x * arenaSize.x, 0f, z * arenaSize.z);
                    GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    clone.transform.position = offset;
                    clone.transform.SetParent(container.transform);
                    clone.name = $"LevelDesignClone_{x}_{z}";
                    clones.Add(clone);
                }
            }
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();
            var obstacles = GameObject.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);

            foreach (var obstacle in obstacles)
            {
                // Mark the whole obstacle GameObject as Not Walkable
                var markup = new NavMeshBuildMarkup
                {
                    root = obstacle.transform,
                    overrideArea = true,
                    area = NavMesh.GetAreaFromName("Not Walkable"), // usually 1
                    ignoreFromBuild = false // Important: we WANT to include this, but override the area
                };

                markups.Add(markup);

                // OPTIONAL: move visuals to a different layer excluded by layerMask
                // or disable MeshRenderers/Colliders before CollectSources if needed
            }

            NavMeshBuilder.CollectSources(
                surface.transform,
                surface.layerMask,
                surface.useGeometry,
                surface.defaultArea,
                markups,
                sources
            );
            //NavMeshBuilder.CollectSources(
            //    surface.transform,
            //    surface.layerMask,
            //    surface.useGeometry,
            //    surface.defaultArea,
            //    new List<NavMeshBuildMarkup>(),
            //    sources
            //);
            // Compute bounds manually
            Bounds bounds = new Bounds(Vector3.zero, new Vector3(3 * arenaSize.x, 20f, 3 * arenaSize.z));

            //foreach (var obs in obstacles)
            //{
            //    obs.gameObject.SetActive(false);
            //}

            // Build NavMeshData manually
            var buildSettings = NavMesh.GetSettingsByID(surface.agentTypeID);

            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(
                buildSettings,
                sources,
                bounds,
                surface.transform.position,
                surface.transform.rotation
            );

            //foreach (var obs in obstacles)
            //{
            //    obs.gameObject.SetActive(true);
            //}

            if (navMeshData == null)
            {
                Debug.LogError("Failed to build NavMeshData.");
                return;
            }

            // Add to scene
            NavMesh.AddNavMeshData(navMeshData);

            // Save as asset
            string folderPath = "Assets/BakedNavMeshes";
            string assetPath = $"{folderPath}/ArenaNavMeshData.asset";

            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets", "BakedNavMeshes");

            AssetDatabase.CreateAsset(navMeshData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"NavMeshData baked and saved to {assetPath}");

            // Assign to surface for future builds (optional)
            surface.navMeshData = navMeshData;

            // Cleanup temporary clones
            GameObject.DestroyImmediate(container);
            Debug.Log("Temporary clones cleaned up.");

            // Instantiate main LevelDesign instance at center
            GameObject mainInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            mainInstance.transform.position = Vector3.zero;
            mainInstance.transform.SetParent(arenaManager.transform);
            mainInstance.name = "MainLevelDesign";

            // Assign to runtime state field
            LevelDesign levelDesignComponent = mainInstance.GetComponent<LevelDesign>();
            SerializedObject serializedArenaManager = new SerializedObject(arenaManager);
            SerializedProperty prop = serializedArenaManager.FindProperty("m_mainLevelDesignInstance");
            prop.objectReferenceValue = levelDesignComponent;
            serializedArenaManager.ApplyModifiedProperties();

            Debug.Log("Main LevelDesign instance created and assigned.");

        }
    }
#endif

    public class ArenaManager : BaseBehaviour
    {
        [Header("Setup")]
        [SerializeField] private GameObject m_levelDesignPrefab;
        [SerializeField] private NavMeshSurface m_aiPathNMSurface;

        [Header("Runtime State")]
        [SerializeField, ReadOnly] private LevelDesign m_mainLevelDesignInstance;
        
        private NavMeshPath m_navPath1;
        private NavMeshPath m_navPath2;

        private List<Vector3> m_tempPathPoints = new List<Vector3>(64);

        private Vector3[] m_lastPath;
        private int m_lastTeleportIndex = -1;

        public NavMeshSurface AiUnwrapPathNavMeshSurface { get => m_aiPathNMSurface; protected set { } }
        public GameObject LevelDesignPrefab { get => m_levelDesignPrefab; protected set { } }
        public (Vector3, Vector3) GetArenaTransposerDatas() => (m_mainLevelDesignInstance.transform.position, m_mainLevelDesignInstance.GetPlaneSize());

        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        { }

        /// <summary>
        /// Computes the shortest path between two points on the 3x3 wrapped NavMesh.
        /// Returns true if path found, false otherwise.
        /// Outputs the path and teleport index (-1 if no teleport).
        /// </summary>
        public bool ComputeShortestPath(Vector3 fromPos, Vector3 toPos, out Vector3[] path, out int teleportIndex)
        {
            path = null;
            teleportIndex = -1;

            m_navPath1 ??= new NavMeshPath();

            Vector3 center = m_mainLevelDesignInstance.transform.position;
            Vector3 size = m_mainLevelDesignInstance.GetComponent<LevelDesign>().GetPlaneSize();
            Vector3 arenaMin = center - size * 0.5f;
            Vector3 arenaMax = center + size * 0.5f;
            Vector3 arenaSize = arenaMax - arenaMin;

            float bestLength = float.MaxValue;
            Vector3[] bestCorners = null;
            Vector3 bestOffset = Vector3.zero;

            // Try all 9 wrapped tiles
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = -1; zOffset <= 1; zOffset++)
                {
                    Vector3 offset = new Vector3(xOffset * arenaSize.x, 0f, zOffset * arenaSize.z);
                    Vector3 wrappedTo = toPos + offset;

                    if (!NavMesh.CalculatePath(fromPos, wrappedTo, NavMesh.AllAreas, m_navPath1))
                        continue;

                    float length = PathLength(m_navPath1.corners);
                    if (length < bestLength)
                    {
                        bestLength = length;
                        bestCorners = (Vector3[])m_navPath1.corners.Clone();
                        bestOffset = offset;
                    }
                }
            }

            if (bestCorners == null)
                return false;

            // Now post-process teleport logic
            if (bestOffset.sqrMagnitude > 0.01f)
            {
                for (int i = 0; i < bestCorners.Length; i++)
                {
                    if (IsOutsideArena(bestCorners[i]))
                    {
                        // Found first corner outside
                        teleportIndex = Mathf.Max(0, i - 1);

                        Vector3 beforeCorner = bestCorners[teleportIndex];
                        Vector3 afterCorner = bestCorners[i];

                        Vector3 dir = (afterCorner - beforeCorner).normalized;

                        Vector3 intersectionPoint = ComputeArenaBoundaryIntersection(beforeCorner, afterCorner, arenaMin, arenaMax);

                        intersectionPoint += dir * 2f;

                        List<Vector3> newCorners = new();

                        for (int j = 0; j <= teleportIndex; j++)
                            newCorners.Add(bestCorners[j]);

                        newCorners.Add(intersectionPoint);

                        for (int j = teleportIndex + 1; j < bestCorners.Length; j++)
                            newCorners.Add(WrapPositionToArena(bestCorners[j], arenaMin, arenaMax));

                        path = newCorners.ToArray();
                        m_lastPath = path;
                        m_lastTeleportIndex = teleportIndex + 1;
                        teleportIndex = m_lastTeleportIndex;

                        return true;
                    }
                }
            }

            // No teleport case, all inside
            for (int i = 0; i < bestCorners.Length; i++)
                bestCorners[i] = WrapPositionToArena(bestCorners[i], arenaMin, arenaMax);

            path = bestCorners;
            m_lastPath = path;
            m_lastTeleportIndex = teleportIndex;
            return true;
        }


        // Calculate length of path
        private float PathLength(Vector3[] points)
        {
            float length = 0f;
            for (int i = 1; i < points.Length; i++)
            {
                length += Vector3.Distance(points[i - 1], points[i]);
            }
            return length;
        }
        private Vector3 WrapPositionToArena(Vector3 pos, Vector3 arenaMin, Vector3 arenaMax)
        {
            float width = arenaMax.x - arenaMin.x;
            float depth = arenaMax.z - arenaMin.z;

            float wrappedX = pos.x;
            float wrappedZ = pos.z;

            if (pos.x < arenaMin.x)
                wrappedX += width;
            else if (pos.x > arenaMax.x)
                wrappedX -= width;

            if (pos.z < arenaMin.z)
                wrappedZ += depth;
            else if (pos.z > arenaMax.z)
                wrappedZ -= depth;

            return new Vector3(wrappedX, pos.y, wrappedZ);
        }

        // Example get point just outside arena boundary by pushing slightly outside arena bounds
        private Vector3 GetPointJustOutsideArena(Vector3 point, Vector3 arenaMin, Vector3 arenaMax, float margin)
        {
            Vector3 clamped = point;

            if (point.x < arenaMin.x) clamped.x = arenaMin.x - margin;
            else if (point.x > arenaMax.x) clamped.x = arenaMax.x + margin;

            if (point.z < arenaMin.z) clamped.z = arenaMin.z - margin;
            else if (point.z > arenaMax.z) clamped.z = arenaMax.z + margin;

            clamped.y = point.y; // keep original y
            return clamped;
        }

        public bool IsOutsideArena(Vector3 pos)
        {
            Vector3 center = m_mainLevelDesignInstance.transform.position;
            Vector3 size = m_mainLevelDesignInstance.GetComponent<LevelDesign>().GetPlaneSize();

            Vector3 arenaMin = center - size * 0.5f;
            Vector3 arenaMax = center + size * 0.5f;

            return pos.x < arenaMin.x || pos.x > arenaMax.x || pos.z < arenaMin.z || pos.z > arenaMax.z;
        }
        Vector3 ComputeArenaBoundaryIntersection(Vector3 insidePos, Vector3 outsidePos, Vector3 min, Vector3 max)
        {
            Vector3 dir = outsidePos - insidePos;

            // We only care about X and Z axes (assuming Y irrelevant here)
            // For each axis, if outsidePos is out of bounds, intersect with that plane

            if (outsidePos.x < min.x)
                return IntersectWithPlane(insidePos, dir, min.x, Axis.X);
            if (outsidePos.x > max.x)
                return IntersectWithPlane(insidePos, dir, max.x, Axis.X);

            if (outsidePos.z < min.z)
                return IntersectWithPlane(insidePos, dir, min.z, Axis.Z);
            if (outsidePos.z > max.z)
                return IntersectWithPlane(insidePos, dir, max.z, Axis.Z);

            // if no axis outside? return insidePos (shouldn't happen if outsidePos truly outside)
            return insidePos;
        }

        enum Axis { X, Z }

        Vector3 IntersectWithPlane(Vector3 origin, Vector3 dir, float planePos, Axis axis)
        {
            float t;
            if (axis == Axis.X)
            {
                // line: origin + t * dir
                // solve for x = planePos
                if (dir.x == 0) return origin; // parallel, no intersection
                t = (planePos - origin.x) / dir.x;
            }
            else // Z
            {
                if (dir.z == 0) return origin;
                t = (planePos - origin.z) / dir.z;
            }

            t = Mathf.Clamp01(t); // clamp so intersection is between origin and origin+dir

            Vector3 intersection = origin + dir * t;
            return intersection;
        }

        private void OnDrawGizmos()
        {
            Vector3 center = m_mainLevelDesignInstance.transform.position;
            Vector3 size = m_mainLevelDesignInstance.GetComponent<LevelDesign>().GetPlaneSize();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
