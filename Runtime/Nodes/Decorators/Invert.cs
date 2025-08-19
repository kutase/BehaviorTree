namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    // Decorator that inverts the result of its child node
    // Success becomes Failure, Failure becomes Success, Running remains Running
    public class Invert : Decorator
    {
        public Invert(Node child)
        {
            this.child = child;
        }

        protected override NodeState Run()
        {
            var childState = child.Tick();
            
            // Invert Success and Failure, keep Running as is
            return childState switch
            {
                NodeState.Success => NodeState.Failure,
                NodeState.Failure => NodeState.Success,
                _ => childState // Running and NotActive remain unchanged
            };
        }
    }
} 