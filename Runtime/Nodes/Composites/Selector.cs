namespace Plugins.BehaviorTree.Runtime.Nodes.Composites
{
    // Selector: OR behavior
    public class Selector : Composite
    {
        private int currentIndex;

        public Selector(params Node[] nodes) : base(nodes)
        {
        }

        protected override void OnEnter()
        {
            currentIndex = 0;
        }

        protected override NodeState ExecuteNode()
        {
            while (currentIndex < children.Count)
            {
                var result = children[currentIndex].Tick();
                if (result == NodeState.Running)
                    return NodeState.Running;
                if (result == NodeState.Success)
                    return NodeState.Success;
                currentIndex++;
            }

            return NodeState.Failure;
        }
    }
}