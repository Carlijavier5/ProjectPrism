using UnityEngine;

public static class GridUtils {
    
    public static Vector3Int Round(this Vector3 vec) {
        return new Vector3Int(Mathf.RoundToInt(vec.x),
                              Mathf.RoundToInt(vec.y),
                              Mathf.RoundToInt(vec.z));
    }
}