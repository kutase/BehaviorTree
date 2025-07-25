using System;
using Bonsai.Designer;
using Plugins.Behavior_Tree.Runtime.New.Configs;
using Plugins.Behavior_Tree.Runtime.New.Nodes;
using Plugins.Behavior_Tree.Runtime.New.Nodes.Composites;
using Plugins.Behavior_Tree.Runtime.New.Nodes.Decorators;

namespace Plugins.Behavior_Tree.Runtime.New.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    public class BehaviorTreeEditorWindow : EditorWindow
    {
        private Vector2 panOffset = Vector2.zero;
        private Vector2 dragStartPos;
        private bool dragging = false;

        private Dictionary<Node, Vector2> positions = new();
        private Dictionary<Node, Vector2> nodeSizes = new();

        private BehaviorTree currentTree;

        private bool firstCenter = true;

        private readonly float verticalSpacing = 100;
        private readonly float horizontalSpacing = 30;

        #region Styles
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

        private GUIStyle HeaderStyle;
        private GUIStyle BodyStyle;

        private void InitStyles()
        {
            if (HeaderStyle == null)
                HeaderStyle = CreateHeaderStyle();

            if (BodyStyle == null)
                BodyStyle = CreateBodyStyle();
        }

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

        Vector2 GetNodeSize(Node node)
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

        #endregion

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

        private void PlayModeStateChanged(PlayModeStateChange obj)
        {
            OnSelectionChange();
        }

        [MenuItem("Window/Behavior Tree Viewer")]
        public static void OpenWindow()
        {
            var window = CreateInstance<BehaviorTreeEditorWindow>();
            window.titleContent = new GUIContent("Behavior Tree");
            window.Show();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var runner = Selection.activeGameObject.GetComponent<BehaviorTreeRunner>();
                if (runner != null)
                {
                    currentTree = runner.Tree;
                }
                else
                {
                    currentTree = null;
                }
            }

            positions.Clear();
            nodeSizes.Clear();
            firstCenter = true;

            Repaint();
        }

        private void OnGUI()
        {
            HandleInput();

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

        private void HandleInput()
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

        private void Layout(Node node, Vector2 origin)
        {
            float width = 0f;
            float maxHeight = 0f;

            if (node is Composite composite && composite.Children.Count > 0)
            {
                List<Vector2> childPositions = new();
                float xOffset = 0f;

                foreach (var child in composite.Children)
                {
                    var childNodeSize = GetNodeSize(child);

                    Layout(child, origin + new Vector2(xOffset, verticalSpacing));
                    Vector2 childPos = positions[child];
                    childPositions.Add(childPos);
                    xOffset += childNodeSize.x + horizontalSpacing;
                }

                float midX = (childPositions.First().x + childPositions.Last().x) / 2f;
                positions[node] = new Vector2(midX, origin.y);
            }
            else if (node is Decorator decorator && decorator.Child != null)
            {
                Layout(decorator.Child, origin + new Vector2(0, verticalSpacing));
                Vector2 childPos = positions[decorator.Child];
                positions[node] = new Vector2(childPos.x, origin.y);
            }
            else
            {
                positions[node] = origin;
            }
        }

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
            float tileAmountX = Mathf.Round(size.x * zoom) / texture.width;
            float tileAmountY = Mathf.Round(size.y * zoom) / texture.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(canvas, texture, new Rect(tileOffset, tileAmount));
        }

        private void DrawGrid(Vector2 size)
        {
            var canvasRect = new Rect(Vector2.zero, size);
            DrawGrid(canvasRect, BehaviorTreeConfig.Instance.gridTexture, 1f, panOffset);
        }

        private void DrawCanvas()
        {
            Handles.BeginGUI();

            DrawGrid(position.size);

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

            Handles.EndGUI();

            foreach (var pair in positions)
            {
                DrawNode(pair.Key, pair.Value + panOffset);
            }
        }
        
        // Helper method to draw textures with color tint.
        public static void DrawTexture(Rect r, Texture2D tex, Color c)
        {
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true, 0f, c, 0f, 0f);
        }

        private void DrawNode(Node node, Vector2 pos)
        {
            // Рассчитываем размер узла на основе содержимого
            Vector2 headerSize = HeaderStyle.CalcSize(node.HeaderContent);
            Vector2 bodySize = BodyStyle.CalcSize(node.BodyContent);
    
            // Добавляем отступы
            float padding = 10f;
            float width = Mathf.Max(headerSize.x, bodySize.x) + padding * 2;
            float height = headerSize.y + bodySize.y + padding * 3;
    
            Rect rect = new(pos.x, pos.y, width, height);
    
            DrawNodeBackground(node, rect);

            DrawNodeStatus(node, rect);

            DrawNodeContent(node, rect, headerSize, bodySize);
        }

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

        private void DrawNodeContent(Node node, Rect rect, Vector2 headerSize, Vector2 bodySize)
        {
            // Создаем контейнер с вертикальным расположением элементов
            GUILayout.BeginArea(rect);
            {
                GUILayout.BeginVertical();
                {
                    // Заголовок с авто-подбором размера текста
                    GUILayout.Label(node.HeaderContent, HeaderStyle, 
                        GUILayout.ExpandWidth(true), 
                        GUILayout.Height(headerSize.y));
            
                    // Тело с авто-подбором размера текста
                    GUILayout.Label(node.BodyContent, BodyStyle, 
                        GUILayout.ExpandWidth(true), 
                        GUILayout.Height(bodySize.y));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }

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
        }

        private void DrawConnection(Vector2 fromNodeSize, Vector2 toNodeSize, Vector2 from, Vector2 to)
        {
            Vector3 start = from + panOffset + new Vector2(fromNodeSize.x / 2f, fromNodeSize.y);
            Vector3 end = to + panOffset + new Vector2(toNodeSize.x / 2f, 0);

            Vector3 startTan = start + Vector3.up * 30f;
            Vector3 endTan = end + Vector3.down * 30f;

            Handles.DrawBezier(start, end, startTan, endTan, Color.gray, null, 3f);
        }
    }
}