using UnityEngine;

namespace Plugins.Behavior_Tree.Runtime.New.Nodes.Tasks
{
    public class MoveToTarget : Task
    {
        private readonly float speed;
        private readonly Vector3 target;
        private readonly Transform transform;

        public MoveToTarget(Transform t, Vector3 target, float speed)
        {
            transform = t;
            this.target = target;
            this.speed = speed;
        }

        protected override void OnEnter()
        {
            // Could initialize pathfinding
        }

        protected override NodeState Run()
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            return Vector3.Distance(transform.position, target) < 0.1f
                ? NodeState.Success
                : NodeState.Running;
        }

        protected override void OnExit()
        {
            // Stop movement or animations
        }
    }
}