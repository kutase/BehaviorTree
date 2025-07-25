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

        protected override NodeState Run()
        {
            // Switch child based on condition
            child = condition() ? ifNode : elseNode;
            return base.Run();
        }
    }
}