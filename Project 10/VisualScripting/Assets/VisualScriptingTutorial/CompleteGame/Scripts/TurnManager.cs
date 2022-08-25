using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VisualScriptingTutorial
{
    public class TurnManager : MonoBehaviour
    {
        public bool IsInTurn => m_SinceLastTurn > 0.0f;
        
        //constant length of a turn (all animation are based on that)
        public static readonly float TurnTime = 0.35f;
        
        private List<TurnReceiver> m_TurnReceivers = new List<TurnReceiver>();

        private float m_SinceLastTurn = 0.0f;

        public void RegisterTurnReceiver(TurnReceiver receiver)
        {
            m_TurnReceivers.Add(receiver);
        }

        public void UnregisterTurnReceiver(TurnReceiver receiver)
        {
            m_TurnReceivers.Remove(receiver);
        }

        private void Update()
        {
            if (m_SinceLastTurn > 0.0f)
                m_SinceLastTurn -= Time.deltaTime;
        }

        public void TakeTurn()
        {
            foreach (var turnReceiver in m_TurnReceivers)
            {
                turnReceiver.TakeTurn();
            }

            m_SinceLastTurn = TurnTime;
        }

        public void Undo()
        {
            foreach (var turnReceiver in m_TurnReceivers)
            {
                turnReceiver.Undo();
            }
        }
    }

    public abstract class TurnReceiver : MonoBehaviour
    {
        public abstract void Undo();

        public virtual void TakeTurn()
        {
            EventBus.Trigger(TurnEvent.EventHook, gameObject);
        }
    }
}