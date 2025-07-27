using System.Collections.Generic;
using Plugins.BehaviorTree.Runtime.Nodes;

namespace Plugins.BehaviorTree.Runtime
{
    // Main BehaviorTree class to manage ticking and reset
    public class BehaviorTree
    {
        public string TreeName { get; private set; }

        private readonly Node root;

        public NodeState LastState { get; private set; } = NodeState.NotActive;

        public Node Root => root;

        public BehaviorTree(Node root, string treeName = "")
        {
            this.root = root;
            TreeName = treeName;
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

        public void Start()
        {
            LastState = NodeState.Running;
        }
    }
}