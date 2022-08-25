using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VisualScriptingTutorial
{
    /// <summary>
    /// Moving object are all object that can move from cell to cell (player, pushable box, enemies...)
    /// This script will take care of triggering the relevant events on the Script graph on objects in the cells and
    /// edges when a move is requested. It also have functions to query if the move was successful which can be useful
    /// in script (e.g. pushable box will cancel the movement of whatever is pushing them if their move was canceled,
    /// meaning that couldn't be pushed)
    /// </summary>
    public class MovingObject : TurnReceiver
    {
        public enum State
        {
            Idle,
            Moving
        }

        private readonly float Speed = 1.0f / TurnManager.TurnTime;

        public State CurrentState => m_CurrentState;
        //use Vector2 as easier to handle in Visual Scripting than Vector3Int
        public Vector2 CurrentCell => new Vector2(m_CurrentCell.x, m_CurrentCell.z);
        public Vector3 TargetPosition => m_TargetPosition;
    
        public Vector3Int IntCurrentCell => m_CurrentCell;
    
        private Vector3Int m_CurrentCell;
        private Vector3Int m_TargetCell;

        private Vector3 m_TargetPosition;
        private float m_DistanceToMove;

        private State m_CurrentState;

        private bool m_MovementTemp;
        private bool m_EdgeEventTriggered;
        
        protected Stack<Vector3Int> m_History = new Stack<Vector3Int>();
    
        private void Start()
        {
            m_CurrentState = State.Idle;

            if(Level.Instance == null)
                return;
            
            var cell = Level.Instance.Grid.WorldToCell(transform.position);
            TeleportTo(new Vector2(cell.x, cell.z));

            Level.Instance.GetCell(m_CurrentCell).ContainedMovingObjects.Add(this);
        }

        private void OnEnable()
        {
            EntryPoint.Instance.TurnManager.RegisterTurnReceiver(this);
        }

        private void OnDisable()
        {
            EntryPoint.Instance.TurnManager.UnregisterTurnReceiver(this);
        }

        private void OnDestroy()
        {
            var cell = Level.Instance.GetCell(m_CurrentCell);
            cell.ContainedMovingObjects.Remove(this);
        }

        void Update()
        {
            switch (m_CurrentState)
            {
                case State.Moving:
                    MoveUpdate();
                    break;
            }
        }
    
    
        public void CancelMovement()
        {
            m_MovementTemp = false;
        }

        public bool MovementStillValid()
        {
            return m_MovementTemp;
        }

        //To simplify using it through visual scripting, we give a Vector2 we manually round to a Vector3Int (as Vector3Int
        //don't have native support in Visual Scripting)
        public void Move(Vector2 targetPosition)
        {
            Vector3Int targetPositionInt = new Vector3Int(Mathf.RoundToInt(targetPosition.x), 0, Mathf.RoundToInt(targetPosition.y));

            m_MovementTemp = false;
            var edge = Level.Instance.GetEdge(m_CurrentCell, targetPositionInt);

            //if we have a passable edge in that directions
            if (edge != null && edge.Passable)
            {
                m_MovementTemp = true;

                if (edge.EdgeObject != null)
                {//if we have an edge object, we call the want to Cross Edge event so the edge object can cancel the movement
                    EventBus.Trigger(WantCrossEdgeEvent.EventHook, edge.EdgeObject.gameObject, this);
                }
            
                if (m_MovementTemp)
                {//if the movement wasn't canceled by the Edge Object
                    var currentCell = Level.Instance.GetCell(m_CurrentCell);
                    if (currentCell.Object != null)
                    {//if we have an object in the current cell, we offer a possibility to it to cancel the movement too
                        EventBus.Trigger(WantToExitCell.EventHook, currentCell.Object.gameObject, this);
                    }

                    var nextCell = Level.Instance.GetCell(targetPositionInt);
                    if (nextCell.Object != null)
                    {//if we have an object in the target 
                        EventBus.Trigger(WantToEnterCell.EventHook, nextCell.Object.gameObject, this);
                    }
                    
                    if (m_MovementTemp && nextCell.ContainedMovingObjects.Count > 0)
                    {
                        //we copy as the WantToEnterCell event could modify that list
                        var objects = new List<MovingObject>(nextCell.ContainedMovingObjects);
                        foreach (var movingObject in objects)
                        {
                            EventBus.Trigger(WantToEnterCell.EventHook, movingObject.gameObject, this);
                        }
                    }

                    if (m_MovementTemp)
                    {//if neither the edge nor the objects on the current or next cell cancel the movement, we start the movement
                    
                        Level.Instance.GetCell(m_CurrentCell).ContainedMovingObjects.Remove(this);
                        m_TargetCell = targetPositionInt;
                        m_TargetPosition = Level.Instance.Grid.GetCellCenterWorld(m_TargetCell);
                        m_CurrentState = State.Moving;
                        m_DistanceToMove = (Level.Instance.Grid.GetCellCenterWorld(m_CurrentCell) - m_TargetPosition).magnitude;

                        m_EdgeEventTriggered = false;

                        if(currentCell.Object != null)
                            EventBus.Trigger(ExitCell.EventHook, currentCell.Object.gameObject, this);
                    }
                }
            }
        }

        void MoveUpdate()
        {
            Vector3 pos = transform.position;
            pos = Vector3.MoveTowards(pos, m_TargetPosition, Speed * Time.smoothDeltaTime);
        
            float remainingDistance = (m_TargetPosition - pos).sqrMagnitude / m_DistanceToMove;
            if (remainingDistance < 0.5f)
            {
                if (!m_EdgeEventTriggered)
                {
                    var edge = Level.Instance.GetEdge(m_CurrentCell, m_TargetCell);

                    if (edge.EdgeObject != null)
                        EventBus.Trigger(CrossEdgeEvent.EventHook, edge.EdgeObject.gameObject, this);

                    m_EdgeEventTriggered = true;
                }
            }
            else if (remainingDistance < 0.001f)
            {
                var targetCell = Level.Instance.GetCell(m_TargetCell);
            }

            if (Vector3.SqrMagnitude(pos - m_TargetPosition) < 0.001f)
            {
                pos = m_TargetPosition;
                var targetCell = Level.Instance.GetCell(m_TargetCell);
                if(targetCell.Object != null)
                    EventBus.Trigger(EnterCell.EventHook, targetCell.Object.gameObject, this);
            
                m_CurrentCell = m_TargetCell;
                Level.Instance.GetCell(m_CurrentCell).ContainedMovingObjects.Add(this);

                m_CurrentState = State.Idle;
            }

            transform.position = pos;
        }

        //The Vector2 is rounded to a vector3int, make it easier to use in visual scripting
        public void TeleportTo(Vector2 cell)
        {
            m_CurrentCell = new Vector3Int(Mathf.RoundToInt(cell.x), 0, Mathf.RoundToInt(cell.y));
            var pts = Level.Instance.Grid.GetCellCenterWorld(m_CurrentCell);
            transform.position = pts;
        }

        public override void TakeTurn()
        {
            m_History.Push(m_CurrentCell);
            base.TakeTurn();
        }

        public override void Undo()
        {
            if(m_History.Count == 0)
                return;
            
            var prev = m_History.Pop();
            
            //no need to make anything, we were in the same cell the previous turn
            if(prev == m_CurrentCell)
                return;

            var currentCell = Level.Instance.GetCell(m_CurrentCell);
            if(currentCell.Object != null)
                EventBus.Trigger(ExitCell.EventHook, currentCell.Object.gameObject, this);

            currentCell.ContainedMovingObjects.Remove(this);
            
            var oldCell = Level.Instance.GetCell(prev);
            if(oldCell.Object != null)
                EventBus.Trigger(EnterCell.EventHook, oldCell.Object.gameObject, this);
            
            oldCell.ContainedMovingObjects.Add(this);
            
            TeleportTo(new Vector2(prev.x, prev.z));
        }
    }
}