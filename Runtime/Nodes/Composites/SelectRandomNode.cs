using System.Collections.Generic;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Nodes.Composites
{
    // SelectRandomNode: Randomly selects one child node to execute
    public class SelectRandomNode : Composite
    {
        private int selectedChildIndex;

        public SelectRandomNode(params Node[] nodes) : base(nodes)
        {
        }

        protected override void OnEnter()
        {
            // Randomly select a child node when entering
            selectedChildIndex = Random.Range(0, children.Count);
        }

        protected override NodeState Run()
        {
            // Execute only the randomly selected child
            if (children.Count == 0)
                return NodeState.Failure;

            return children[selectedChildIndex].Tick();
        }
    }
} 