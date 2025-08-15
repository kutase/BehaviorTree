using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.BehaviorTree.Runtime.Configs;
using Plugins.BehaviorTree.Runtime.Nodes;
using Plugins.BehaviorTree.Runtime.Nodes.Composites;
using Plugins.BehaviorTree.Runtime.Nodes.Decorators;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Plugins.BehaviorTree.Runtime.Editor
{
    public class BehaviorTreeEditorWindow : EditorWindow
    {
        #region Fields
        // Panning and dragging
        private Vector2 panOffset = Vector2.zero;
        private Vector2 dragStartPos;
        private bool dragging = false;

        // Node positions and sizes
        private Dictionary<Node, Vector2> positions = new();
        private Dictionary<Node, Vector2> nodeSizes = new();

        // Current tree and runner
        private BehaviorTree currentTree;
        private BehaviorTreeRunner currentTreeRunner;
        private bool firstCenter = true;

        // Spacing between nodes
        private readonly float verticalSpacing = 80;
        private readonly float horizontalSpacing = 40;
        private readonly float minVerticalSpacing = 30; // minimum vertical spacing between nodes

        // Styles
        private GUIStyle treeNameStatusStyle;
        private Rect treeNameStatusRect;
        private GUIStyle HeaderStyle;
        private GUIStyle BodyStyle;
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            InitStyles();
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            Selection.selectionChanged += OnSelectionChange;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            Selection.selectionChanged -= OnSelectionChange;
        }
        #endregion

        #region Menu
        [MenuItem("Window/Behavior Tree Viewer")]
        public static void OpenWindow()
        {
            var window = CreateInstance<BehaviorTreeEditorWindow>();
            window.titleContent = new GUIContent("Behavior Tree");
            window.Show();
        }
        #endregion

        #region Main GUI
        private void OnGUI()
        {
            if (currentTreeRunner != null && currentTree != currentTreeRunner.Tree)
            {
                OnSelectionChange();
            }

            HandlePanInput();

            if (currentTree != null && currentTree.Root != null)
            {
                if (firstCenter)
                {
                    Layout(currentTree.Root, Vector2.zero);
                    CenterTree();
                    firstCenter = false;
                }

                DrawCanvas();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a GameObject with a BehaviorTreeRunner.", MessageType.Info);
            }
        }

        void Update()
        {
            var updateCondition = EditorApplication.isPlaying && currentTree != null;
            if (updateCondition)
            {
                Repaint();
            }
        }
        #endregion

        #region Selection & Playmode
        private void PlayModeStateChanged(PlayModeStateChange obj)
        {
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                currentTreeRunner = Selection.activeGameObject.GetComponentInChildren<BehaviorTreeRunner>();
                if (currentTreeRunner != null)
                {
                    currentTree = currentTreeRunner.Tree;
                }
                else
                {
                    currentTree = null;
                }
            }
            else
            {
                currentTree = null;
                currentTreeRunner = null;
            }

            positions.Clear();
            nodeSizes.Clear();
            firstCenter = true;

            Repaint();
        }
        #endregion

        #region Styles
        private void InitStyles()
        {
            if (HeaderStyle == null)
                HeaderStyle = CreateHeaderStyle();

            if (BodyStyle == null)
                BodyStyle = CreateBodyStyle();

            treeNameStatusStyle = new GUIStyle { fontSize = 36, fontStyle = FontStyle.Bold };
            treeNameStatusStyle.normal.textColor = BehaviorTreeConfig.Instance.treeNameColor;

            treeNameStatusRect = new Rect(20f, 20f, 250f, 150f);
        }

        private GUIStyle CreateHeaderStyle()
        {
            var style = new GUIStyle();
            style.normal.textColor = BehaviorTreeConfig.Instance.headerColor;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            style.stretchWidth = true;
            style.stretchHeight = true;
            return style;
        }

        private GUIStyle CreateBodyStyle()
        {
            var style = new GUIStyle();
            style.normal.textColor = BehaviorTreeConfig.Instance.descriptionColor;
            style.alignment = TextAnchor.UpperCenter;
            style.wordWrap = true;
            style.clipping = TextClipping.Clip;
            style.stretchWidth = true;
            style.stretchHeight = true;
            return style;
        }
        #endregion

        #region Input
        /// <summary>
        /// Handles mouse input for panning.
        /// </summary>
        private void HandlePanInput()
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                dragging = true;
                dragStartPos = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && dragging)
            {
                Vector2 delta = e.mousePosition - dragStartPos;
                panOffset += delta;
                dragStartPos = e.mousePosition;
                Repaint();
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                dragging = false;
                e.Use();
            }
        }
        #endregion

        #region Layout
        /// <summary>
        /// Calculates the width of a subtree to avoid overlapping
        /// </summary>
        private float GetSubtreeWidth(Node node)
        {
            var size = GetNodeSize(node);

            if (node is Composite composite && composite.Children.Count > 0)
            {
                float totalWidth = 0f;
                for (int i = 0; i < composite.Children.Count; i++)
                {
                    var child = composite.Children[i];
                    float childWidth = GetSubtreeWidth(child);
                    totalWidth += childWidth;

                    if (i < composite.Children.Count - 1)
                        totalWidth += GetDynamicSpacing(child); // dynamic spacing between child nodes
                }
                return Mathf.Max(size.x, totalWidth);
            }
            else if (node is Decorator decorator && decorator.Child != null)
            {
                return Mathf.Max(size.x, GetSubtreeWidth(decorator.Child));
            }
            else
            {
                return size.x;
            }
        }

        /// <summary>
        /// Dynamic horizontal spacing to prevent node overlapping
        /// </summary>
        private float GetDynamicSpacing(Node node)
        {
            // Minimum spacing + part of node width to prevent overlapping
            var nodeSize = GetNodeSize(node);
            return horizontalSpacing + nodeSize.x * 0.4f;
        }

        /// <summary>
        /// Recursively lays out the tree, taking into account subtree widths and dynamic spacing
        /// </summary>
        private void Layout(Node node, Vector2 origin)
        {
            var size = GetNodeSize(node);

            if (node is Composite composite && composite.Children.Count > 0)
            {
                float totalChildrenWidth = GetSubtreeWidth(node);
                float x = origin.x - totalChildrenWidth / 2f;

                List<Vector2> childPositions = new();

                for (int i = 0; i < composite.Children.Count; i++)
                {
                    var child = composite.Children[i];
                    float childWidth = GetSubtreeWidth(child);
                    float childX = x + childWidth / 2f;

                    // Use maximum spacing to prevent overlapping
                    Layout(child, new Vector2(childX, origin.y + size.y + Mathf.Max(verticalSpacing, minVerticalSpacing)));
                    childPositions.Add(positions[child]);

                    x += childWidth;
                    if (i < composite.Children.Count - 1)
                        x += GetDynamicSpacing(child);
                }

                float midX = (childPositions.First().x + childPositions.Last().x) / 2f;
                positions[node] = new Vector2(midX, origin.y);
            }
            else if (node is Decorator decorator && decorator.Child != null)
            {
                // Place child node below with minimum spacing
                Layout(decorator.Child, origin + new Vector2(0, size.y + Mathf.Max(verticalSpacing, minVerticalSpacing)));
                Vector2 childPos = positions[decorator.Child];
                positions[node] = new Vector2(childPos.x, origin.y);
            }
            else
            {
                positions[node] = origin;
            }
        }

        /// <summary>
        /// Centers the tree on the screen.
        /// </summary>
        private void CenterTree()
        {
            if (positions.Count == 0)
                return;

            float minX = positions.Values.Min(pos => pos.x);
            float maxX = positions.Values.Max(pos => pos.x);
            float minY = positions.Values.Min(pos => pos.y);
            float maxY = positions.Values.Max(pos => pos.y);

            Vector2 center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
            Vector2 canvasCenter = position.size / 2f;

            panOffset = canvasCenter - center;
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Main method for drawing the canvas.
        /// </summary>
        private void DrawCanvas()
        {
            Handles.BeginGUI();
            DrawGrid(position.size);
            DrawTreeName();
            DrawConnections();
            Handles.EndGUI();
            DrawNodes();
        }

        /// <summary>
        /// Draws all connections between nodes.
        /// </summary>
        private void DrawConnections()
        {
            foreach (var pair in positions)
            {
                var startNodeSize = GetNodeSize(pair.Key);

                if (pair.Key is Composite comp)
                {
                    foreach (var child in comp.Children)
                    {
                        var childNodeSize = GetNodeSize(child);
                        if (positions.TryGetValue(child, out var childPos))
                            DrawConnection(startNodeSize, childNodeSize, pair.Value, childPos);
                    }
                }
                else if (pair.Key is Decorator dec && dec.Child != null)
                {
                    var childNodeSize = GetNodeSize(dec.Child);
                    if (positions.TryGetValue(dec.Child, out var childPos))
                        DrawConnection(startNodeSize, childNodeSize, pair.Value, childPos);
                }
            }
        }

        /// <summary>
        /// Draws all nodes.
        /// </summary>
        private void DrawNodes()
        {
            foreach (var pair in positions)
            {
                DrawNode(pair.Key, pair.Value + panOffset);
            }
        }

        /// <summary>
        /// Draws the tree name.
        /// </summary>
        public void DrawTreeName()
        {
            GUI.Label(treeNameStatusRect, currentTree?.TreeName ?? "", treeNameStatusStyle);
        }

        /// <summary>
        /// Draws the grid.
        /// </summary>
        private void DrawGrid(Vector2 size)
        {
            var canvasRect = new Rect(Vector2.zero, size);
            DrawGrid(canvasRect, BehaviorTreeConfig.Instance.gridTexture, 1f, panOffset);
        }

        /// <summary>
        /// Draws a single node.
        /// </summary>
        private void DrawNode(Node node, Vector2 pos)
        {
            Rect rect = GetNodeRect(node, pos);
            DrawNodeBackground(node, rect);
            DrawNodeStatus(node, rect);
            DrawNodeContent(node, rect, node);
        }

        /// <summary>
        /// Gets the rectangle for a node.
        /// </summary>
        private Rect GetNodeRect(Node node, Vector2 pos)
        {
            Vector2 size = GetNodeSize(node);
            return new Rect(pos.x, pos.y, size.x, size.y);
        }

        /// <summary>
        /// Gets the node size (with caching).
        /// </summary>
        private Vector2 GetNodeSize(Node node)
        {
            if (nodeSizes.ContainsKey(node))
                return nodeSizes[node];

            node.UpdateEditorGui();
            Vector2 headerSize = HeaderStyle.CalcSize(node.HeaderContent);
            Vector2 bodySize = BodyStyle.CalcSize(node.BodyContent);
            float padding = 10f;
            float width = Mathf.Max(headerSize.x, bodySize.x) + padding * 2;
            float height = headerSize.y + bodySize.y + padding * 3;
            nodeSizes[node] = new Vector2(width, height);
            return nodeSizes[node];
        }

        /// <summary>
        /// Draws the node background.
        /// </summary>
        private void DrawNodeBackground(Node node, Rect rect)
        {
            Color bgColor = GetStateColor(node.State);
            GUI.DrawTexture(
                rect,
                BehaviorTreeConfig.Instance.nodeGradient,
                ScaleMode.StretchToFill,
                true,
                0f,
                bgColor,
                0f,
                4f);
        }

        /// <summary>
        /// Draws the node content.
        /// </summary>
        private void DrawNodeContent(Node node, Rect rect, Node contentNode)
        {
            Vector2 headerSize = HeaderStyle.CalcSize(contentNode.HeaderContent);
            Vector2 bodySize = BodyStyle.CalcSize(contentNode.BodyContent);
            float padding = 10f;
            GUILayout.BeginArea(rect);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(contentNode.HeaderContent, HeaderStyle,
                        GUILayout.ExpandWidth(true),
                        GUILayout.Height(headerSize.y));
                    GUILayout.Label(contentNode.BodyContent, BodyStyle,
                        GUILayout.ExpandWidth(true),
                        GUILayout.Height(bodySize.y));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draws the node status (state icon).
        /// </summary>
        private static void DrawNodeStatus(Node node, Rect rect)
        {
            if (node.State == NodeState.Success)
            {
                DrawTexture(rect, BehaviorTreeConfig.Instance.successSymbol, BehaviorTreeConfig.Instance.successColor);
            }
            else if (node.State == NodeState.Failure)
            {
                DrawTexture(rect, BehaviorTreeConfig.Instance.failureSymbol, BehaviorTreeConfig.Instance.failureColor);
            }
            else if (node.State == NodeState.Running)
            {
                DrawTexture(rect, BehaviorTreeConfig.Instance.runningSymbol, BehaviorTreeConfig.Instance.runningColor);
            }
        }

        /// <summary>
        /// Draws a connection between two nodes.
        /// </summary>
        private void DrawConnection(Vector2 fromNodeSize, Vector2 toNodeSize, Vector2 from, Vector2 to)
        {
            Vector3 start = from + panOffset + new Vector2(fromNodeSize.x / 2f, fromNodeSize.y);
            Vector3 end = to + panOffset + new Vector2(toNodeSize.x / 2f, 0);
            Vector3 startTan = start + Vector3.up * 30f;
            Vector3 endTan = end + Vector3.down * 30f;
            Handles.DrawBezier(start, end, startTan, endTan, Color.gray, null, 3f);
        }

        /// <summary>
        /// Helper method for drawing a texture with color.
        /// </summary>
        public static void DrawTexture(Rect r, Texture2D tex, Color c)
        {
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true, 0f, c, 0f, 0f);
        }

        /// <summary>
        /// Draws the grid (static method).
        /// </summary>
        public static void DrawGrid(Rect canvas, Texture texture, float zoom, Vector2 pan)
        {
            var size = canvas.size;
            var center = size / 2f;
            float xOffset = -(center.x * zoom + pan.x) / texture.width;
            float yOffset = ((center.y - size.y) * zoom + pan.y) / texture.height;
            Vector2 tileOffset = new Vector2(xOffset, yOffset);
            float tileAmountX = Mathf.Round(size.x * zoom) / texture.width;
            float tileAmountY = Mathf.Round(size.y * zoom) / texture.height;
            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);
            GUI.DrawTextureWithTexCoords(canvas, texture, new Rect(tileOffset, tileAmount));
        }
        #endregion

        #region Utils
        /// <summary>
        /// Gets the color for the node state.
        /// </summary>
        private Color GetStateColor(NodeState state)
        {
            return state switch
            {
                NodeState.Running => BehaviorTreeConfig.Instance.runningColor,
                NodeState.Success => BehaviorTreeConfig.Instance.successColor,
                NodeState.Failure => BehaviorTreeConfig.Instance.failureColor,
                NodeState.NotActive => BehaviorTreeConfig.Instance.defaultNodeBackgroundColor,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
        #endregion
    }
}
#endif