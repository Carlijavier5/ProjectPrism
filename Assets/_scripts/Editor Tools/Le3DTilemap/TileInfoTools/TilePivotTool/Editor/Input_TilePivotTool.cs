using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {

    public partial class TilePivotTool {

        private Vector3Int hintTile;
        private Vector3Int pickedTile;
        private bool hasPicked;
        private bool hasHint;

        private double raycastCD;
        private double RaycastCD {
            get => EditorApplication.timeSinceStartup > raycastCD ? -1 : 1;
            set { raycastCD = EditorApplication.timeSinceStartup + value; }
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
                } HandleUtils.DrawDottedOctohedron(Info.transform.position,
                                                   Vector3.one, Color.white, 5f);
                using (new Handles.DrawingScope(Color.red)) {
                    Handles.SphereHandleCap(0, Info.transform.position,
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
            if (enter >= 0 && enter <= settings.raycastDistance) {
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
                            Info.TranslatePivot(pickedTile, false);
                        } ResetSelection();
                        break;
                }
            } else hasHint = false;
        }

        private void ResetSelection() {
            hasHint = false;
            hasPicked = false;
        }
    }
}