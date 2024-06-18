#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public class TileInfo : MonoBehaviour {

        public event System.Action OnSelectionChanged;

        [SerializeField] private List<TileCollider> colliders;
        public List<TileCollider> Colliders => colliders ??= new();

        [SerializeField] private Vector3Int tilePivot;
        public Vector3Int TilePivot {
            get => tilePivot;
            set {
                if (tilePivot != value) {
                    UndoUtils.RecordScopeUndo(this, "Change Tile Pivot (TileInfo)");
                    tilePivot = value;
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
        
        [HideInInspector]
        [SerializeField] private int hashVersion;
        public int HashVersion => hashVersion;

        void OnValidate() => HideTransformAndColliders();

        public void HideTransformAndColliders() {
            gameObject.hideFlags = HideFlags.NotEditable;
            transform.hideFlags = HideFlags.NotEditable;
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

            Undo.CollapseUndoOperations(undoGroup);
        }

        public void Remove() {
            if (colliders == null || colliders.Count == 0) return;

            int undoGroup = UndoUtils.StartUndoGroup("Remove Tile Collider (Tile Info)");
            UndoUtils.RecordFullScopeUndo(this, "Remove Tile Collider (Tile Info)");

            TileCollider collider = colliders[colliders.Count - 1];
            colliders.RemoveAt(colliders.Count - 1);
            collider.Dispose();

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
        }

        void OnDestroy() {
            foreach (TileCollider collider in colliders) {
                collider.Dispose();
            }
        }
    }
}
#endif