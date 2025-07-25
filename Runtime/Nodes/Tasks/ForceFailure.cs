namespace Plugins.BehaviorTree.Runtime.Nodes.Tasks
{
    public class ForceFailure : Node
    {
        protected override NodeState Run()
        {
            return NodeState.Failure;
        }
    }
}