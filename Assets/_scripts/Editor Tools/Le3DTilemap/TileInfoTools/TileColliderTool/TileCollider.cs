#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    [System.Serializable]
    public class TileCollider {

        public char name;
        public TileInfo info;
        public BoxCollider collider;

        [SerializeField] private Vector3Int pivot;
        public Vector3Int Pivot {
            get => pivot;
            set {
                if (value != pivot) {
                    int undoGroup = UndoUtils.StartUndoGroup("Move (Tile Collider)");
                    UndoUtils.RecordScopeUndo(collider, "Move (Box Collider)");
                    collider.center += value - pivot;
                    UndoUtils.RecordScopeUndo(info, "Move (Tile Collider)");
                    pivot = value;
                    info.RecordTilespaceChange();
                    Undo.CollapseUndoOperations(undoGroup);
                }
            }
        }
        public Vector3Int Size {
            get => collider.size.Round();
            set => collider.size.Round();
        }
        public Vector3 Center => collider.center;
        public TileCollider(TileInfo info, char name) {
            this.info = info;
            this.name = name;

            HideFlags ogFlags = info.gameObject.hideFlags;
            info.gameObject.hideFlags = HideFlags.None;
            collider = info.gameObject.AddComponent<BoxCollider>();
            Undo.RegisterCreatedObjectUndo(collider, "Create Component (Box Collider)");
            info.gameObject.hideFlags = ogFlags;
            
            collider.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
        }

        public void Resize(Vector3Int diffCenter, Vector3Int diffSize) {
            int undoGroup = UndoUtils.StartUndoGroup("Resize (Tile Collider)");
            UndoUtils.RecordScopeUndo(collider, "Resize (Tile Collider)");
            collider.center += (Vector3) diffCenter / 2f;
            collider.size += diffSize;
            /// Normalize the direction of the size change;
            Vector3Int sizeDeltaNormal = diffSize.Normalize();
            /// Isolate the axis of the center point, the normal bound, and the pivot;
            Vector3 centerNormal = VectorUtils.Mult(collider.center, sizeDeltaNormal);
            Vector3 boundNormal = centerNormal + VectorUtils.Mult(collider.size / 2, sizeDeltaNormal);
            Vector3 pivotNormal = VectorUtils.Mult(pivot, sizeDeltaNormal);
            /// If the distance to the pivot is greater than the distance to the bound,
            /// the pivot is out of bounds and must be moved;
            if (Vector3.Distance(centerNormal, pivotNormal)
                > Vector3.Distance(centerNormal, boundNormal)) {
                UndoUtils.RecordScopeUndo(info, "Change Pivot (Tile Collider)");
                /// Return the pivot by the change in size, towards the center;
                pivot += (pivotNormal - centerNormal).Round().Normalize() * diffSize.Abs();
            } info.RecordTilespaceChange();
            Undo.CollapseUndoOperations(undoGroup);
        }

        public void ShiftPivot(Vector3Int newPivot) {
            UndoUtils.RecordScopeUndo(info, "Change Collider Pivot (Tile Collider)");
            pivot = newPivot;
            info.RecordTilespaceChange();
        }

        public void Dispose() {
            if (collider == null) {
                Debug.Log("Collider was already disposed;\n" +
                          "Safe operation, please disregard;");
                return;
            } GameObject gameObject = info.gameObject;
            Undo.DestroyObjectImmediate(collider);
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif