namespace Plugins.BehaviorTree.Runtime.Nodes.Tasks
{
    public class ForceSuccess : Node
    {
        protected override NodeState Run()
        {
            return NodeState.Success;
        }
    }
}