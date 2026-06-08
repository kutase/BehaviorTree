#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Editor
{
    public static class BehaviorTreeEditorSelection
    {
        public static bool TryGetRunnerFromSelection(out BehaviorTreeRunner runner)
        {
            runner = null;
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject == null)
            {
                return false;
            }

            var root = GetWithTag(activeGameObject.transform, "NPC");

            if (!root.CompareTag("NPC"))
            {
                return false;
            }

            runner = root.GetComponentInChildren<BehaviorTreeRunner>(true);
            return runner != null;
        }

        private static Transform GetWithTag(Transform transform, string tag = "")
        {
            return string.IsNullOrEmpty(tag)
                ? transform
                : transform.CompareTag(tag) ? transform : transform.parent == null ? transform : GetWithTag(transform.parent, tag);
        }
    }
}
#endif
