using System.Collections.Generic;

namespace Plugins.Behavior_Tree.Runtime.New.Nodes.Composites
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

        public override void CollectNodes(List<Node> nodes)
        {
            base.CollectNodes(nodes);
            foreach (var c in children) c.CollectNodes(nodes);
        }

        public override void CollectEdges(List<(Node parent, Node child)> edges)
        {
            foreach (var c in children)
            {
                edges.Add((this, c));
                c.CollectEdges(edges);
            }
        }
    }
}