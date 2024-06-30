#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {

    public class TileInfo : MonoBehaviour {

        private const float OFFSET = 0.5f;

        public event System.Action OnSelectionChanged;

        [SerializeField] private List<TileCollider> colliders;
        public List<TileCollider> Colliders => colliders ??= new();

        [SerializeField] private Transform meshRoot;
        public Transform MeshRoot {
            get => meshRoot;
            set {
                if (meshRoot != value) {
                    UndoUtils.RecordScopeUndo(this, "Update Root Mesh (TileInfo)");
                    meshRoot = value;
                }
            }
        }

        [SerializeField] private int nameHint;
        private char NextChar => (char) (65 + (nameHint++ % 26));

        private int selectedIndex;
        public int SelectedIndex => selectedIndex;
        public TileCollider SelectedCollider {
            get {
                if (SelectedIndex >= 0
                    && SelectedIndex < colliders.Count) {
                    return colliders[SelectedIndex];
                } return null;
            }
        }

        [HideInInspector]
        [SerializeField] private SerializableHashSet<Vector3Int> tilespace;
        public SerializableHashSet<Vector3Int> Tilespace => tilespace ??= new();
        
        [SerializeField] private int hashVersion;
        public int HashVersion => hashVersion;

        [SerializeField] private bool pendingHash;
        public bool PendingHash => pendingHash;

        void OnValidate() => HideTransformAndColliders();

        public void TranslatePivot(Vector3Int diff, bool translateColliders,
                           bool translateMesh) {
            diff = transform.InverseTransformPoint(diff).Round();
            UndoUtils.RecordScopeUndo(this, "Change Tile Pivot (TileInfo)");
            if (translateColliders) {
                foreach (TileCollider collider in Colliders) {
                    collider.Pivot -= diff;
                } RecordTilespaceChange();
            }
            if (meshRoot && translateMesh) {
                UndoUtils.RecordScopeUndo(meshRoot, "Change Tile Pivot (Transform)");
                meshRoot.position -= diff;
            }
        }

        public void RotateTilespace(Vector3Int normal, float delta,
                                    bool rotatesColliders, bool rotatesMesh) {
            int signDelta = Mathf.RoundToInt(Mathf.Sign(delta));
            int signInverse = signDelta * -1;
            Matrix4x4 tMatrix = normal.x != 0 ? new Matrix4x4(new(1, 0, 0, 0),
                                                              new(0, 0, signDelta, 0),
                                                              new(0, signInverse, 0, 0),
                                                              new(0, 0, 0, 1))
                              : normal.y != 0 ? new Matrix4x4(new(0, 0, signInverse, 0),
                                                              new(0, 1, 0, 0),
                                                              new(signDelta, 0, 0, 0),
                                                              new(0, 0, 0, 1))
                              : normal.z != 0 ? new Matrix4x4(new(0, signDelta, 0, 0),
                                                              new(signInverse, 0, 0, 0),
                                                              new(0, 0, 1, 0),
                                                              new(0, 0, 0, 1))
                                              : new Matrix4x4(new(1, 0, 0, 0),
                                                              new(0, 1, 0, 0),
                                                              new(0, 0, 1, 0),
                                                              new(0, 0, 0, 1));
            if (rotatesColliders) {
                foreach (TileCollider collider in Colliders) {
                    collider.Rotate(tMatrix);
                } RecordTilespaceChange();
            }

            if (rotatesMesh) {
                UndoUtils.RecordScopeUndo(meshRoot, "Tilespace Rotation (Transform)");
                meshRoot.localPosition = tMatrix.MultiplyPoint3x4(meshRoot.localPosition).Round();
                meshRoot.RotateAround(meshRoot.position, normal, 90 * signDelta);
            }
        }

        public void HideTransformAndColliders() {
            gameObject.SetActive(true);
            gameObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
            transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
            foreach (TileCollider collider in Colliders) {
                collider.collider.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
            } hideFlags = HideFlags.None;
        }

        public TileCollider FindColliderInfo(BoxCollider target, out int index) {
            for (int i = 0; i < colliders.Count; i++) {
                if (colliders[i].collider == target) {
                    index = i;
                    return colliders[i];
                }
            } index = -1;
            return null;
        }

        public void Add() {
            int undoGroup = UndoUtils.StartUndoGroup("Add Tile Collider (Tile Info)");
            UndoUtils.RecordFullScopeUndo(this, "Add Tile Collider (Tile Info)");

            Colliders.Add(new TileCollider(this, NextChar));

            RecordTilespaceChange();
            Undo.CollapseUndoOperations(undoGroup);
        }

        public void Remove() {
            if (colliders == null || colliders.Count == 0) return;

            int undoGroup = UndoUtils.StartUndoGroup("Remove Tile Collider (Tile Info)");
            UndoUtils.RecordFullScopeUndo(this, "Remove Tile Collider (Tile Info)");

            TileCollider collider = colliders[colliders.Count - 1];
            colliders.RemoveAt(colliders.Count - 1);
            collider.Dispose();

            RecordTilespaceChange();
            Undo.CollapseUndoOperations(undoGroup);
        }

        public int Remove(TileCollider target) {
            if (colliders == null || colliders.Count == 0) return -1;
            int index = colliders.FindIndex((collider) => collider == target);
            if (index < 0) return -1;

            int undoGroup = UndoUtils.StartUndoGroup("Remove Tile Collider (Tile Info)");
            UndoUtils.RecordFullScopeUndo(this, "Remove Tile Collider (Tile Info)");

            TileCollider collider = colliders[index];
            colliders.RemoveAt(index);
            collider.Dispose();

            RecordTilespaceChange();
            Undo.CollapseUndoOperations(undoGroup);
            return index;
        }

        public void SoftRemoveAt(int index) {
            if (colliders != null && index >= 0
                && index < colliders.Count) {
                colliders.RemoveAt(index);
            }
        }

        public void Insert(int index, TileCollider collider) {
            colliders.Insert(index, collider);
            EditorUtility.SetDirty(this);
        }

        public void ToggleSelectedIndex(int index) {
            selectedIndex = index == selectedIndex ? -1 : index;
            OnSelectionChanged?.Invoke();
        }

        public void EventDispose() {
            selectedIndex = -1;
            OnSelectionChanged = null;
            HashTilespace();
        }

        public void RecordTilespaceChange() {
            UndoUtils.RecordScopeUndo(this, "Pending Hash (TileInfo)");
            pendingHash = true;
        }

        public void HashTilespace() {
            UndoUtils.RecordScopeUndo(this, "Tilespace Hash (TileInfo)");
            SerializableHashSet<Vector3Int> newHash = new();
            foreach (TileCollider collider in Colliders) {
                for (int x = Mathf.RoundToInt(collider.Center.x - collider.Size.x / 2f + OFFSET);
                     x <= Mathf.RoundToInt(collider.Center.x + collider.Size.x / 2f - OFFSET); x++) {
                    for (int y = Mathf.RoundToInt(collider.Center.y - collider.Size.y / 2f + OFFSET);
                         y <= Mathf.RoundToInt(collider.Center.y + collider.Size.y / 2f - OFFSET); y++) {
                        for (int z = Mathf.RoundToInt(collider.Center.z - collider.Size.z / 2f + OFFSET);
                             z <= Mathf.RoundToInt(collider.Center.z + collider.Size.z / 2f - OFFSET); z++) {
                            newHash.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            } if (!newHash.SetEquals(tilespace)) {
                tilespace = newHash;
                hashVersion++;
            } pendingHash = false;
        }

        void OnDestroy() {
            foreach (TileCollider collider in colliders) {
                collider.Dispose();
            }
        }
    }
}
#endif