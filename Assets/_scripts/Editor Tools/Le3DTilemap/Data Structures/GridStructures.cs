using UnityEngine;

namespace Le3DTilemap {
    [System.Serializable]
    public class Cell {

        public Vector3Int localPosition;
        public TileInfo info;
        public TileInstanceData Data => info.InstanceData;

        public Cell(Vector3Int localPosition, TileInfo info) {
            this.localPosition = localPosition;
            this.info = info;
        }
    }

    [System.Serializable]
    public class TileInstanceData {

        public TileData data;
        public GameObject gameObject;

        public Vector3Int position;
        public Vector3Int tileRotation;
        public Vector3 meshRotation;

        public TileInstanceData(TileData data, GameObject gameObject, Vector3Int position, 
                                Vector3Int tileRotation, Vector3 meshRotation) {
            this.data = data;
            this.gameObject = gameObject;
            this.position = position;
            this.tileRotation = tileRotation;
            this.meshRotation = meshRotation;
        }
    }
}