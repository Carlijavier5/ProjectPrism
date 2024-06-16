using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TileColliderTool {

        private Vector3Int pivotEdgePos;
        private Vector3Int pivotNormal;
        private Vector3Int potentialPivot;

        private bool onPivotCollider;
        private bool pivotSelected;
        private int pivotOffset;

        private double wheelCD;
        private double WheelCD {
            get => EditorApplication.timeSinceStartup > wheelCD ? -1 : 1;
            set { wheelCD = EditorApplication.timeSinceStartup + value; }
        }

        private void HighlightPivotTarget() {
            if (Event.current.type == EventType.Repaint) {
                if (onPivotCollider) {
                    Vector3Int center = pivotEdgePos + pivotNormal * pivotOffset;
                    if (pivotSelected) {
                        HandleUtils.DrawOctohedralVolume(center, Vector3.one,
                                                         new Vector4(1, 0, 0, 0.25f),
                                                         Color.white);
                    } else {
                        using (new Handles.DrawingScope(UIColors.Blue)) {
                            Handles.DrawWireCube(center, Vector3.one * 0.75f);
                        }
                    }
                } HandleUtils.DrawDottedOctohedron(Info.SelectedCollider.Pivot,
                                                   Vector3.one, Color.white, 5f);
                using (new Handles.DrawingScope(Color.red)) {
                    Handles.SphereHandleCap(0, Info.SelectedCollider.Pivot,
                                        Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
        }
        
        private void DoLocalPivotSelector() {
            if (Event.current.button > 0) return;
            switch (Event.current.type) {
                case EventType.MouseMove:
                    if (RaycastCD > 0) {
                        pendingCast = true;
                    } else {
                        RaycastCD = 0.1f;
                        DoPivotSignal(EventType.MouseMove);
                    } break;
                case EventType.MouseUp:
                case EventType.MouseDown:
                    DoPivotSignal(Event.current.type);
                    break;
                case EventType.Repaint:
                    if (RaycastCD < 0 && pendingCast) {
                        DoPivotSignal(EventType.MouseMove);
                    } break;
                case EventType.ScrollWheel:
                    if (!onPivotCollider) return;
                    if (WheelCD < 0) {
                        DoOffsetScroll(Event.current.delta.y);
                        WheelCD = 0.075;
                    } Event.current.Use();
                    break;
            }
        }

        private void DoPivotSignal(EventType eventType) {
            pendingCast = false;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Info.SelectedCollider.collider.Raycast(ray, out RaycastHit hit, 500f)) {
                pivotNormal = hit.normal.Round();
                onPivotCollider = true;
                switch (eventType) {
                    case EventType.MouseMove:
                        pivotEdgePos = (hit.point + ((Vector3) pivotNormal / 2f)).Round();
                        break;
                    case EventType.MouseDown:
                        potentialPivot = pivotEdgePos + pivotNormal * pivotOffset;
                        pivotSelected = true;
                        break;
                    case EventType.MouseUp:
                        if (pivotSelected) {
                            toolMode = ToolMode.Move;
                            Info.SelectedCollider.ShiftPivot(potentialPivot);
                        } ResetPivot();
                        RaycastCD = 0.5f;
                        break;
                }
            } else onPivotCollider = false;
        }

        private void DoOffsetScroll(float delta) {
            int offset = pivotOffset + (int) Mathf.Sign(delta);
            if (Info.SelectedCollider.collider
                    .bounds.Contains(pivotEdgePos + pivotNormal * offset)) {
                pivotOffset = offset;
            }
        }

        private void ResetPivot() {
            onPivotCollider = false;
            pivotSelected = false;
            pivotOffset = -1;
        }
    }
}