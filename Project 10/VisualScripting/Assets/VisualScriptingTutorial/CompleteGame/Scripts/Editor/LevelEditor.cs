using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualScriptingTutorial;

public class LevelEditor : EditorWindow
{
    private Level m_Level;
    private Grid m_Grid;

    enum ToolType
    {
        Ground,
        Wall,
        Spawn,
        End,
        CellObject,
        EdgeObject,
        None
    }

    class PrefabEntry<T>
    {
        public List<T> Prefabs;
        public bool Folded;

        public PrefabEntry()
        {
            Prefabs = new List<T>();
            Folded = false;
        }
    }

    private ToolType m_Tool = ToolType.None;
    
    private bool m_GroundFirstClickType = false;
    private Vector3Int m_LastCell;

    private Dictionary<string, PrefabEntry<EdgeObject>> m_EdgeObjectPrefabs = new Dictionary<string, PrefabEntry<EdgeObject>>();
    private EdgeObject m_SelectedEdgeObject = null;
    
    private Dictionary<string, PrefabEntry<CellObject>> m_CellObjectsPrefabs = new Dictionary<string, PrefabEntry<CellObject>>();
    private CellObject m_SelectedCellObject = null;

    [MenuItem("Tutorial/Tools/Level Editor")]
    static void Open()
    {
        var editor = GetWindow<LevelEditor>();
        editor.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;

        Init();
    }

    private void OnFocus()
    {
        Selection.activeObject = null;
    }

    void Init()
    {
        m_Level = FindObjectOfType<Level>();
        
        if(m_Level == null)
            return;
        
        m_Grid = m_Level.GetComponentInParent<Grid>();

        m_EdgeObjectPrefabs = new Dictionary<string, PrefabEntry<EdgeObject>>
        {
            ["Tutorial"] = new PrefabEntry<EdgeObject>(),
            ["Sample"] = new PrefabEntry<EdgeObject>(),
            ["Custom"] = new PrefabEntry<EdgeObject>()
        };

        m_CellObjectsPrefabs = new Dictionary<string, PrefabEntry<CellObject>>
        {
            ["Tutorial"] = new PrefabEntry<CellObject>(),
            ["Sample"] = new PrefabEntry<CellObject>(),
            ["Custom"] = new PrefabEntry<CellObject>()
        };

        //for now look at all prefabs, TODO : check if there is a better way
        var prefabs = AssetDatabase.FindAssets("t:GameObject");

        foreach (var prefab in prefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefab);

            string category = "";
            if (path.Contains("CompleteGame"))
                category = "Sample";
            else if (path.Contains("/Tutorial")) //ne the / as the root folder is VisualScriptingTutorial
                category = "Tutorial";
            else
                category = "Custom";
            
            var loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var edgeObj = loadedPrefab.GetComponentInChildren<EdgeObject>();
            if (edgeObj != null)
            {
                m_EdgeObjectPrefabs[category].Prefabs.Add(edgeObj);
            }
            else
            {
                var cellObj = loadedPrefab.GetComponentInChildren<CellObject>();
                if (cellObj != null)
                {
                    m_CellObjectsPrefabs[category].Prefabs.Add(cellObj);
                }
            }
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }

