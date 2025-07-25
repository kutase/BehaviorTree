namespace Plugins.Behavior_Tree.Runtime.New.Nodes
{
    // Possible return states for all nodes
    public enum NodeState
    {
        NotActive = 0,
        Running = 1,
        Success = 2,
        Failure = 3
    }
}