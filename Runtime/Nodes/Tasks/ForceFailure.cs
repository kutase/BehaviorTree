namespace Plugins.BehaviorTree.Runtime.Nodes.Tasks
{
    public class ForceFailure : Task
    {
        protected override NodeState Run()
        {
            return NodeState.Failure;
        }
    }
}