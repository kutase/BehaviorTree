#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Plugins.BehaviorTree.Runtime.Editor
{
    public enum PanMouseButton
    {
        Mouse0 = 0,
        Mouse1 = 1,
        MouseWheelClick = 2
    }

    /// <summary>
    /// Viewport navigation settings for the Behavior Tree Viewer editor window.
    /// </summary>
    public class BehaviorTreeEditorViewSettings
    {
        private const string PrefsPrefix = "BehaviorTree.EditorView.";
        private const string PanButtonKey = PrefsPrefix + "PanButton";
        private const string InvertZoomKey = PrefsPrefix + "InvertZoom";
        private const string PanSpeedKey = PrefsPrefix + "PanSpeed";
        private const string ZoomSpeedKey = PrefsPrefix + "ZoomSpeed";

        public const float DefaultPanSpeed = 1f;
        public const float DefaultZoomSpeed = 1f;
        public const float MinSpeed = 0.1f;
        public const float MaxSpeed = 3f;

        public PanMouseButton PanButton { get; set; } = PanMouseButton.Mouse0;
        public bool InvertZoom { get; set; }
        public float PanSpeed { get; set; } = DefaultPanSpeed;
        public float ZoomSpeed { get; set; } = DefaultZoomSpeed;

        public void ApplyDefaults()
        {
            PanButton = PanMouseButton.Mouse0;
            InvertZoom = false;
            PanSpeed = DefaultPanSpeed;
            ZoomSpeed = DefaultZoomSpeed;
        }

        public void Load()
        {
            if (!EditorPrefs.HasKey(PanButtonKey))
            {
                ApplyDefaults();
                return;
            }

            PanButton = (PanMouseButton)EditorPrefs.GetInt(PanButtonKey, (int)PanMouseButton.Mouse0);
            InvertZoom = EditorPrefs.GetBool(InvertZoomKey, false);
            PanSpeed = EditorPrefs.GetFloat(PanSpeedKey, DefaultPanSpeed);
            ZoomSpeed = EditorPrefs.GetFloat(ZoomSpeedKey, DefaultZoomSpeed);
            ClampSpeeds();
        }

        public void Save()
        {
            ClampSpeeds();
            EditorPrefs.SetInt(PanButtonKey, (int)PanButton);
            EditorPrefs.SetBool(InvertZoomKey, InvertZoom);
            EditorPrefs.SetFloat(PanSpeedKey, PanSpeed);
            EditorPrefs.SetFloat(ZoomSpeedKey, ZoomSpeed);
        }

        public void ResetToDefaults()
        {
            ApplyDefaults();
        }

        private void ClampSpeeds()
        {
            PanSpeed = Mathf.Clamp(PanSpeed, MinSpeed, MaxSpeed);
            ZoomSpeed = Mathf.Clamp(ZoomSpeed, MinSpeed, MaxSpeed);
        }
    }
}
#endif
