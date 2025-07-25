namespace Plugins.BehaviorTree.Runtime.Nodes.Composites
{
    // Sequence: AND behavior
    public class Sequence : Composite
    {
        private int currentIndex;

        public Sequence(params Node[] nodes) : base(nodes)
        {
        }

        protected override void OnEnter()
        {
            currentIndex = 0;
        }

        protected override NodeState Run()
        {
            while (currentIndex < children.Count)
            {
                var result = children[currentIndex].Tick();
                if (result == NodeState.Running)
                    return NodeState.Running;
                if (result == NodeState.Failure)
                    return NodeState.Failure;
                currentIndex++;
            }

            return NodeState.Success;
        }
    }
}