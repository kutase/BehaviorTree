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

        // Click detection (to open node script on click)
        private bool clickCandidate = false;
        private Vector2 clickStartPos;
        private const float clickThreshold = 4f; // pixels

        // Zoom
        private float zoom;

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

            // Input handling (pan + zoom + clicks)
            HandlePanInput();
            HandleZoomInput();

            if (currentTree != null && currentTree.Root != null)
            {
                if (firstCenter)
                {
                    Layout(currentTree.Root, Vector2.zero);
                    CenterTree();
                    firstCenter = false;
                }

                DrawCanvas();

                // Show zoom percentage
                DrawZoomLabel();
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

            ResetZoom();

            Repaint();
        }

        private void ResetZoom()
        {
            zoom = BehaviorTreeConfig.Instance.defaultZoom;

            HeaderStyle.fontSize = Mathf.RoundToInt(zoom * BehaviorTreeConfig.Instance.headerFontSize);
            BodyStyle.fontSize = Mathf.RoundToInt(zoom * BehaviorTreeConfig.Instance.bodyFontSize);
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
            style.fontSize = Mathf.RoundToInt(BehaviorTreeConfig.Instance.headerFontSize * zoom);
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
            style.fontSize = Mathf.RoundToInt(BehaviorTreeConfig.Instance.bodyFontSize * zoom);
            return style;
        }
        #endregion

        #region Input
        /// <summary>
        /// Handles mouse input for panning and node clicking.
        /// </summary>
        private void HandlePanInput()
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Begin possible click/drag
                dragging = false; // don't assume pan until we see drag
                dragStartPos = e.mousePosition;
                clickStartPos = e.mousePosition;
                clickCandidate = true;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0)
            {
                Vector2 delta = e.mousePosition - dragStartPos;

                // If movement exceeds threshold, consider this a drag (pan)
                if (!dragging && delta.magnitude > clickThreshold)
                {
                    dragging = true;
                    clickCandidate = false;
                }

                if (dragging)
                {
                    panOffset += delta;
                    dragStartPos = e.mousePosition;
                    Repaint();
                }

                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                // If it was a click (no significant drag), handle node click
                if (clickCandidate && Vector2.Distance(clickStartPos, e.mousePosition) <= clickThreshold)
                {
                    OnNodeClicked(e.mousePosition);
                }

                // Reset flags
                dragging = false;
                clickCandidate = false;
                e.Use();
            }
        }

        /// <summary>
        /// Handles mouse wheel input for zooming (zoom centered on mouse pointer).
        /// </summary>
        private void HandleZoomInput()
        {
            Event e = Event.current;
            if (e.type == EventType.ScrollWheel)
            {
                var zoomDirection = e.delta.y;

                float scale = (zoomDirection < 0f) ? (1f - BehaviorTreeConfig.Instance.zoomSensitivity) : (1f + BehaviorTreeConfig.Instance.zoomSensitivity);
                var nextZoom = zoom * scale;

                ZoomAt(e.mousePosition, nextZoom);

                e.Use();
            }
        }

        /// <summary>
        /// Zooms the canvas keeping the screenPoint under the mouse stationary.
        /// </summary>
        /// <param name="screenPoint">Mouse position in screen coordinates (GUI coords)</param>
        /// <param name="scale">Multiplicative scale to apply to current zoom.</param>
        private void ZoomAt(Vector2 screenPoint, float scale)
        {
            float oldZoom = zoom;
            float newZoom = Mathf.Clamp(scale, BehaviorTreeConfig.Instance.minZoom, BehaviorTreeConfig.Instance.maxZoom);
            if (Mathf.Approximately(newZoom, oldZoom))
                return;

            float finalScale = newZoom / oldZoom;

            // Translate panOffset so that the screenPoint remains pointing to the same logical canvas point.
            // panOffset is in screen-space pixels; positions are scaled by zoom then offset by panOffset.
            panOffset = (panOffset - screenPoint) * finalScale + screenPoint;

            zoom = newZoom;
            Repaint();
        }
        #endregion

        #region Node Click Handling
        /// <summary>
        /// Called when user clicks somewhere on the canvas. Checks whether a node was clicked and opens its script.
        /// </summary>
        /// <param name="mousePosition">Mouse position in GUI coordinates (screen space)</param>
        private void OnNodeClicked(Vector2 mousePosition)
        {
            if (positions == null || positions.Count == 0)
                return;

            // iterate in reverse to prioritize nodes drawn later (in case of overlap) — although order is arbitrary here
            foreach (var pair in positions.Reverse())
            {
                Node node = pair.Key;
                Vector2 screenPos = pair.Value * zoom + panOffset;
                Rect rect = GetNodeRect(node, screenPos);

                if (rect.Contains(mousePosition))
                {
                    OpenNodeScriptForNode(node);
                    return;
                }
            }
        }

        /// <summary>
        /// Attempts to find and open the MonoScript corresponding to the node's class.
        /// Scans project MonoScripts and compares GetClass().
        /// </summary>
        /// <param name="node">Node instance</param>
        private void OpenNodeScriptForNode(Node node)
        {
            if (node == null)
                return;

            Type nodeType = node.GetType();

            // Search all MonoScript assets and compare class
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (mono == null)
                    continue;

                var scriptClass = mono.GetClass();
                if (scriptClass == nodeType)
                {
                    // Open the script asset in the code editor
                    AssetDatabase.OpenAsset(mono);
                    return;
                }
            }

            // Fallback: if not found, show a dialog
            EditorUtility.DisplayDialog("Script not found", $"C# script file for node type '{nodeType.FullName}' not found in the project assets.", "OK");
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
            else if (node is DoWhile doWhile)
            {
                float actionWidth = GetSubtreeWidth(doWhile.ActionNode);
                float conditionWidth = GetSubtreeWidth(doWhile.ConditionNode);
                return Mathf.Max(size.x, actionWidth + conditionWidth + GetDynamicSpacing(doWhile.ActionNode));
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
            else if (node is DoWhile doWhile)
            {
                float actionWidth = GetSubtreeWidth(doWhile.ActionNode);
                float conditionWidth = GetSubtreeWidth(doWhile.ConditionNode);
                float totalWidth = actionWidth + conditionWidth + GetDynamicSpacing(doWhile.ActionNode);

                float x = origin.x - totalWidth / 2f;

                // Condition first (left)
                float conditionX = x + conditionWidth / 2f;
                Layout(doWhile.ConditionNode, new Vector2(conditionX, origin.y + size.y + Mathf.Max(verticalSpacing, minVerticalSpacing)));

                x += conditionWidth + GetDynamicSpacing(doWhile.ActionNode);

                // Action second (right)
                float actionX = x + actionWidth / 2f;
                Layout(doWhile.ActionNode, new Vector2(actionX, origin.y + size.y + Mathf.Max(verticalSpacing, minVerticalSpacing)));

                float midX = (positions[doWhile.ConditionNode].x + positions[doWhile.ActionNode].x) / 2f;
                positions[node] = new Vector2(midX, origin.y);
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

            // When zoomed, scale the logical center before centering.
            panOffset = canvasCenter - center * zoom;
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

        private List<(Vector2 fromNodeSize, Vector2 toNodeSize, Vector2 from, Vector2 to, Color color, float width, bool highlighted)> connectionsBuffer = new();

        /// <summary>
        /// Draws all connections between nodes; highlights lines that lead to active nodes.
        /// Uses orthogonal (90°) segments instead of Bezier curves.
        /// </summary>
        private void DrawConnections()
        {
            connectionsBuffer.Clear();

            foreach (var pair in positions)
            {
                var startNodeSize = GetNodeSize(pair.Key);

                if (pair.Key is Composite comp)
                {
                    foreach (var child in comp.Children)
                    {
                        var childNodeSize = GetNodeSize(child);
                        if (positions.TryGetValue(child, out var childPos))
                        {
                            bool isActive = child.State != NodeState.NotActive;
                            // preserve the same color logic as before (GetStateColor for active)
                            Color lineColor = isActive ? BehaviorTreeConfig.Instance.activeLineColor : BehaviorTreeConfig.Instance.defaultLineColor;
                            // width: try to use config values if present, fallback to literals

                            float lineWidth = (isActive ? BehaviorTreeConfig.Instance.activeLineWidth : BehaviorTreeConfig.Instance.defaultLineWidth) * Mathf.Max(1f, zoom);

                            connectionsBuffer.Add((startNodeSize, childNodeSize, pair.Value, childPos, lineColor, lineWidth, isActive));
                        }
                    }
                }
                else if (pair.Key is Decorator dec && dec.Child != null)
                {
                    var childNodeSize = GetNodeSize(dec.Child);
                    if (positions.TryGetValue(dec.Child, out var childPos))
                    {
                        bool isActive = dec.Child.State != NodeState.NotActive;
                        Color lineColor = isActive ? BehaviorTreeConfig.Instance.activeLineColor : BehaviorTreeConfig.Instance.defaultLineColor;

                        float lineWidth = (isActive ? BehaviorTreeConfig.Instance.activeLineWidth : BehaviorTreeConfig.Instance.defaultLineWidth) * Mathf.Max(1f, zoom);

                        connectionsBuffer.Add((startNodeSize, childNodeSize, pair.Value, childPos, lineColor, lineWidth, isActive));
                    }
                }
                else if (pair.Key is DoWhile doWhile)
                {
                    // Draw connection to Condition
                    var conditionSize = GetNodeSize(doWhile.ConditionNode);
                    if (positions.TryGetValue(doWhile.ConditionNode, out var conditionPos))
                    {
                        bool isActive = doWhile.ConditionNode.State != NodeState.NotActive;
                        Color lineColor = isActive ? BehaviorTreeConfig.Instance.activeLineColor : BehaviorTreeConfig.Instance.defaultLineColor;
                        float lineWidth = (isActive ? BehaviorTreeConfig.Instance.activeLineWidth : BehaviorTreeConfig.Instance.defaultLineWidth) * Mathf.Max(1f, zoom);
                        connectionsBuffer.Add((startNodeSize, conditionSize, pair.Value, conditionPos, lineColor, lineWidth, isActive));
                    }

                    // Draw connection to Action
                    var actionSize = GetNodeSize(doWhile.ActionNode);
                    if (positions.TryGetValue(doWhile.ActionNode, out var actionPos))
                    {
                        bool isActive = doWhile.ActionNode.State != NodeState.NotActive;
                        Color lineColor = isActive ? BehaviorTreeConfig.Instance.activeLineColor : BehaviorTreeConfig.Instance.defaultLineColor;
                        float lineWidth = (isActive ? BehaviorTreeConfig.Instance.activeLineWidth : BehaviorTreeConfig.Instance.defaultLineWidth) * Mathf.Max(1f, zoom);
                        connectionsBuffer.Add((startNodeSize, actionSize, pair.Value, actionPos, lineColor, lineWidth, isActive));
                    }
                }
            }

            foreach (var c in connectionsBuffer)
            {
                if (c.highlighted)
                    continue;

                DrawConnection(c.fromNodeSize, c.toNodeSize, c.from, c.to, c.color, c.width, c.highlighted);
            }

            foreach (var c in connectionsBuffer)
            {
                if (!c.highlighted)
                    continue;

                DrawConnection(c.fromNodeSize, c.toNodeSize, c.from, c.to, c.color, c.width, c.highlighted);
            }
        }

        /// <summary>
        /// Draws an orthogonal (right-angle) connection between two nodes.
        /// If highlighted==true, draws a glow (wider translucent line) then the main line.
        /// </summary>
        private void DrawConnection(Vector2 fromNodeSize, Vector2 toNodeSize, Vector2 from, Vector2 to, Color color, float width, bool highlighted)
        {
            // Convert logical positions/sizes to screen space
            Vector2 start = from * zoom + panOffset + new Vector2((fromNodeSize.x * zoom) / 2f, fromNodeSize.y * zoom);
            Vector2 end = to * zoom + panOffset + new Vector2((toNodeSize.x * zoom) / 2f, 0);

            // Compute middle Y for the horizontal segment (halfway between start and end)
            float midY = (start.y + end.y) * 0.5f;

            // Build orthogonal polyline: start -> (start.x, midY) -> (end.x, midY) -> end
            Vector3 p0 = start;
            Vector3 p1 = new Vector3(start.x, midY, 0f);
            Vector3 p2 = new Vector3(end.x, midY, 0f);
            Vector3 p3 = end;

            // Remember previous Handles.color and restore later
            Color prevHandlesColor = Handles.color;

            // If highlighted, draw a glow/backline first using same hue but reduced alpha
            if (highlighted && BehaviorTreeConfig.Instance.addGlowToActiveLines)
            {
                float glowAlpha = Mathf.Clamp01(BehaviorTreeConfig.Instance.activeLineGlowTransparency);
                Color glowColor = new Color(color.r, color.g, color.b, glowAlpha);
                float glowWidth = Mathf.Max(1f, width * BehaviorTreeConfig.Instance.activeLineGlowWidth);

                Handles.color = glowColor;
                Handles.DrawAAPolyLine(glowWidth, p0, p1, p2, p3);
            }

            // Draw the main line with full color
            Handles.color = color;
            Handles.DrawAAPolyLine(Mathf.Max(1f, width), p0, p1, p2, p3);

            // Restore previous color
            Handles.color = prevHandlesColor;
        }

        /// <summary>
        /// Draws all nodes.
        /// </summary>
        private void DrawNodes()
        {
            foreach (var pair in positions)
            {
                // Convert logical position -> screen position
                Vector2 screenPos = pair.Value * zoom + panOffset;
                DrawNode(pair.Key, screenPos);
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
            DrawGrid(canvasRect, BehaviorTreeConfig.Instance.gridTexture, zoom, panOffset);
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
            Vector2 size = GetNodeSize(node) * zoom;
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

            HeaderStyle.fontSize = Mathf.RoundToInt(zoom * BehaviorTreeConfig.Instance.headerFontSize);
            BodyStyle.fontSize = Mathf.RoundToInt(zoom * BehaviorTreeConfig.Instance.bodyFontSize);

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
        /// Helper method for drawing a texture with color.
        /// </summary>
        public static void DrawTexture(Rect r, Texture2D tex, Color c)
        {
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true, 0f, c, 0f, 0f);
        }

        /// <summary>
        /// Draw a tiled grid that can be scaled and translated.
        /// </summary>
        /// <param name="canvas">The area to draw the grid</param>
        /// <param name="texture">The grid tile texture</param>
        /// <param name="zoom">Scales the grid by zoom amount</param>
        /// <param name="pan">Translates the grid pan amount</param>
        public static void DrawGrid(Rect canvas, Texture texture, float zoom, Vector2 pan)
        {
            var size = canvas.size;
            var center = size / 2f;

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + pan.x) / texture.width;
            float yOffset = ((center.y - size.y) * zoom + pan.y) / texture.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(size.x * (1f / zoom)) / texture.width;
            float tileAmountY = Mathf.Round(size.y * (1f / zoom)) / texture.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
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

        private void DrawZoomLabel()
        {
            var rect = new Rect(position.width - 120f, 10f, 110f, 20f);
            GUI.Label(rect, $"Zoom: {Mathf.RoundToInt(zoom * 100f)}%", EditorStyles.whiteLabel);
        }
        #endregion
    }
}
#endif
