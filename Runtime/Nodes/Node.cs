using System.Collections.Generic;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Nodes
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
        public virtual string Name
        {
            get
            {
                return string.IsNullOrWhiteSpace(title) ? GetType().Name : title;
            }

            set
            {
                title = value;

                #if UNITY_EDITOR
                UpdateEditorGui();
                #endif
            }
        }

        public virtual string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(description) &&
                    (!string.IsNullOrWhiteSpace(title) && title != GetType().Name))
                {
                    return GetType().Name;
                }

                return description;
            }
    
            set
            {
                description = value;

                #if UNITY_EDITOR
                UpdateEditorGui();
                #endif
            }
        }

        public NodeState State => state;

        public Node WithTitle(string title = "", string description = "")
        {
            this.title = title;
            this.description = description;

            return this;
        }

        public virtual void Reset()
        {
            state = NodeState.NotActive;
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
        protected abstract NodeState Run();

        // Public tick: handles enter/exit and execution
        public NodeState Tick()
        {
            if (!started)
            {
                OnEnter();
                started = true;
                state = NodeState.Running;
            }

            state = Run();

            if (state != NodeState.Running)
            {
                OnExit();
                started = false;
            }

            return state;
        }
    }
}