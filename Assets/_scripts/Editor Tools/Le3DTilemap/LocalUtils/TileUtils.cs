using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Le3DTilemap {

    public static class TileUtils {

        #if UNITY_EDITOR

        public static void TileSizeField(TileCollider collider,
                                         params GUILayoutOption[] options) {
            Vector3Int size = EditorGUILayout.Vector3IntField("", collider.Size, options).Abs();
            if (collider.Size != size) {
                Vector3Int sizeDiff = size - collider.Size;
                Vector3 mod = VectorUtils.Mod((Vector3) sizeDiff / 2f
                                              + collider.Center,
                                              Vector3.one);
                Vector3Int adjustment = new (mod.x != 0 ? 1 : 0,
                                             mod.y != 0 ? 1 : 0,
                                             mod.z != 0 ? 1 : 0);
                collider.Resize(Vector3Int.zero, sizeDiff + adjustment);
            }
        }

        #endif
    }
}