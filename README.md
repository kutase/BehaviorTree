# Behavior Tree

AI Classes and Tools to create configurable artificial intelligence for Unity based on Behavior Trees.

## Installation

In Unity Package Manager, click `+` → `Add package from git URL...` and paste:

```
https://github.com/kutase/BehaviorTree.git
```

## Overview

This package provides a flexible Behavior Tree implementation for Unity, allowing you to build AI logic using composable nodes (composites, decorators, and tasks).

## Core Concepts

- **BehaviorTree**: The main class that manages the execution of a tree of nodes.
- **Node**: The base class for all behavior tree nodes. Nodes return a `NodeState` (`Running`, `Success`, `Failure`).
- **Composite Nodes**: Nodes that have children (e.g., `Sequence`, `Selector`).
- **Task Nodes**: Leaf nodes that perform actions or checks (e.g., `Delay`, custom tasks).

## Node States

- `Running`: The node is still executing.
- `Success`: The node finished successfully.
- `Failure`: The node failed.

> **Note:** The `NotActive` state is used internally for the visualizer and should not be returned from custom nodes.

## Behavior Tree Visualizer

The package includes a built-in visualizer for debugging and inspecting behavior trees at runtime.

- Open the visualizer via **Window → Behavior Tree Viewer** in the Unity Editor.
- The visualizer displays the currently running tree for the selected GameObject with a `BehaviorTreeRunner` component.
- Node states are color-coded and updated in real time as the tree executes.

## Example: Creating a Simple Behavior Tree

```csharp
using Plugins.BehaviorTree.Runtime;
using Plugins.BehaviorTree.Runtime.Nodes.Composites;
using Plugins.BehaviorTree.Runtime.Nodes.Tasks;

// Create a tree: Sequence(Delay(2s), ForceFailure())
var tree = new BehaviorTree(
    new Sequence(
        new Delay(2f),
        new ForceFailure()
    ),
    "Simple Example"
);
```

## Running a Behavior Tree in Unity

Attach the `BehaviorTreeRunner` MonoBehaviour to a GameObject and start the tree:

```csharp
using Plugins.BehaviorTree.Runtime;
using UnityEngine;

public class ExampleRunner : MonoBehaviour
{
    public BehaviorTreeRunner runner;

    void Start()
    {
        var tree = /* create your tree here */;
        runner.StartTree(tree);
    }
}
```

## Custom Nodes

Create your own nodes by inheriting from `Node`:

```csharp
using Plugins.BehaviorTree.Runtime.Nodes;

public class CustomAction : Node
{
    protected override NodeState Run()
    {
        // Your logic here
        return NodeState.Success;
    }
}
```

Custom nodes should only return `Running`, `Success`, or `Failure`.

## Real-World Example

A more advanced tree using dependency injection (Zenject) and custom nodes:

```csharp
using Plugins.BehaviorTree.Runtime;
using Plugins.BehaviorTree.Runtime.Nodes.Composites;
using Zenject;

public class GoToLaundryClientBehaviorTreeCreator : INPCClientBehaviorTreeCreator, IEarlyContainerConstructable
{
    public BehaviorTree CreateTree(DiContainer container)
    {
        var tree = new Sequence(
            container.Instantiate<SelectEquipment>(new object[] { "BagEquipment" }),
            container.Instantiate<GoToLaundry>()
        );
        return new BehaviorTree(tree, "Go to Laundry");
    }
}
```

### Tree visualization:
<img width="1442" height="687" alt="image" src="https://github.com/user-attachments/assets/b301f622-65c9-46af-a07f-d82cf9e0ffa2" />


Where `SelectEquipment` and `GoToLaundry` are custom nodes:

```csharp
public class SelectEquipment : Node
{
    [Inject] private NPCEquipment _equipment;
    private string _itemId;
    public SelectEquipment(string itemId) { _itemId = itemId; }
    protected override NodeState Run()
    {
        _equipment.SetContext(_itemId);
        return NodeState.Success;
    }
}

public class GoToLaundry : SetTargetBase
{
    [Inject] private Environment _environment;
    protected override void OnEnter()
    {
        SetTarget(_environment.LaundryEntrance);
    }
}
```

## License

MIT
