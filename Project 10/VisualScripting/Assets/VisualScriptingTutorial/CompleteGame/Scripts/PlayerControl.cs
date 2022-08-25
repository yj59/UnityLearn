using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VisualScriptingTutorial
{
    public class PlayerControl : MonoBehaviour
    { 
        private Animator m_Animator;
        private MovingObject m_MovingObject;
        private bool m_WaitingForInput;
        private bool m_WonState;
        private bool m_Knockout;

        void Awake()
        {
            m_MovingObject = GetComponent<MovingObject>();
            m_WaitingForInput = true;

            m_Animator = GetComponentInChildren<Animator>();
        }

        void Restart()
        {
            EntryPoint.Instance.UI.ResetUI();
            m_WonState = false;
            m_Knockout = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Update()
        {   
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (m_WaitingForInput)
            {
                if (m_WonState)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        Restart();
                    }
                }
                else if (m_Knockout)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        Restart();
                    }
                }
                else
                {
                    if (input.sqrMagnitude > 0.01f)
                    {
                        m_WaitingForInput = false;
                        Vector2 targetPos = m_MovingObject.CurrentCell;
                        if (Math.Abs(input.x) > Mathf.Abs(input.y))
                        {
                            if (input.x > 0)
                            {
                                targetPos.x++;
                            }
                            else
                            {
                                targetPos.x--;
                            }
                        }
                        else
                        {
                            if (input.y > 0)
                            {
                                targetPos.y++;
                            }
                            else
                            {
                                targetPos.y--;
                            }
                        }

                        m_MovingObject.Move(targetPos);

                        if (m_MovingObject.CurrentState == MovingObject.State.Moving)
                        {
                            //if we are moving, the move wasn't canceled, so we take a turn in the game
                            m_Animator.SetTrigger("Move");

                            transform.forward = Vector3.Normalize(m_MovingObject.TargetPosition - transform.position);

                            EntryPoint.Instance.TurnManager.TakeTurn();
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Space))
                    {
                        m_WaitingForInput = false;
                        EntryPoint.Instance.TurnManager.TakeTurn();
                    }
                    else if (Input.GetKeyDown(KeyCode.R))
                    {
                        EntryPoint.Instance.TurnManager.Undo();
                    }
                }
            }
            else if(!EntryPoint.Instance.TurnManager.IsInTurn)
            {
                EndOfTurnCheck();
                
                if (m_MovingObject.CurrentState == MovingObject.State.Idle && input.sqrMagnitude < 0.01f)
                {
                    if (m_MovingObject.IntCurrentCell == Level.Instance.EndPoint)
                    {//finished, reload the scene
                        m_WonState = true;
                        
                        EntryPoint.Instance.UI.WinPanel.SetActive(true);
                        var vfx = EntryPoint.Instance.VictoryVFX;
                        vfx.transform.position = transform.position;
                        vfx.gameObject.SetActive(true);
                        vfx.Play();
                    }
        
                    m_WaitingForInput = true;
                }
            }
        }

        public void EndOfTurnCheck()
        {
            //we notify all other movingObject & object on that cell that the player entered it
            var cell = Level.Instance.GetCell(m_MovingObject.IntCurrentCell);
            if(cell.Object != null)
                EventBus.Trigger(PlayerEnteredCellUnit.EventHook, cell.Object.gameObject, this);
            foreach (var movingObject in cell.ContainedMovingObjects)
            {
                if(movingObject.gameObject == gameObject)
                    continue;
                EventBus.Trigger(PlayerEnteredCellUnit.EventHook, movingObject.gameObject, this);
            }
        }

        public void KnockOut()
        {
            EntryPoint.Instance.UI.KOPanel.SetActive(true);
            m_Animator.SetTrigger("Knockout");
            m_Knockout = true;
        }

        public void TeleportTo(Vector2 cell)
        {
            m_MovingObject.TeleportTo(cell);
            CameraHandler.Instance.SetupCamera(m_MovingObject);
        }
    }
}