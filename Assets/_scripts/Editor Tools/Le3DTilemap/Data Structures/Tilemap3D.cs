using System.Linq;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class Tilemap3D {

    [HideInInspector]
    [SerializeField] private Matrix3D<Cell> matrix;
    [HideInInspector]
    [SerializeField] private Vector3Int size;
    public Cell this[Vector3Int position] {
        get {
            return matrix[position - size / 2];
        }
    }

    public Tilemap3D(Vector3Int size) {
        matrix = new Matrix3D<Cell>(size);
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.z; z++) {
                    matrix[x, y, z] = new Cell(size - size / 2);
                }
            }
        } this.size = size;
    }

    public Cell UnscaledInAdjacent(Vector3Int center, Vector3 normal) {
        Vector3Int offset = normal.Round();
        /// Need to compute offset length by propagating offset til an empty tile is found for composite tiles;
        return this[center + offset];
    }
}