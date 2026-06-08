using System.Collections.Generic;
using Plugins.BehaviorTree.Runtime.Nodes;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime
{
    // Usage in a MonoBehaviour
    public class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeField] private bool _runTreeInUpdate = true;

        public BehaviorTree Tree { get; private set; }

        public NodeState TreeState => Tree?.LastState ?? NodeState.NotActive;

        private HashSet<BehaviorTree> ResettingTrees { get; } = new();

        public void ReplaceTree(BehaviorTree tree)
        {
            ResetTreesInternal();
            Tree = tree;
        }

        public void StartTree(BehaviorTree tree)
        {
            ResetTreesInternal();
            Tree = tree;
            Tree.Start();
        }

        public void StopTree()
        {
            if (Tree == null)
            {
                return;
            }

            ResettingTrees.Add(Tree);
            Tree = null;
        }

        private void Update()
        {
            if (!_runTreeInUpdate)
            {
                return;
            }

            if (Tree == null)
            {
                return;
            }

            if (TreeState == NodeState.Running)
            {
                Tree.Tick();
            }

            ResetTreesInternal();
        }

        private void ResetTreesInternal()
        {
            foreach (var tree in ResettingTrees)
            {
                tree.Reset();
            }
            ResettingTrees.Clear();
        }
    }
}