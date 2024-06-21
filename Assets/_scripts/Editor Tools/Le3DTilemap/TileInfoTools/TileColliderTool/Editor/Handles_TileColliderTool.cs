using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public partial class TileColliderTool {
        private class ColliderHandles {
            public readonly TileCollider collider;
            private readonly ArrowHandle[] arrowHandles;
            private readonly Transform info;

            public ColliderHandles(Transform info, TileCollider collider) {
                this.collider = collider;
                this.info = info;
                arrowHandles = new[] { new ArrowHandle(collider, GUIUtility.GetControlID(10, FocusType.Keyboard),
                                                       info, Vector3Int.left, Handles.xAxisColor),
                                       new ArrowHandle(collider, GUIUtility.GetControlID(11, FocusType.Keyboard),
                                                       info, Vector3Int.right, Handles.xAxisColor),
                                       new ArrowHandle(collider, GUIUtility.GetControlID(12, FocusType.Keyboard),
                                                       info, Vector3Int.up, Handles.yAxisColor),
                                       new ArrowHandle(collider, GUIUtility.GetControlID(13, FocusType.Keyboard),
                                                       info, Vector3Int.down, Handles.yAxisColor),
                                       new ArrowHandle(collider, GUIUtility.GetControlID(14, FocusType.Keyboard),
                                                       info, Vector3Int.back, Handles.zAxisColor),
                                       new ArrowHandle(collider, GUIUtility.GetControlID(15, FocusType.Keyboard),
                                                       info, Vector3Int.forward, Handles.zAxisColor) };
            }

            public void DoHandles(ref int activeID, ToolMode toolMode) {
                if (collider == null || collider.collider == null) return;
                switch (toolMode) {
                    case ToolMode.Move:
                        Vector3 pivot = info.TransformPoint(collider.Pivot);
                        Vector3 pos = Handles.DoPositionHandle(pivot, Quaternion.identity);
                        Vector3Int newPivot = info.InverseTransformPoint(pos).Round();
                        if (newPivot != collider.Pivot) {
                            collider.Pivot = newPivot;
                            CenterHandles();
                        } break;
                    case ToolMode.Scale:
                        foreach (ArrowHandle handle in arrowHandles) handle.DoHandle(ref activeID);
                        break;
                }
            }

            public void CenterHandles() {
                foreach (ArrowHandle handle in arrowHandles) {
                    handle.CenterPos();
                }
            }
        } private ColliderHandles activeHandles;

        private void UpdateHandles() {
            if (activeHandles != null) {
                activeHandles.CenterHandles();
            }
        }

        private class ArrowHandle {

            private const float OFFSET = 0.5f;
            private readonly TileCollider collider;
            private readonly int id;
            private readonly Transform info;
            private readonly Vector3Int normal;
            private readonly Color color;
            private Vector3 mousePos;
            private Vector3 pos;

            public ArrowHandle(TileCollider collider, int id,
                               Transform info, Vector3Int normal, Color color) {
                this.collider = collider;
                this.id = id;
                this.info = info;
                this.normal = normal;
                this.color = color;
                CenterPos();
            }

            public void DoHandle(ref int activeID) {
                UpdatePos();
                Vector3 offsetPos = pos - (Vector3) normal * OFFSET;
                Vector3 prevPos = pos;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = HandleUtility.nearestControl == id ? activeID == id ? Color.yellow
                                                                                    : color * 1.1f
                                                                   : color;
                Handles.DrawSolidDisc(offsetPos, normal,
                                      Mathf.Max(0.04f, HandleUtility.GetHandleSize(offsetPos) * 0.04f));
                HandleUtils.DrawArrowHandle(id, normal, Mathf.Max(0.5f, HandleUtility.GetHandleSize(offsetPos)),
                                            OFFSET, color, ref pos, ref activeID, ref mousePos, RoundCallback);
                Vector3Int newIntPos = pos.Round();
                Vector3Int prevIntPos = prevPos.Round();
                if ((collider.Size * normal).magnitude == 1
                    && SizeDelta(prevIntPos, newIntPos) < 0) { /// Prevents handle from going out of bounds;
                    pos = prevPos;
                } else if (prevIntPos != newIntPos) {
                    collider.Resize((newIntPos - prevIntPos) * normal.Abs(),
                                    (newIntPos - prevIntPos) * normal);
                }
            }

            public void CenterPos() {
                Vector3 center = info.TransformPoint(collider.Center);
                pos = center + (Vector3) normal * OFFSET
                      + VectorUtils.Mult((Vector3) collider.Size / 2f, normal);
            }

            public void UpdatePos() {
                /// Isolate normal axis;
                pos = VectorUtils.Mult(pos, normal.Abs());
                /// Remove normal axis from collider pivot;
                Vector3 center = info.TransformPoint(collider.Center);
                center -= VectorUtils.Mult(center, normal.Abs());
                /// New pos preserves the normal axis and inherits pivot values;
                pos += center;
            }

            private float SizeDelta(Vector3Int oldPos, Vector3Int newPos) {
                float oldSize = collider.Size.magnitude;
                float newSize = (collider.Size - (newPos - oldPos) * normal).magnitude;
                return oldSize - newSize;
            }

            private void RoundCallback() => pos = pos.Round();
        }
    }
}