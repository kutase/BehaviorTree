using System;
using System.Collections.Generic;

namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    // Example decorator: IfElse
    public class IfElse : Decorator
    {
        private readonly Func<bool> condition;
        private readonly Node elseNode;
        private readonly Node ifNode;

        public IfElse(Func<bool> condition, Node ifNode, Node elseNode)
            : base(condition() ? ifNode : elseNode)
        {
            this.condition = condition;
            this.ifNode = ifNode;
            this.elseNode = elseNode;
        }

        protected override NodeState ExecuteNode()
        {
            // Switch child based on condition
            child = condition() ? ifNode : elseNode;
            return base.ExecuteNode();
        }

        public override void CollectNodes(List<Node> nodes)
        {
            base.CollectNodes(nodes);
            ifNode.CollectNodes(nodes);
            elseNode.CollectNodes(nodes);
        }

        public override void CollectEdges(List<(Node parent, Node child)> edges)
        {
            edges.Add((this, ifNode));
            edges.Add((this, elseNode));
            ifNode.CollectEdges(edges);
            elseNode.CollectEdges(edges);
        }
    }
}