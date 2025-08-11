# Behavior Tree

AI classes and tools for creating configurable artificial intelligence in Unity using Behavior Trees.

## Installation

1. In the Unity Package Manager, click **+** → **Add package from Git URL...**  
2. Paste the following URL:

   ```
   https://github.com/kutase/BehaviorTree.git
   ```

## Overview

This package provides a flexible implementation of Behavior Trees for Unity, allowing you to build AI logic using composable nodes (composites, decorators, and tasks).

## Core Concepts

- **BehaviorTree**: The main class that manages the execution of a tree of nodes.  
- **Node**: The base class for all behavior tree nodes. Nodes return a `NodeState` (`Running`, `Success`, or `Failure`).  
- **Composite Nodes**: Nodes that contain multiple children (e.g., `Sequence`, `Selector`).  
- **Decorator Nodes**: Nodes that modify the behavior of a single child (e.g., `Repeat`, `Inverter`, `IfElse`).  
- **Task Nodes**: Leaf nodes that perform actions or checks (e.g., `Delay`, `ForceFailure`, custom task nodes).

## Node Reference

### Abstract Base Classes

- **Node**  
  The abstract base class for all behavior tree nodes.  
  - **Key methods**:  
    - `Tick()`: Starts node execution and handles enter/exit logic.  
    - `Run()`: Abstract method; implement your node’s main logic here.  
    - `OnEnter()`, `OnExit()`: Hooks called when the node starts and finishes.  
  - **Properties**:  
    - `Name`, `Description`: For editor/debug display.  
    - `State`: Current state (`NodeState`).

- **Composite**  
  Abstract class for composite nodes (contain multiple child nodes).  
  - **Properties**:  
    - `Children`: List of child nodes.  
  - **Constructor**: Takes a collection of child nodes.

- **Decorator**  
  Abstract class for decorators (wrap a single child node).  
  - **Properties**:  
    - `Child`: The wrapped child node.  
  - **Constructor**: Takes a single child node.  
  - **Default behavior**: Simply ticks the child node.

### Implemented Composite Nodes

- **Sequence**  
  Executes children in order (AND logic).  
  - If any child returns `Failure`, the sequence returns `Failure`.  
  - If all children return `Success`, returns `Success`.  
  - If a child returns `Running`, returns `Running`.

- **Selector**  
  Executes children in order (OR logic).  
  - If any child returns `Success`, the selector returns `Success`.  
  - If all children return `Failure`, returns `Failure`.  
  - If a child returns `Running`, returns `Running`.

### Implemented Decorators

- **IfElse**  
  Chooses between two child nodes based on a condition (`Func<bool>`).  
  - **Constructor**: Takes a condition, an 'if' node, and an 'else' node.  
  - On each tick, runs the 'if' node if the condition is true; otherwise runs the 'else' node.

### Implemented Tasks (Leaf Nodes)

- **Delay**  
  Waits for a specified number of seconds before returning `Success`.  
  - **Constructor**: Takes the duration (seconds).  
  - Returns `Running` until the time elapses, then returns `Success`.

- **ForceFailure**  
  Always returns `Failure`.

---

## Node Reference

### Abstract Base Classes

- **Node**  
  The abstract base class for all behavior tree nodes.
  - Key methods:
    - `Tick()`: Starts node execution, handles enter/exit logic.
    - `Run()`: Abstract method; implement your node's main logic here.
    - `OnEnter()`, `OnExit()`: Hooks called when the node starts and finishes.
  - Properties:
    - `Name`, `Description`: For editor/debug display.
    - `State`: Current state (`NodeState`).

- **Composite**  
  Abstract class for composite nodes (contain multiple child nodes).
  - Properties:
    - `Children`: List of child nodes.
  - Constructor takes a collection of child nodes.

- **Decorator**  
  Abstract class for decorators (wrap a single child node).
  - Properties:
    - `Child`: The wrapped child node.
  - Constructor takes a single child node.
  - By default, simply ticks the child node.

---

### Implemented Composite Nodes

- **Sequence**  
  Executes child nodes in order (AND logic).
  - If any child returns `Failure`, the sequence returns `Failure`.
  - If all children return `Success`, returns `Success`.
  - If a child returns `Running`, returns `Running`.

- **Selector**  
  Executes child nodes in order (OR logic).
  - If any child returns `Success`, the selector returns `Success`.
  - If all children return `Failure`, returns `Failure`.
  - If a child returns `Running`, returns `Running`.

