using UnityEngine;

namespace VisualScriptingTutorial
{
    /// <summary>
    /// This will switch between a normal version of a high wall (HighWall) and a shorter or transparent version
    /// (LowWall) based on viewDir to allow to see behind wall. This is placed on every wall prefab, the Level hold
    /// a list of all wall (built at edit time, no runtime cost) and this is called to update which wall is displayed by
    /// the camera code.
    /// </summary>
    public class InteractiveCutWall : MonoBehaviour
    {
        public GameObject HighWall;
        public GameObject LowWall;

        public Transform DecorationRoot;
    
        [HideInInspector] public bool IsInternalWall;
    
        public void UpdateWall(Vector3 viewDir)
        {
            float dot = Vector3.Dot(viewDir, transform.up);
        
            Debug.DrawRay(transform.position, viewDir, Color.red);
            Debug.DrawRay(transform.position, transform.up, Color.green);

            if (dot < 0.95f && (IsInternalWall || transform.forward == -Vector3.forward))
            {
                LowWall.SetActive(true);
                HighWall.SetActive(false);
            }
            else
            {
                LowWall.SetActive(false);
                HighWall.SetActive(true);
            }
        }
    }
}