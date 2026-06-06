#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Editor
{
    [EditorToolbarElement(OpenBehaviorTreeViewerButton.Id, typeof(SceneView))]
    internal sealed class OpenBehaviorTreeViewerButton : EditorToolbarButton
    {
        public const string Id = "BehaviorTree/SceneView/OpenViewer";

        public OpenBehaviorTreeViewerButton()
        {
            icon = ResolveIcon();
            tooltip = "Behavior Tree Viewer";
            clicked += BehaviorTreeEditorWindow.OpenOrFocus;
        }

        private static Texture2D ResolveIcon()
        {
            return TryGet("d_tree_icon_leaf", out var icon) ? icon : Texture2D.whiteTexture;
        }

        private static bool TryGet(string name, out Texture2D icon)
        {
            icon = null;
            var content = EditorGUIUtility.IconContent(name);
            if (content == null || content.image == null)
            {
                return false;
            }

            icon = content.image as Texture2D;
            return true;
        }
    }

    [Overlay(typeof(SceneView), "", true)]
    internal sealed class BehaviorTreeViewerToolbarOverlay : ToolbarOverlay, ITransientOverlay
    {
        public bool visible => BehaviorTreeEditorSelection.TryGetRunnerFromSelection(out _);

        BehaviorTreeViewerToolbarOverlay() : base(OpenBehaviorTreeViewerButton.Id)
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            base.OnWillBeDestroyed();
        }

        private static void OnSelectionChanged()
        {
            SceneView.RepaintAll();
        }
    }
}
#endif
