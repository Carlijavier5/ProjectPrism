using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
    [CustomEditor(typeof(LevelGridHook))]
    public class LevelGridHookEditor : Editor {

        private LevelGridHook Hook => target as LevelGridHook;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Add Tile")) {
                ///Hook.PlaceTile(Vector3Int.zero);
            }
            if (GUILayout.Button("Remove Tile")) {
                ///Hook.RemoveTile(Vector3Int.zero);
            }
            if (GUILayout.Button("Print Tiles")) {
                ///Hook.PrintTiles();
            }
        }
    }
}
