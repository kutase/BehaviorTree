using System.Collections.Generic;

namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    // AbortIf executes action, but checks condition every tick.
    // If condition is Success, it aborts action and returns Success.
    public class AbortIf : Node
    {
        private readonly Node actionNode;
        private readonly Node conditionNode;

        public Node ActionNode => actionNode;
        public Node ConditionNode => conditionNode;

        public AbortIf(Node actionNode, Node conditionNode)
        {
            this.actionNode = actionNode;
            this.conditionNode = conditionNode;
        }

        protected override NodeState Run()
        {
            var conditionState = conditionNode.Tick();
            if (conditionState == NodeState.Success)
            {
                actionNode.Reset();
                return NodeState.Success;
            }
            else
            {
                return actionNode.Tick();
            }
        }

        public override void Reset()
        {
            actionNode.Reset();
            conditionNode.Reset();

            base.Reset();
        }
    }
}
