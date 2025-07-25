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

        protected override NodeState ExecuteNode()
        {
            return child.Tick();
        }

        public override void CollectNodes(List<Node> nodes)
        {
            base.CollectNodes(nodes);
            child.CollectNodes(nodes);
        }

        public override void CollectEdges(List<(Node parent, Node child)> edges)
        {
            edges.Add((this, child));
            child.CollectEdges(edges);
        }
    }
}