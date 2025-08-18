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

        public void ReplaceTree(BehaviorTree tree)
        {
            Tree = tree;
        }

        public void StartTree(BehaviorTree tree)
        {
            Tree = tree;
            Tree.Start();
        }

        public void StopTree()
        {
            Tree = null;
        }

        private void Update()
        {
            if (!_runTreeInUpdate)
                return;

            if (Tree == null)
                return;

            if (TreeState == NodeState.Running)
                Tree.Tick();
        }
    }
}