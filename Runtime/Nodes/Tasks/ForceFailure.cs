namespace Plugins.Behavior_Tree.Runtime.New.Nodes.Tasks
{
    public class ForceFailure : Task
    {
        protected override NodeState Run()
        {
            return NodeState.Failure;
        }
    }
}