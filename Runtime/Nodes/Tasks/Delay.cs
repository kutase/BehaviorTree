using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Nodes.Tasks
{
    // Example Tasks with OnEnter/OnExit
    public class Delay : Node
    {
        private readonly float duration;
        private float elapsed;

        public override string Description => $"Wait: {Mathf.Max(duration - elapsed, 0f):0.0}s";

        public Delay(float seconds)
        {
            duration = seconds;
        }

        protected override void OnEnter()
        {
            elapsed = 0f;
        }

        protected override NodeState Run()
        {
            elapsed += Time.deltaTime;

            // updating description
            #if UNITY_EDITOR
            UpdateEditorGui();
            #endif

            return elapsed >= duration ? NodeState.Success : NodeState.Running;
        }
    }
}