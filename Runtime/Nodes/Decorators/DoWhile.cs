namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    // DoWhile executes action, then checks condition.
    // If condition is Success, it repeats action.
    // If condition is Failure, it stops and returns Success.
    public class DoWhile : Node
    {
        private readonly Node actionNode;
        private readonly Node conditionNode;

        public Node ActionNode => actionNode;
        public Node ConditionNode => conditionNode;

        public DoWhile(Node actionNode, Node conditionNode)
        {
            this.actionNode = actionNode;
            this.conditionNode = conditionNode;
        }

        protected override NodeState Run()
        {
            var conditionState = conditionNode.Tick();
            if (conditionState == NodeState.Success)
            {
                actionNode.Tick();
                return NodeState.Running;
            }
            else if (conditionState == NodeState.Failure)
            {
                return NodeState.Success;
            }
            else
            {
                return conditionState;
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
