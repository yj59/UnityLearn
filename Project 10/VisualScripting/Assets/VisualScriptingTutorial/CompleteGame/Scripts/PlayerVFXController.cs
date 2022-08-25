using UnityEngine;
using UnityEngine.VFX;

namespace VisualScriptingTutorial
{
    public class PlayerVFXController : MonoBehaviour
    {
        public VisualEffect smokePuff;

        public void PlaySmokePuff()
        {
            smokePuff.Play();
        }
    }
}