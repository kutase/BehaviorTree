using System.Collections.Generic;

namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    // Decorator wraps one child node
    public abstract class Decorator : Node
    {
        protected Node child;

        public Node Child => child;

        public Decorator(Node child)
        {
            this.child = child;
        }

        protected override NodeState Run()
        {
            return child.Tick();
        }
    }
}