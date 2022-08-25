using UnityEngine;
using UnityEngine.VFX;

namespace VisualScriptingTutorial
{
    public class EntryPoint : MonoBehaviour
    {
        private static EntryPoint s_Instance;

        [SerializeField] private PlayerControl PlayerPrefab;
        [SerializeField] private TurnManager TurnManagerPrefab;
        [SerializeField] private VisualEffect VictoryPrefab;
        [SerializeField] private UIHandler UIPrefab;
        
        public PlayerControl PlayerInstance => m_PlayerInstance;
        public TurnManager TurnManager => m_TurnManager;
        public VisualEffect VictoryVFX => m_VictoryVFX;
        public UIHandler UI => m_UI;
        
        private PlayerControl m_PlayerInstance;
        private TurnManager m_TurnManager;
        private VisualEffect m_VictoryVFX;
        private UIHandler m_UI;

        public static EntryPoint Instance
        {
            get
            {
#if UNITY_EDITOR
                //we only pay the cost of testing if the entry point exist in the editor where we can start from anywhere
                //In a build, the EntryPoint is created by the starting script Loading.cs in the Loading scene.
                if (s_Instance == null)
                {
                    Create();
                    Init();
                }
#endif

                return s_Instance;
            }
        }

        public static void Create()
        {
            s_Instance = Resources.Load<EntryPoint>("EntryPoint");
        }

        public static void Init()
        {
            s_Instance.m_TurnManager = Instantiate(s_Instance.TurnManagerPrefab);
            DontDestroyOnLoad(s_Instance.m_TurnManager.gameObject);

            //If a player was already placed in the scene when this is initialized, we don't need to create a new one
            var player = FindObjectOfType<PlayerControl>();
            if (player != null)
            {
                s_Instance.m_PlayerInstance = player;
            }
            else
            {
                s_Instance.m_PlayerInstance = Instantiate(s_Instance.PlayerPrefab);
                DontDestroyOnLoad(s_Instance.m_PlayerInstance.gameObject);
                s_Instance.m_PlayerInstance.gameObject.SetActive(false);
            }

            s_Instance.m_VictoryVFX = Instantiate(s_Instance.VictoryPrefab);
            DontDestroyOnLoad(s_Instance.m_VictoryVFX);
            s_Instance.m_VictoryVFX.gameObject.SetActive(false);

            s_Instance.m_UI = Instantiate(s_Instance.UIPrefab);
            DontDestroyOnLoad(s_Instance.m_UI);
        }
    }
}