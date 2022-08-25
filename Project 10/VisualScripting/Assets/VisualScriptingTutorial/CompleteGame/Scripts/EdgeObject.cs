using UnityEngine;

namespace VisualScriptingTutorial
{
    public class EdgeObject : TurnReceiver
    {
        public int Owner => m_EdgeOwner;
    
        [HideInInspector]
        [SerializeField]
        protected int m_EdgeOwner;

        public void SetOwner(int edge)
        {
            m_EdgeOwner = edge;
        }
        private void OnEnable()
        {
            EntryPoint.Instance.TurnManager.RegisterTurnReceiver(this);
        }

        private void OnDisable()
        {
            EntryPoint.Instance.TurnManager.UnregisterTurnReceiver(this);
        }

        public override void Undo()
        {
            
        }
    }
}