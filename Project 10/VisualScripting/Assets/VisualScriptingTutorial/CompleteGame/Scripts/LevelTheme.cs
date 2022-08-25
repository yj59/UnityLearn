using UnityEngine;

namespace VisualScriptingTutorial
{
    [CreateAssetMenu(fileName = "Theme.asset", menuName = "VSTutorial/LevelTheme")]
    public class LevelTheme : ScriptableObject
    {
        public GameObject[] GroundPrefab;
        public GameObject[] WallPrefab;
        public GameObject[] InsideWallPrefab;
    }
}