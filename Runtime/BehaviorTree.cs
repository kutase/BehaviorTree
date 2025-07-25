using System.Collections.Generic;
using Plugins.BehaviorTree.Runtime.Nodes;

namespace Plugins.BehaviorTree.Runtime
{
    // Main BehaviorTree class to manage ticking and reset
    public class BehaviorTree
    {
        private readonly Node root;

        public NodeState LastState { get; private set; } = NodeState.NotActive;

        public Node Root => root;

        public BehaviorTree(Node root)
        {
            this.root = root;
        }

        // Call each Update()
        public NodeState Tick()
        {
            LastState = root.Tick();
            return LastState;
        }

        public void Reset()
        {
            // You may recreate tree or implement recursive reset if needed
            // root.CollectNodes(new List<Node>()).ForEach(node => {
            //     // reflection-based reset of state and started flag if implemented
            // });
            LastState = NodeState.NotActive;
        }

        // For editor: get all nodes and edges
        public List<Node> GetAllNodes()
        {
            var nodes = new List<Node>();
            root.CollectNodes(nodes);
            return nodes;
        }

        public List<(Node parent, Node child)> GetAllEdges()
        {
            var edges = new List<(Node, Node)>();
            root.CollectEdges(edges);
            return edges;
        }

        public void Start()
        {
            LastState = NodeState.Running;
        }
    }
}