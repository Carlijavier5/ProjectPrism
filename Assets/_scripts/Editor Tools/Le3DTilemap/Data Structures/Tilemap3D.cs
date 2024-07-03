using System.Linq;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class Tilemap3D<T> where T : ScriptableObject {

    [HideInInspector]
    [SerializeField] private Matrix3D<Cell<T>> matrix;
    [HideInInspector]
    [SerializeField] private Vector3Int size;
    public Cell<T> this[Vector3Int position] {
        get {
            return matrix[position - size / 2];
        }
    }

    public Tilemap3D(Vector3Int size) {
        matrix = new Matrix3D<Cell<T>>(size);
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.z; z++) {
                    matrix[x, y, z] = new Cell<T>(size - size / 2);
                }
            }
        } this.size = size;
    }

    public Cell<T> UnscaledInAdjacent(Vector3Int center, Vector3 normal) {
        Vector3Int offset = normal.Round();
        /// Need to compute offset length by propagating offset til an empty tile is found for composite tiles;
        return this[center + offset];
    }
}