namespace Plugins.BehaviorTree.Runtime.Nodes.Tasks
{
    // Leaf nodes: actual actions or conditions
    public abstract class Task : Node
    {
        protected override NodeState ExecuteNode()
        {
            return Run();
        }

        protected abstract NodeState Run();
    }
}