    private void OnGUI()
    {
        if (m_Level == null || m_Grid == null)
        {
            GUILayout.Label("No Level or Grid found in this scene");
            Init();
            //if Init couldn't find a level, then exit, nothing to edit
            if(m_Level == null)
                return;
        }
        
        GUI.enabled = m_Tool != ToolType.Ground;
        if (GUILayout.Button("Ground"))
        {
            m_SelectedEdgeObject = null;
            m_Tool = ToolType.Ground;
        }

        GUI.enabled = m_Tool != ToolType.Wall;
        if (GUILayout.Button("Wall"))
        {
            m_SelectedEdgeObject = null;
            m_Tool = ToolType.Wall;
        }

        GUI.enabled = m_Tool != ToolType.Spawn;
        if (GUILayout.Button("Spawn Point"))
        {
            m_SelectedEdgeObject = null;
            m_Tool = ToolType.Spawn;
        }
        
        GUI.enabled = m_Tool != ToolType.End;
        if (GUILayout.Button("End Point"))
        {
            m_SelectedEdgeObject = null;
            m_Tool = ToolType.End;
        }
        
        //Edge Object
        EditorGUILayout.Separator();
        GUILayout.Label("Cell Object");

        var c = GUI.color;
        GUI.enabled = m_Tool != ToolType.CellObject || m_SelectedCellObject != null;
        if (GUILayout.Button("Erase"))
        {
            m_Tool = ToolType.CellObject;
            m_SelectedCellObject = null;
        }

        GUI.enabled = true;
        float buttonSize = position.width / 4;
        foreach(var pair in m_CellObjectsPrefabs)
        {
            pair.Value.Folded = EditorGUILayout.BeginFoldoutHeaderGroup(pair.Value.Folded, pair.Key);

            if (pair.Value.Folded)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < pair.Value.Prefabs.Count; ++i)
                {
                    if (i > 0 && i % 4 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    GUI.enabled = m_SelectedCellObject != pair.Value.Prefabs[i];
                    if (GUILayout.Button(pair.Value.Prefabs[i].name, GUILayout.MaxWidth(buttonSize)))
                    {
                        m_SelectedCellObject = pair.Value.Prefabs[i];
                        m_Tool = ToolType.CellObject;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        
        //Edge Object
        EditorGUILayout.Separator();
        GUILayout.Label("Edge Object");

        c = GUI.color;
        GUI.enabled = m_Tool != ToolType.EdgeObject || m_SelectedEdgeObject != null;
        if (GUILayout.Button("Erase"))
        {
            m_Tool = ToolType.EdgeObject;
            m_SelectedEdgeObject = null;
        }
        
        GUI.enabled = true;
        foreach (var pair in m_EdgeObjectPrefabs)
        {
            pair.Value.Folded = EditorGUILayout.BeginFoldoutHeaderGroup(pair.Value.Folded, pair.Key);

            if (pair.Value.Folded)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < pair.Value.Prefabs.Count; ++i)
                {
                    if (i > 0 && i % 4 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    GUI.enabled = m_SelectedEdgeObject != pair.Value.Prefabs[i];
                    if (GUILayout.Button(pair.Value.Prefabs[i].name, GUILayout.MaxWidth(buttonSize)))
                    {
                        m_SelectedEdgeObject = pair.Value.Prefabs[i];
                        m_Tool = ToolType.EdgeObject;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        
        GUI.enabled = true;
    }

    void SceneGUI(SceneView view)
    {
        if(!hasFocus)
            return;

        if(m_Level == null || m_Grid == null)
        {
            return;
        }
        
        var evt = Event.current;

        if (Event.current.type == EventType.Layout) 
        {
            if(GUIUtility.hotControl == HandleUtility.nearestControl)
                return;
            
            HandleUtility.AddDefaultControl(0);

            //If we have wall tool + hovering the scene view we force focus the scene view to detect R key press to switch wall type
            var pos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            if (view.position.Contains(pos) && m_Tool == ToolType.Wall)
            {
                view.Focus();
            }
        }
        
        var ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
        
        Plane p = new Plane(m_Level.transform.up, m_Level.transform.position);
        float d = 0.0f;

        if (p.Raycast(ray, out d))
        {
            var point = ray.GetPoint(d);
            
            //find closest edge
            var cell = m_Grid.WorldToCell(point);
            var worldCell = m_Grid.GetCellCenterWorld(cell);
            
            worldCell.y = 0.0f;

            switch (m_Tool)
            {
                case ToolType.Ground:
                case ToolType.CellObject:
                    PlaceGround(cell, worldCell);
                    break;
                case ToolType.Wall:
                case ToolType.EdgeObject:
                    PlaceWall(cell, point, worldCell);
                    break;
                case ToolType.Spawn:
                    PlaceSpawnPoint(cell, worldCell);
                    break;
                case ToolType.End:
                    PlaceEndPoint(cell, worldCell);
                    break;
            }
        }
        
        view.Repaint();
    }

    void PlaceGround(Vector3Int cell, Vector3 worldCell)
    {
        Level.Cell c = m_Level.GetCell(cell);
        
        var color = c == null ? Color.green : Color.red;

        if (m_Tool == ToolType.Ground || c != null)
        {
            Handles.DrawSolidRectangleWithOutline(new Vector3[]
            {
                worldCell - new Vector3(-0.5f, 0.0f, -0.5f),
                worldCell - new Vector3(0.5f, 0.0f, -0.5f),
                worldCell - new Vector3(0.5f, 0.0f, 0.5f),
                worldCell - new Vector3(-0.5f, 0.0f, 0.5f),
            }, new Color(0,0,0,0), color);
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (m_Tool == ToolType.Ground)
            {
                m_GroundFirstClickType = c == null;

                //true mean we didn't have a cell there so create one, false mean we have a cell, remove it
                if (m_GroundFirstClickType)
                {
                    m_Level.GetOrCreateCell(cell);
                }
                else
                {
                    m_Level.RemoveCell(cell);
                }

                m_LastCell = cell;
            }
            else if(c != null)
            {//placing cell object
                m_Level.SetCellObjectEditor(cell, m_SelectedCellObject);
            }
        }
        else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && m_Tool != ToolType.CellObject
        ) //cell object can't be dragged placed, so ignore that
        {
            if (m_LastCell != cell)
            {
                //true mean our first action was creating a cell, so during that drag only create cell if none exist
                if (m_GroundFirstClickType)
                {
                    m_Level.GetOrCreateCell(cell);
                }
                else //false mean we deleted a cell as 1st action so continue doing only so
                {
                    m_Level.RemoveCell(cell);
                }

                m_LastCell = cell;
            }
        }
        else if (m_Tool == ToolType.Wall && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
        {
            m_Level.CycleGroundPrefab(cell);
        }
    }
    
    void PlaceWall(Vector3Int cellIdx, Vector3 point, Vector3 worldCell)
    {
        var cell = m_Level.GetCell(cellIdx);
        if(cell == null)
            return;
        
        Level.EdgeDirection edgePosition = Level.EdgeDirection.Up;
        var dir = point - worldCell;
            
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            if (dir.x > 0)
                edgePosition = Level.EdgeDirection.Right;
            else
                edgePosition = Level.EdgeDirection.Left;
        }
        else
        {
            if (dir.z > 0)
                edgePosition = Level.EdgeDirection.Up;
            else
                edgePosition = Level.EdgeDirection.Down;
        }

        if (m_Tool == ToolType.EdgeObject && m_SelectedEdgeObject != null)
        {
            //if we are placing edge object and this edge is not passable or have an object, early exit we can't place it here
            var edge = m_Level.Edges[cell.Edges[(int) edgePosition]];
            if (!edge.Passable || edge.EdgeObject != null)
                return;
        }
            
        DrawBorder(edgePosition, worldCell);
            
        if(Event.current.type == EventType.MouseUp && Event.current.button == 0) 
        {
            if(m_Tool == ToolType.Wall)
                m_Level.SwitchEdgePassable(cellIdx, edgePosition);
            else
            {
                m_Level.SetEdgeObjectEditor(cellIdx, edgePosition, m_SelectedEdgeObject);
            }
        }
        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
        {
            if (m_Tool == ToolType.Wall)
            {
                m_Level.CycleEdgeWallPrefab(cellIdx, edgePosition);
            }
        }
    }

    void DrawBorder(Level.EdgeDirection border, Vector3 cellPosition)
    {
        Vector3 pos = new Vector3(), 
            size = new Vector3();
        
        switch (border)
        {
            case Level.EdgeDirection.Up :
                pos = cellPosition + Vector3.forward * 0.5f;
                size = new Vector3(1.0f, 0.4f, 0.2f);
                break;
            case Level.EdgeDirection.Right :
                pos = cellPosition + Vector3.right * 0.5f;
                size = new Vector3(0.2f, 0.4f, 1.0f);
                break;
            case Level.EdgeDirection.Down :
                pos = cellPosition + Vector3.back * 0.5f;
                size = new Vector3(1.0f, 0.4f, 0.2f);
                break;
            case Level.EdgeDirection.Left :
                pos = cellPosition + Vector3.left * 0.5f;
                size = new Vector3(0.2f, 0.4f, 1.0f);
                break;
        }

        Handles.DrawWireCube(pos, size);
    }

    void PlaceSpawnPoint(Vector3Int cell, Vector3 worldPos)
    {
        Handles.DrawWireCube(worldPos - Vector3.up * 0.2f, Vector3.one * 0.6f);
        
        if(Event.current.type == EventType.MouseUp && Event.current.button == 0) 
        {
            m_Level.SetSpawn(cell); 
        }
    }
    
    void PlaceEndPoint(Vector3Int cell, Vector3 worldPos)
    {
        Handles.DrawWireCube(worldPos - Vector3.up * 0.2f, Vector3.one * 0.6f);
        
        if(Event.current.type == EventType.MouseUp && Event.current.button == 0) 
        {
            m_Level.SetEnd(cell); 
        }
    }
}
