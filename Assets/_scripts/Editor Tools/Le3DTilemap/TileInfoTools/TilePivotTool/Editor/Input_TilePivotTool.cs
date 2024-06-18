using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {

    public partial class TilePivotTool {

        private const float OFFSET = 0.5f;

        private GridInputMode defaultInput;
        private GridInputMode overrideInput;

        private Vector3Int hintTile;
        private Vector3Int pickedTile;
        private bool hasPicked;
        private bool hasHint;

        private Plane plane;
        private int depth;

        private double raycastCD;
        private double RaycastCD {
            get => EditorApplication.timeSinceStartup > raycastCD ? -1 : 1;
            set { raycastCD = EditorApplication.timeSinceStartup + value; }
        }
        private bool pendingCast;

        private double wheelCD;
        private double WheelCD {
            get => EditorApplication.timeSinceStartup > wheelCD ? -1 : 1;
            set { wheelCD = EditorApplication.timeSinceStartup + value; }
        }

        private void HighlightHintTile() {
            if (Event.current.type == EventType.Repaint) {
                if (hasHint) {
                    if (hasPicked) {
                        HandleUtils.DrawOctohedralVolume(pickedTile, Vector3.one,
                                                         new Vector4(1, 0, 0, 0.25f),
                                                         Color.white);
                    } else {
                        using (new Handles.DrawingScope(UIColors.Blue)) {
                            Handles.DrawWireCube(hintTile, Vector3.one * 0.75f);
                        }
                    }
                } HandleUtils.DrawDottedOctohedron(Info.TilePivot,
                                                   Vector3.one, Color.white, 5f);
                using (new Handles.DrawingScope(Color.red)) {
                    Handles.SphereHandleCap(0, Info.TilePivot,
                                            Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
        }

        private void DoSelectionInput() {
            switch (Event.current.type) {
                case EventType.MouseMove:
                    if (RaycastCD > 0) {
                        pendingCast = true;
                    } else {
                        RaycastCD = 0.005f;
                        DoSelectionSignal(EventType.MouseMove);
                    } break;
                case EventType.MouseUp:
                case EventType.MouseDown:
                    if (Event.current.button > 0) return;
                    DoSelectionSignal(Event.current.type);
                    break;
                case EventType.Repaint:
                    if (RaycastCD < 0 && pendingCast) {
                        DoSelectionSignal(EventType.MouseMove);
                    } break;
            }
        }

        private void DoSelectionSignal(EventType eventType) {
            pendingCast = false;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            plane.Raycast(ray, out float enter);
            if (enter >= 0) {
                hasHint = true;
                Vector3Int hitTile = ray.GetPoint(enter).Round();
                switch (eventType) {
                    case EventType.MouseMove:
                        hintTile = hitTile;
                        break;
                    case EventType.MouseDown:
                        hasPicked = true;
                        pickedTile = hitTile;
                        break;
                    case EventType.MouseUp:
                        if (hasPicked
                            && hitTile == pickedTile) {
                            Info.TilePivot = pickedTile;
                        } ResetSelection();
                        break;
                }
            } else hasHint = false;
        }

        private void DoScrollInput(SceneView sceneView) {
            if (Event.current.type == EventType.ScrollWheel) {
                if (WheelCD < 0) {
                    DoOffsetScroll(sceneView, Event.current.delta.y);
                } Event.current.Use();
            }
        }

        private void DoOffsetScroll(SceneView sceneView, float delta) {
            GridInputMode activeMode = overrideInput > 0 ? overrideInput
                                                         : defaultInput;
            int intDelta = (int) Mathf.Sign(delta);
            switch (activeMode) {
                case GridInputMode.Move:
                    if (this.orientation switch { 
                            GridOrientation.XZ
                            => sceneView.camera.transform.position.y,
                            GridOrientation.XY
                            => sceneView.camera.transform.position.z,
                            _
                            => sceneView.camera.transform.position.x,
                        } < depth) {
                        intDelta *= -1;
                    } depth += intDelta;
                    pendingCast = true;
                    UpdateGridDepth();
                    WheelCD = 0.075;
                    break;
                case GridInputMode.Turn:
                    int orientation = ((int) this.orientation + intDelta) % 3;
                    orientation = (orientation < 0) ? 3 + orientation : orientation;
                    SetGridOrientation((GridOrientation) orientation);
                    WheelCD = 0.2;
                    break;
            }
        }

        private void DoInputOverrides() {
            if (Event.current.type == EventType.KeyDown) {
                switch (Event.current.keyCode) {
                    case KeyCode.LeftControl:
                        overrideInput = GridInputMode.Move;
                        Event.current.Use();
                        break;
                    case KeyCode.R:
                        overrideInput = GridInputMode.Turn;
                        Event.current.Use();
                        break;
                }
            } else if (Event.current.type == EventType.KeyUp) {
                overrideInput = GridInputMode.None;
            }
        }

        private void UpdateGridDepth() {
            Vector3 normal;
            Vector3 nonNormalAxis;
            switch (orientation) {
                case GridOrientation.XZ:
                    normal = Vector3.up;
                    nonNormalAxis = new Vector3(gridQuad.Position.x,
                                                0, gridQuad.Position.z);
                    break;
                case GridOrientation.XY:
                    normal = Vector3.forward;
                    nonNormalAxis = new Vector3(gridQuad.Position.x,
                                                gridQuad.Position.y, 0);
                    break;
                default:
                    normal = Vector3.right;
                    nonNormalAxis = new Vector3(0, gridQuad.Position.y,
                                                gridQuad.Position.z);
                    break;
            } gridQuad.Position = nonNormalAxis + normal * (depth - OFFSET);
            plane = new Plane(normal, nonNormalAxis + normal * depth);
        }

        private void UpdateGridPos(SceneView sceneView) {
            if (Event.current.type == EventType.Repaint) {
                Vector3Int center = (settings.followCamera ? sceneView.camera.transform.position
                                                           : Vector3.zero).Round();
                float depth = this.depth - OFFSET;
                switch (orientation) {
                    case GridOrientation.XZ:
                        gridQuad.Position = new Vector3(center.x, depth, center.z);
                        break;
                    case GridOrientation.XY:
                        gridQuad.Position = new Vector3(center.x, center.y, depth);
                        break;
                    case GridOrientation.YZ:
                        gridQuad.Position = new Vector3(depth, center.y, center.z);
                        break;
                }
            }
        }

        private void ResetSelection() {
            hasHint = false;
            hasPicked = false;
        }
    }
}