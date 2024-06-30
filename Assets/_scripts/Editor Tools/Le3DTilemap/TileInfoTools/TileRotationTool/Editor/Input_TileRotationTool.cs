using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TileRotationTool {

        private void HighlightPivots() {
            if (Event.current.type == EventType.Repaint) {
                if (Info.MeshRoot) {
                    using (new Handles.DrawingScope(Color.green)) {
                        Handles.SphereHandleCap(0, Info.MeshRoot.position,
                                                Quaternion.identity, 0.1f, EventType.Repaint);
                    }
                } DrawRotationAxis();
                using (new Handles.DrawingScope(Color.red)) {
                    Handles.SphereHandleCap(0, Info.transform.position,
                                            Quaternion.identity, 0.1f, EventType.Repaint);
                } 
            }
        }

        private void DrawRotationAxis() {
            Vector3 normal;
            Color color;
            switch (gridOrientation) {
                case GridOrientation.XZ:
                    color = Handles.yAxisColor;
                    normal = Vector3.up;
                    break;
                case GridOrientation.XY:
                    normal = Vector3.forward;
                    color = Handles.zAxisColor;
                    break;
                case GridOrientation.YZ:
                    normal = Vector3.right;
                    color = Handles.xAxisColor;
                    break;
                default:
                    normal = Vector3.zero;
                    color = Color.white;
                    break;
            } using (new Handles.DrawingScope(color)) {
                Vector3 origin = Info.transform.position - normal * 0.5f;
                Vector3 end = Info.transform.position + normal * 0.5f;
                Handles.DrawWireDisc(origin, normal, 0.25f, 2.5f);
                Handles.DrawWireDisc(end, normal, 0.25f, 2.5f);
                Handles.DrawLine(origin, end, 2.5f);
            }
        }

        private void DoToolInput() {
            if (Event.current.type == EventType.ScrollWheel) {
                if (WheelCD < 0) {
                    DoRotateScroll(Event.current.delta.y);
                } Event.current.Use();
            } /// Update Handles, if needed;
        }

        private void DoRotateScroll(float delta) {
            Vector3Int normal = gridOrientation switch {
                GridOrientation.XZ => Vector3Int.up,
                GridOrientation.XY => Vector3Int.forward,
                GridOrientation.YZ => Vector3Int.right,
                _ => Vector3Int.zero,
            }; Info.RotateTilespace(normal, delta, 
                                    settings.rotatesColliders,
                                    settings.rotatesMesh);
        }
    }
}