using Plugins.BehaviorTree.Runtime.Nodes;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime
{
    // Usage in a MonoBehaviour
    public class BehaviorTreeRunner : MonoBehaviour
    {
        public BehaviorTree Tree { get; private set; }

        public NodeState TreeState => Tree?.LastState ?? NodeState.NotActive;

        public void StartTree(BehaviorTree tree)
        {
            Tree = tree;
            Tree.Start();
        }

        public void StopTree()
        {
            Tree = null;
        }

        // private void Start()
        // {
        //     var tree = new BehaviorTree(new Sequence(
        //         new Delay(5f),
        //         new ForceFailure()
        //     ));
        //     StartTree(tree);
        // }

        private void Update()
        {
            if (Tree == null)
                return;

            if (TreeState == NodeState.Running)
                Tree.Tick();
        }
    }
}