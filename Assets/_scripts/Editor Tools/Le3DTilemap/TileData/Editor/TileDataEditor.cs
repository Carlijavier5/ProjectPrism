using UnityEngine;
using UnityEditor;

namespace Le3DTilemap {
    [CustomEditor(typeof(TileData))]
    public class TileDataEditor : Editor {

        private TileData TileData => target as TileData;

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            return TileData.Preview == null ? base.RenderStaticPreview(assetPath, subAssets, width, height)
                                            : TileData.Preview;
        }
    }
}