---

### Implemented Decorators

- **IfElse**  
  Decorator that chooses between two child nodes based on a condition (`Func<bool>`).
  - Constructor takes: a condition, an 'if' node, and an 'else' node.
  - On each tick, selects which node to run based on the condition result.

---

### Implemented Tasks (Leaf Nodes)

- **Delay**  
  Leaf node that waits for a specified number of seconds before returning `Success`.
  - Constructor takes the duration (seconds).
  - Returns `Running` until the time elapses, then returns `Success`.

- **ForceFailure**  
  Leaf node that always returns `Failure`.

---

### Example: Creating Custom Nodes

```csharp
public class MyCustomTask : Node
{
    protected override NodeState Run()
    {
        // Your logic here
        return NodeState.Success;
    }
}

public class MyCustomComposite : Composite
{
    public MyCustomComposite(IEnumerable<Node> children) : base(children) { }
    // Implement your own traversal logic
}
```

## Node States

- **Running**: The node is still executing.  
- **Success**: The node completed successfully.  
- **Failure**: The node failed.

> **Note:** The `NotActive` state is used internally by the visualizer and should not be returned by custom nodes.

## Behavior Tree Visualizer

The package includes a built-in visualizer for debugging and inspecting behavior trees at runtime:

1. Open it via **Window → Behavior Tree Viewer** in the Unity Editor.  
2. Select a GameObject with a `BehaviorTreeRunner` component to view its currently running tree.  
3. Node states are color-coded and update in real time as the tree executes.

### Tree Visualization

<img width="733" height="442" alt="grafik" src="https://github.com/user-attachments/assets/6acc11ea-194f-48a7-b8e9-90b968d737aa" />



## Example: Creating a Simple Behavior Tree

```csharp
using Plugins.BehaviorTree.Runtime;
using Plugins.BehaviorTree.Runtime.Nodes.Composites;
using Plugins.BehaviorTree.Runtime.Nodes.Tasks;

// Create a tree: Sequence(Delay(2f), ForceFailure())
var tree = new BehaviorTree(
    new Sequence(
        new Delay(2f),
        new ForceFailure()
    ),
    "Simple Example"
);
```

## Running a Behavior Tree in Unity

Attach the `BehaviorTreeRunner` component to a GameObject and start the tree:

```csharp
public class ExampleRunner : MonoBehaviour
{
    public BehaviorTreeRunner behaviorTreeRunner;

    public async void RunTree()
    {
        // Create your tree here
        var tree = /* create your tree */;
        behaviorTreeRunner.StartTree(tree);

        // Wait until the tree finishes executing
        while (behaviorTreeRunner.TreeState == NodeState.Running)
            await UniTask.Yield();

        behaviorTreeRunner.StopTree();
    }
}
```

## Creating Custom Nodes

```csharp
// Custom task node
public class MyCustomTask : Node
{
    protected override NodeState Run()
    {
        // Your logic here
        return NodeState.Success;
    }
}

// Custom composite node
public class MyCustomComposite : Composite
{
    public MyCustomComposite(IEnumerable<Node> children) : base(children) { }
    // Implement your own traversal logic here
}
```

## Example with Zenject

This package works seamlessly with Zenject. Use a `GameObjectContext` to provide dependencies to nodes:

```csharp
using Plugins.BehaviorTree.Runtime;
using Plugins.BehaviorTree.Runtime.Nodes.Composites;
using Zenject;

//...
public class GoToTargetNode : SetTargetBaseNode
 {
     [Inject] private IEnvironment _environment;
     [Inject] private IRandomizer _randomizer;

     protected override void OnEnter()
     {
         var exitPoint = _randomizer.SelectRandomElement(_environment.GetExitPointsPark());
         SetTarget(exitPoint);
     }
 }
//...

public class TreeFactory
{
    public BehaviorTree CreateTree(DiContainer container)
    {
        var sequence = new Sequence(
            container.Instantiate<SelectEquipmentNode>(new object[] { "BagEquipment" }),
            container.Instantiate<GoToTargetNode>(),
            container.Instantiate<DespawnCharacterNode>()
        );

        return new BehaviorTree(sequence, "Go to Laundry");
    }
}
```

## License

This project is licensed under the GNU General Public License v3.0.  
See [LICENSE](https://github.com/kutase/BehaviorTree/blob/main/LICENSE) for full terms.
