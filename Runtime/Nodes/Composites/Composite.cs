using System.Collections.Generic;

namespace Plugins.BehaviorTree.Runtime.Nodes.Composites
{
    // Composite nodes have multiple children
    public abstract class Composite : Node
    {
        protected readonly List<Node> children = new();

        public List<Node> Children => children;

        public Composite(IEnumerable<Node> nodes)
        {
            children.AddRange(nodes);
        }
    }
}