using UnityEngine;

namespace VisualScriptingTutorial
{
    // This will place the camera so it never look too much outside the play area, to maximize the amount of the 
    // play area visible at any time
    [RequireComponent(typeof(Camera))]
    public class CameraHandler : MonoBehaviour
    {
        private static CameraHandler s_Instance = null;
        public static CameraHandler Instance => s_Instance;

        public int CellLimit;
        public float XRotation;
        public float Distance;
        public float Speed = 5.0f;

        public bool SnapToLevel = true;
        
        private MovingObject m_Target;
        private Vector3Int m_CurrentCell;

        private Camera m_Camera;
        
        private Vector3[] m_GroundCorners = new Vector3[4];

        private void Awake()
        {
            s_Instance = this;
            m_Camera = GetComponent<Camera>();
        }

        private void OnDestroy()
        {
            if (s_Instance == this) s_Instance = null;
        }

        public void SetupCamera(MovingObject target)
        {
            m_Target = target;
            FindCurrentCell();

            transform.forward = Quaternion.Euler(XRotation, 0, 0) * Vector3.forward;
            transform.position = PlaceForWorldPosition(Level.Instance.Grid.GetCellCenterWorld(m_CurrentCell));

            Level.Instance.CameraMoved(transform.position);
        }

        //Check if we moved further than the define limit. This allow to avoid moving the camera with every move of 
        //the player which could be disorienting.
        void FindCurrentCell()
        {
            var diff = m_Target.IntCurrentCell - m_CurrentCell;

            if (Mathf.Abs(diff.x) > CellLimit)
                m_CurrentCell.x = m_CurrentCell.x + (diff.x < -1.0 ? -CellLimit : CellLimit);

            if (Mathf.Abs(diff.z) > CellLimit)
                m_CurrentCell.z = m_CurrentCell.z + (diff.z < -1.0 ? -CellLimit : CellLimit);
        }

        //Raycast each corner of the camera against the play plane to find the extends viewed from the camera
        void FindGroundPlaceCorner()
        {
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            Vector3[] viewpoints = 
            {
                new Vector3(0,0,0), 
                new Vector3(0,1,0),
                new Vector3(1,1,0),
                new Vector3(1,0,0)
            };
            
            for (int i = 0; i < 4; ++i)
            {
                var ray = m_Camera.ViewportPointToRay(viewpoints[i]);
                float d = 0.0f;
                if (ground.Raycast(ray, out d))
                {
                    m_GroundCorners[i] = ray.GetPoint(d);
                }
                else
                {
                    Debug.LogError("Camera is wrongly setup, it can see over the horizon");
                }
            }
        }

        private void Update()
        {
            if(m_Target == null)
                return;

            FindCurrentCell();

            Vector3 targetPos = PlaceForWorldPosition(Level.Instance.Grid.GetCellCenterWorld(m_CurrentCell));
            
            if(SnapToLevel)
                ClampTargetPosToLevelBorder(ref targetPos);
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Speed * Time.smoothDeltaTime);

            Level.Instance.CameraMoved(transform.position);
        }
        
        void ClampTargetPosToLevelBorder(ref Vector3 targetPos)
        {
            var t = transform;
            
            //we need to move the camera to the new clamp position for the raycast to be valid
            //so we save the position so we can revert at the end
            var startingPos = t.position;
            var cells = Level.Instance.Cells;
            var grid = Level.Instance.Grid;
            
            //build a linear list of all cell centers. This will be faster to fo through multiple time than the dictionnary
            Vector3[] cellsCenter = new Vector3[cells.Count];
            int idx = 0;
            foreach (var pair in cells)
            {
                cellsCenter[idx] = grid.GetCellCenterWorld(pair.Key);
                idx++;
            }
            
            //check if top of camera is seeing higher than the highest corner in it's view
            t.position = targetPos;
            FindGroundPlaceCorner();
            float highest = float.MinValue;
            //go over all cells and find if our view side (here top) is seeing above the highest cell in its viewport.
            foreach (var cell in cellsCenter)
            {
                if (cell.x >= m_GroundCorners[1].x && cell.x <= m_GroundCorners[2].x && cell.z > highest)
                    highest = cell.z;
            }

            if (highest < m_GroundCorners[1].z)
                targetPos = targetPos + new Vector3(0, 0, highest - m_GroundCorners[1].z + 0.7f);
            
            //check right
            t.position = targetPos;
            FindGroundPlaceCorner();
            highest = float.MinValue;
            foreach (var cell in cellsCenter)
            {
                if (cell.z >= m_GroundCorners[3].z && cell.z <= m_GroundCorners[2].z && cell.x > highest)
                    highest = cell.x;
            }
            
            if (highest < m_GroundCorners[3].x)
                targetPos = targetPos + new Vector3(highest - m_GroundCorners[3].x + 0.7f, 0 ,0 );
            
            //check bottom
            t.position = targetPos;
            FindGroundPlaceCorner(); 
            highest = float.MaxValue;
            foreach (var cell in cellsCenter)
            {
                if (cell.x >= m_GroundCorners[0].x && cell.x <= m_GroundCorners[3].x && cell.z < highest)
                    highest = cell.z;
            }

            if (highest > m_GroundCorners[0].z)
                targetPos = targetPos + new Vector3(0, 0, highest - m_GroundCorners[0].z - 0.7f);
            
            //check left
            t.position = targetPos;
            FindGroundPlaceCorner();
            highest = float.MaxValue;
            foreach (var cell in cellsCenter)
            {
                if (cell.z >= m_GroundCorners[0].z && cell.z <= m_GroundCorners[1].z && cell.x < highest)
                    highest = cell.x;
            }

            if (highest > m_GroundCorners[0].x)
                targetPos = targetPos + new Vector3(highest - m_GroundCorners[0].x - 0.7f, 0 ,0 );
            
            
            //replace camera where it was
            t.position = startingPos;
        }

        Vector3 PlaceForWorldPosition(Vector3 worldPos)
        {
            return worldPos - transform.forward * Distance;
        }
    }
}
