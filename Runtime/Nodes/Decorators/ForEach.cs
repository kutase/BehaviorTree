using System.Collections.Generic;

namespace Plugins.BehaviorTree.Runtime.Nodes.Decorators
{
    /// <summary>
    /// Abstract decorator that iterates through a List and executes child node for each element.
    /// Inherit from this class and implement GetList() to provide the list to iterate through.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list</typeparam>
    public abstract class ForEach<T> : Decorator
    {
        protected int currentIndex = -1;
        protected List<T> currentList;
        protected T currentElement;
        protected bool isInitialized = false;

        /// <summary>
        /// Override this method to provide the list to iterate through.
        /// This method is called once at the beginning of execution.
        /// </summary>
        /// <returns>The list to iterate through</returns>
        protected abstract List<T> GetList();

        protected virtual void ClearList()
        {
            currentList = null;
        }

        /// <summary>
        /// Override this method to handle the current element before executing the child node.
        /// This is called before each child execution.
        /// </summary>
        /// <param name="element">Current element from the list</param>
        /// <param name="index">Current index in the list</param>
        protected virtual void OnElementStart(T element, int index)
        {
            // Override in derived classes if needed
        }

        /// <summary>
        /// Override this method to handle the result after executing the child node.
        /// This is called after each child execution.
        /// </summary>
        /// <param name="element">Current element from the list</param>
        /// <param name="index">Current index in the list</param>
        /// <param name="result">Result from the child node execution</param>
        protected virtual void OnElementComplete(T element, int index, NodeState result)
        {
            // Override in derived classes if needed
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            currentIndex = -1;
            currentList = GetList();
            isInitialized = true;
        }

        protected override void OnExit()
        {
            base.OnExit();
            currentIndex = -1;
            currentElement = default(T);
            isInitialized = false;

            ClearList();
        }

        protected override NodeState Run()
        {
            if (!isInitialized || currentList == null)
            {
                return NodeState.Failure;
            }

            // If we haven't started or finished the current element
            if (currentIndex < 0 || currentIndex >= currentList.Count)
            {
                // Move to next element
                currentIndex++;
                
                // Check if we've processed all elements
                if (currentIndex >= currentList.Count)
                {
                    return NodeState.Success;
                }

                // Get current element and notify
                currentElement = currentList[currentIndex];
                OnElementStart(currentElement, currentIndex);
            }

            // Execute child node
            NodeState childResult = child.Tick();

            // If child is still running, continue
            if (childResult == NodeState.Running)
            {
                return NodeState.Running;
            }

            // Child completed, notify and move to next element
            OnElementComplete(currentElement, currentIndex, childResult);

            return NodeState.Running; // Continue with next element
        }
    }
} 