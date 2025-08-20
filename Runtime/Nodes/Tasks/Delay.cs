using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Nodes.Tasks
{
    public class Delay : Node
    {
        private readonly float duration;
        private float elapsed;

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

            Description = $"Wait: {Mathf.Max(duration - elapsed, 0f):0.0}s";

            return elapsed >= duration ? NodeState.Success : NodeState.Running;
        }
    }
}