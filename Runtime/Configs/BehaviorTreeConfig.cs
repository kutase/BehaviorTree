using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Configs
{
    [CreateAssetMenu(fileName = "BehaviorTreePreferences", menuName = "Behavior Tree/Preferences")]
    public class BehaviorTreeConfig : ScriptableObject
    {
        [Space()] public Texture2D gridTexture;
        public Texture2D nodeGradient;
        public Texture2D failureSymbol;
        public Texture2D successSymbol;
        public Texture2D runningSymbol;

        [Header("Content Colors")]
        public Color defaultNodeBackgroundColor;
        public Color headerColor = Color.black;
        public Color descriptionColor = Color.black;
        public Color treeNameColor = Color.white;

        [Header("Runtime Colors")] 
        public Color runningColor;
        public Color successColor = new Color(0.1f, 1f, 0.54f, 0.25f);
        public Color failureColor = new Color(1f, 0.1f, 0.1f, 0.25f);

        private static BehaviorTreeConfig instance = null;

        public static BehaviorTreeConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadDefaultPreferences();
                }

                return instance;
            }

            set { instance = value; }
        }

        public static BehaviorTreeConfig LoadDefaultPreferences()
        {
            var prefs = Resources.Load<BehaviorTreeConfig>("DefaultBehaviorTreeConfig");

            if (prefs == null)
            {
                Debug.LogWarning("Failed to load DefaultBonsaiPreferences");
                // Empty preferences. Editor will not render nodes correctly.
                prefs = CreateInstance<BehaviorTreeConfig>();
            }

            return prefs;
        }

        public static Texture2D Texture(string name)
        {
            return Resources.Load<Texture2D>(name);
        }
    }
}