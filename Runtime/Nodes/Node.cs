using System.Collections.Generic;
using Bonsai.Designer;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Plugins.Behavior_Tree.Runtime.New.Nodes
{
    // Base class for all nodes, with OnEnter/OnExit hooks
    public abstract class Node
    {
        #if UNITY_EDITOR
        public GUIContent HeaderContent = new GUIContent();
        public GUIContent BodyContent = new GUIContent();

        public void UpdateEditorGui()
        {
            HeaderContent.text = Name;
            BodyContent.text = Description;
        }
        #endif

        private string title;
        private string description;

        private bool started;
        protected NodeState state = NodeState.NotActive;

        // Display name for editor and debugging
        public virtual string Name => string.IsNullOrWhiteSpace(title) ? GetType().Name : title;
        public virtual string Description => string.IsNullOrWhiteSpace(description) ? "" : description;

        public NodeState State => state;

        public Node WithTitle(string title = "", string description = "")
        {
            this.title = title;
            this.description = description;

            return this;
        }

        // Called once before execution when node starts running
        protected virtual void OnEnter()
        {
        }

        // Called once when node finishes (success or failure)
        protected virtual void OnExit()
        {
        }

        // Core execution logic for this node
        protected abstract NodeState ExecuteNode();

        // Public tick: handles enter/exit and execution
        public NodeState Tick()
        {
            if (!started)
            {
                OnEnter();
                started = true;
                state = NodeState.Running;
            }

            state = ExecuteNode();

            if (state != NodeState.Running)
            {
                OnExit();
                started = false;
            }

            return state;
        }

        // Collect all nodes in subtree
        public virtual void CollectNodes(List<Node> nodes)
        {
            nodes.Add(this);
        }

        // Collect edges parent->child for visualization
        public virtual void CollectEdges(List<(Node parent, Node child)> edges)
        {
        }
    }
}