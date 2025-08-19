namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    // Decorator wraps one child node
    public abstract class Decorator : Node
    {
        protected Node child;

        public Node Child => child;

        public virtual Decorator WithChild(Node child)
        {
            this.child = child;
            return this;
        }

        protected override NodeState Run()
        {
            return child.Tick();
        }

        public override void Reset()
        {
            base.Reset();
            child.Reset();
        }
    }
}