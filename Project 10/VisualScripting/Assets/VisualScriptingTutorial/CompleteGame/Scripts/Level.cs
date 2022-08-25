using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VisualScriptingTutorial
{
    public class Level : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static Level s_Instance;
        public static Level Instance => s_Instance;

        public static readonly Vector3Int[] NeighbourOffsets =
        {
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, -1),
            new Vector3Int(-1, 0, 0)
        };

        [Serializable]
        public class Cell
        {
            public CellObject Object = null;
        
            public int[] Edges = new int[4];

            public int Up => Edges[0];
            public int Right => Edges[1];
            public int Down => Edges[2];
            public int Left => Edges[3];

            public List<MovingObject> ContainedMovingObjects = new List<MovingObject>();

            public GameObject[] Visuals;
            public int PrefabIndex = -1; //used to always pick the same prefab when regenerating
            public int Rotation = -1;
        
            public static int GetEdgeIndexBetween(Vector3Int start, Vector3Int end)
            {
                if (start.x - end.x != 0)
                {
                    return start.x > end.x ? 3 : 1;
                }

                if (start.z - end.z != 0)
                {
                    return start.z > end.z ? 2 : 0;
                }

                return -1;
            }
        }

        [Serializable]
        public class Edge
        {
            public GameObject[] Visuals;
            public bool Passable;
            public EdgeObject EdgeObject;
            public int PrefabIndex = -1; //used to always reuse the same prefab when regenerating level to keep it consistent
            public int DecorationPrefabIndex = -1;
        }

        public enum EdgeDirection
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        public LevelTheme Theme;

        public Vector3Int SpawnPoint => m_SpawnPoint;
        public Vector3Int EndPoint => m_EndPoint;
        public Grid Grid => m_Grid;
        public List<Edge> Edges => m_Edges;
        public Dictionary<Vector3Int, Cell> Cells => m_Cells;

        [HideInInspector] [SerializeField] private GameObject GeometryRoot;
        [HideInInspector] [SerializeField] private GameObject ObjectRoot;

        private Dictionary<Vector3Int, Cell> m_Cells = new Dictionary<Vector3Int, Cell>();

        [HideInInspector] [SerializeField] private List<Edge> m_Edges = new List<Edge>();

        [HideInInspector] [SerializeField] private List<int> m_FreeEdgesIndex = new List<int>();

        [HideInInspector] [SerializeField] private List<Vector3Int> m_CellsKey = new List<Vector3Int>();

        [HideInInspector] [SerializeField] private List<Cell> m_CellsValue = new List<Cell>();

        [HideInInspector] [SerializeField] private Vector3Int m_SpawnPoint;
        [HideInInspector] [SerializeField] private Vector3Int m_EndPoint;

        [HideInInspector] [SerializeField] private InteractiveCutWall[] m_InteractiveCutWalls = new InteractiveCutWall[0];

        private Grid m_Grid;

        private void Awake()
        {
            s_Instance = this;
            m_Grid = GetComponentInParent<Grid>();
        }

        private void Start()
        {
            SpawnPlayer();
        }
        
        public void OnAfterDeserialize()
        {
            m_Cells.Clear();

            for (int i = 0; i < m_CellsKey.Count; ++i)
            {
                m_Cells.Add(m_CellsKey[i], m_CellsValue[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        void SpawnPlayer()
        {
            EntryPoint.Instance.PlayerInstance.gameObject.SetActive(true);
            EntryPoint.Instance.PlayerInstance.TeleportTo(new Vector2(SpawnPoint.x, SpawnPoint.z));
        }

        public void CameraMoved(Vector3 cameraPosition)
        {
            foreach (var wall in m_InteractiveCutWalls)
            {
                Vector3 viewDir = (cameraPosition - wall.transform.position).normalized;
                wall.UpdateWall(viewDir);
            }
        }

        public Cell GetCell(Vector3Int cell)
        {
            Cell c = null;
            m_Cells.TryGetValue(cell, out c);

            return c;
        }

        public bool IsCellEmpty(Vector3Int cell)
        {
            Cell c = null;
            m_Cells.TryGetValue(cell, out c);

            return c != null && c.Object == null;
        }

        public Edge GetEdge(Vector3Int start, Vector3Int end)
        {
            Cell c;

            if (m_Cells.TryGetValue(start, out c))
            {
                var idx = Cell.GetEdgeIndexBetween(start, end);
                if (idx != -1) return m_Edges[c.Edges[idx]];
            }
            else if (m_Cells.TryGetValue(end, out c))
            {
                var idx = Cell.GetEdgeIndexBetween(end, start);
                if (idx != -1) return m_Edges[c.Edges[idx]];
            }

            return null;
        }

        // Everything under is wrapped into that define, allowing to strip all that code from build. All those function
        // are only used by the Level Editor to modify the level in editor.
#if UNITY_EDITOR

        [MenuItem("Tutorial/Create Level")]
        static void CreateLevel()
        {
            //Create the level if it don't exist
            if (FindObjectOfType<Level>() == null)
            {
                var obj = new GameObject("Grid");
                var grd = obj.AddComponent<Grid>();
                grd.cellSize = new Vector3(1, 0, 1);

                var lvl = new GameObject("Level");
                lvl.transform.SetParent(obj.transform, false);

                var levelCmp = lvl.AddComponent<Level>();

                var themePaths = AssetDatabase.FindAssets("DefaultTheme t:LevelTheme");
                if (themePaths.Length != 0)
                {
                    var theme = AssetDatabase.LoadAssetAtPath<LevelTheme>(AssetDatabase.GUIDToAssetPath(themePaths[0]));
                    levelCmp.Theme = theme;
                }
                else
                {
                    Debug.LogError(
                        "Couldn't find the default Theme called Theme.asset in Data. Did you deleted or renamed it?");
                }
            }
            
            //find if the main camera is our main Camera prefab
            var cam = FindObjectOfType<Camera>();
            if (cam == null || !PrefabUtility.IsPartOfPrefabInstance(cam.gameObject))
            {
                if(cam != null)
                    DestroyImmediate(cam.gameObject);

                var camPrefabGUID = AssetDatabase.FindAssets("Main Camera");
                if (camPrefabGUID.Length > 0)
                {
                    var camPrefab =
                        AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(camPrefabGUID[0]));

                    PrefabUtility.InstantiatePrefab(camPrefab);
                }
                else
                {
                    Debug.LogError("Couldn't find Main Camera prefab, did you renamed or deleted it?");
                }
            }
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    
        int CreateNewEdge()
        {
            int index = 0;
            if (m_FreeEdgesIndex.Count > 0)
            {
                index = m_FreeEdgesIndex[m_FreeEdgesIndex.Count - 1];
                m_FreeEdgesIndex.RemoveAt(m_FreeEdgesIndex.Count - 1);
            }
            else
            {
                m_Edges.Add(new Edge());
                index = m_Edges.Count - 1;
            }

            m_Edges[index].Passable = false;
        
            return index;
        }

        public Cell GetOrCreateCell(Vector3Int cellIdx)
        {
            if (m_Cells.ContainsKey(cellIdx))
                return m_Cells[cellIdx];

            var cell = new Cell();

            for (int i = 0; i < 4; ++i)
            {
                Cell c;
                if (m_Cells.TryGetValue(cellIdx + NeighbourOffsets[i], out c))
                {
                    var edgeIdx = (i + 2) % 4;
                    //we have a cell in that direction already, reuse the same edge
                    cell.Edges[i] = c.Edges[edgeIdx];
                    //we also make that edge passable, as edge are by default non passable (so you can't get out)
                    m_Edges[cell.Edges[i]].Passable = true;
                }
                else
                {
                    //no cell in that direction still, just create a new edge
                    cell.Edges[i] = CreateNewEdge();
                }
            }

            m_CellsValue.Add(cell);
            m_CellsKey.Add(cellIdx);
            
            m_Cells.Add(cellIdx, cell);

            RegenerateCellAndNeighbour(cellIdx);

            return cell;
        }

        public void RemoveCell(Vector3Int cell)
        {
            Cell c;
            if (!m_Cells.TryGetValue(cell, out c))
                return;

            foreach (var visual in c.Visuals)
            {
                DestroyImmediate(visual);
            }

            c.Visuals = null;

            for (int i = 0; i < 4; ++i)
            {
                if (!m_Cells.ContainsKey(cell + NeighbourOffsets[i]))
                {
                    var e = m_Edges[c.Edges[i]];

                    foreach (var o in e.Visuals)
                    {
                        if(o == null) continue;

                        InteractiveCutWall wall = o.GetComponent<InteractiveCutWall>();
                        if(wall != null) ArrayUtility.Remove(ref m_InteractiveCutWalls, wall);
                    
                        DestroyImmediate(o);
                    }
                
                    e.Visuals = new GameObject[0];
                
                    //we don't have a neighbour in that direction, free the edge to be reused now.
                    m_FreeEdgesIndex.Add(c.Edges[i]);
                }
                else
                {
                    //we set the edge to not passable anymore as it is now an empty cell
                    m_Edges[c.Edges[i]].Passable = false;
                }
            }

            int idx = m_CellsValue.FindIndex(current => c == current);
            m_CellsValue.RemoveAt(idx);
            m_CellsKey.RemoveAt(idx);
            
            m_Cells.Remove(cell);
        
            RegenerateCellAndNeighbour(cell);
        }

        public void SetSpawn(Vector3Int cell)
        {
            m_SpawnPoint = cell;
        }

        public void SetEnd(Vector3Int cell)
        {
            m_EndPoint = cell;
        }

        public void SwitchEdgePassable(Vector3Int cellIdx, EdgeDirection edgeDir)
        {
            Cell cell = GetCell(cellIdx);

            if (cell == null)
                return;

            var edge = m_Edges[cell.Edges[(int) edgeDir]];
            edge.Passable = !edge.Passable;

            EditorUtility.SetDirty(this);

            RegenerateCellAndNeighbour(cellIdx);
        }

        public void CycleEdgeWallPrefab(Vector3Int cellIdx, EdgeDirection edgeDir)
        {
            Cell cell = GetCell(cellIdx);

            if (cell == null)
                return;

            var edge = m_Edges[cell.Edges[(int) edgeDir]];
            edge.PrefabIndex += 1;
            if (edge.PrefabIndex >= Theme.WallPrefab.Length)
                edge.PrefabIndex = 0;
        
            EditorUtility.SetDirty(this);

            RegenerateCellAndNeighbour(cellIdx);
        }
    
        public void CycleGroundPrefab(Vector3Int cellIdx)
        {
            Cell cell = GetCell(cellIdx);

            if (cell == null)
                return;

            cell.PrefabIndex += 1;
            if (cell.PrefabIndex >= Theme.GroundPrefab.Length)
                cell.PrefabIndex = 0;

            EditorUtility.SetDirty(this);

            RegenerateCellAndNeighbour(cellIdx);
        }

        public void SetEdgeObjectEditor(Vector3Int cellIdx, EdgeDirection edgeDir, EdgeObject prefab)
        {
            Cell cell = GetCell(cellIdx);

            if (cell == null)
                return;

            var edgeIdx = cell.Edges[(int) edgeDir];
            var edge = m_Edges[edgeIdx];
            
            if (edge.EdgeObject != null)
            {
                DestroyImmediate(edge.EdgeObject.gameObject);
                edge.EdgeObject = null;
            }

            if (prefab != null)
            {
                CheckObjectRoot();

                var edgeObject = (EdgeObject)PrefabUtility.InstantiatePrefab(prefab, ObjectRoot.transform);
            
                var grid = GetComponentInParent<Grid>();
            
                float[] rotation = {0, 90, 180, 270};
                edgeObject.transform.Rotate(Vector3.up, rotation[(int)edgeDir]);
                edgeObject.transform.position = grid.GetCellCenterWorld(cellIdx);

                edge.EdgeObject = edgeObject;
                edgeObject.SetOwner(edgeIdx);
            }
        
            EditorUtility.SetDirty(this);
        }

        public void SetCellObjectEditor(Vector3Int cellIdx, CellObject prefab)
        {
            Cell cell = GetCell(cellIdx);

            if (cell == null)
                return;
            
            if (cell.Object != null)
            {
                DestroyImmediate(cell.Object.gameObject);
                cell.Object = null;
            }

            if (prefab != null)
            {
                CheckObjectRoot();

                var obj = (CellObject) PrefabUtility.InstantiatePrefab(prefab, ObjectRoot.transform);
                var grid = GetComponentInParent<Grid>();
            
                obj.transform.position = grid.GetCellCenterWorld(cellIdx);

                cell.Object = obj;
            }
        
            //may need to regenerate if the object added or removed override the mesh (like a lava pit)
            RegenerateMesh(cellIdx);
            EditorUtility.SetDirty(this);
        }

        void CheckObjectRoot()
        {
            if (ObjectRoot == null)
            {
                ObjectRoot = new GameObject("ObjectRoot");
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        [ContextMenu("Regenerate Mesh")]
        void RegenerateAllMesh()
        {
            foreach (var cell in m_Cells)
            {
                RegenerateMesh(cell.Key);
            }
        }
    
        [ContextMenu("Regenerate Mesh with new RNG")]
        void RegenerateAllMeshNewRNG()
        {
            foreach (var cell in m_Cells)
            {
                RegenerateMesh(cell.Key, true);
            }
        }

        void RegenerateCellAndNeighbour(Vector3Int cellIdx)
        {
            RegenerateMesh(cellIdx);

            for (int i = 0; i < 4; ++i)
            {
                RegenerateMesh(cellIdx + NeighbourOffsets[i]);
            }
        }

        void RegenerateMesh(Vector3Int cellIdx, bool rerollRNG = false)
        {
            if (GeometryRoot == null)
            {
                GeometryRoot = new GameObject("GeometryRoot");
                GeometryRoot.isStatic = true;
            }
        
            var cell = GetCell(cellIdx);
            if(cell == null)
                return;
        
            var grid = GetComponentInParent<Grid>();
            var pos = grid.GetCellCenterWorld((Vector3Int)cellIdx);

            pos.y = 0;

            if (cell.Visuals != null && cell.Visuals.Length > 0)
            {
                foreach (var o in cell.Visuals)
                {
                    DestroyImmediate(o);
                }
            }
        
            cell.Visuals = new GameObject[0];
        
            //first add ground
            //but only if we don't have a cell object there that override the ground mesh
            if (cell.Object == null || cell.Object.AddGroundMesh)
            {
                if (rerollRNG || (cell.PrefabIndex == -1 || cell.PrefabIndex >= Theme.GroundPrefab.Length || cell.Rotation == -1))
                {
                    cell.Rotation = Random.Range(0, 4);
                    cell.PrefabIndex = Random.Range(0, Theme.GroundPrefab.Length);
                }

                var g = (GameObject) PrefabUtility.InstantiatePrefab(Theme.GroundPrefab[cell.PrefabIndex],
                    GeometryRoot.transform);
            
                g.transform.position = pos;
                g.transform.rotation = Quaternion.Euler(0, cell.Rotation * 90.0f, 0);

                ArrayUtility.Add(ref cell.Visuals, g);
            }

            //then add walls
            float[] rotation = {0, 90, 180, 270};

            for (int i = 0; i < 4; ++i)
            {
                var edge = m_Edges[cell.Edges[i]];
            
                if (edge.Visuals != null && edge.Visuals.Length > 0)
                {
                    foreach (var o in edge.Visuals)
                    {
                        InteractiveCutWall wall = o.GetComponent<InteractiveCutWall>();
                        if(wall != null) ArrayUtility.Remove(ref m_InteractiveCutWalls, wall);
                    
                        DestroyImmediate(o);
                    }
                }
            
                edge.Visuals = new GameObject[0];

                if (!edge.Passable)
                {
                    bool isInternalWall = GetCell(cellIdx + NeighbourOffsets[i]) != null;
                
                    //if the neighbour cell in that direction isn't null, this is an internal wall
                    GameObject[] prefabArray = isInternalWall
                        ? Theme.InsideWallPrefab
                        : Theme.WallPrefab;
                
                    if (rerollRNG || (edge.PrefabIndex < 0 || edge.PrefabIndex >= prefabArray.Length))
                    {
                        edge.PrefabIndex = Random.Range(0, prefabArray.Length);
                    }

                    var inst = (GameObject)PrefabUtility.InstantiatePrefab( prefabArray[edge.PrefabIndex], GeometryRoot.transform);
                    inst.transform.position = pos;

                    inst.transform.Rotate(Vector3.up, rotation[i]);
                    ArrayUtility.Add(ref edge.Visuals, inst);

                    //check if this have an interactive cut wall and add it to the list for fast traversal when updating camera
                    InteractiveCutWall wall = inst.GetComponent<InteractiveCutWall>();
                    if (wall != null)
                    {
                        wall.IsInternalWall = isInternalWall;
                        ArrayUtility.Add(ref m_InteractiveCutWalls, wall);
                    
                    
                        // Add wall decoration
                        int childCount = wall.DecorationRoot.childCount;
                        if (rerollRNG || (edge.DecorationPrefabIndex < 0 ||
                                          edge.DecorationPrefabIndex > childCount))
                        {
                            //+1 as 0 mean no decoration, we just offset everything by 1
                            edge.DecorationPrefabIndex = Random.Range(0, childCount + 1);
                        }

                        if (edge.DecorationPrefabIndex > 0)
                        {
                            wall.DecorationRoot.GetChild(edge.DecorationPrefabIndex - 1).gameObject.SetActive(true);
                        }
                    }
                }
            }
        
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void OnDrawGizmos()
        {
            if(m_Grid == null)  m_Grid = GetComponentInParent<Grid>();
        
            Vector3 spawnCellPos = m_Grid.GetCellCenterWorld(m_SpawnPoint);
        
            Handles.DrawSolidRectangleWithOutline(new Vector3[]
            {
                spawnCellPos + new Vector3(-0.5f, 0, -0.5f),
                spawnCellPos + new Vector3(0.5f, 0, -0.5f),
                spawnCellPos + new Vector3(0.5f, 0, 0.5f),
                spawnCellPos + new Vector3(-0.5f, 0, 0.5f),
            }, new Color(0,1,0,0.2f), Color.green);
        
            Vector3 endCellPos = m_Grid.GetCellCenterWorld(m_EndPoint);
        
            Handles.DrawSolidRectangleWithOutline(new Vector3[]
            {
                endCellPos + new Vector3(-0.5f, 0, -0.5f),
                endCellPos + new Vector3(0.5f, 0, -0.5f),
                endCellPos + new Vector3(0.5f, 0, 0.5f),
                endCellPos + new Vector3(-0.5f, 0, 0.5f),
            }, new Color(1,0,0,0.2f), Color.red);
        }

#endif
    }
}