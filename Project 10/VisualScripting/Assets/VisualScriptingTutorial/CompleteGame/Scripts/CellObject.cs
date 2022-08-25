namespace VisualScriptingTutorial
{
    public class CellObject : TurnReceiver
    {
        public bool AddGroundMesh = true;
    
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