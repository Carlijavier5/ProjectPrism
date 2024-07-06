using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJUtils {
    public static class StaticUtils {

        public static void DeepIterate(this Transform transform,
                                       System.Action<Transform> callback) {
            callback?.Invoke(transform);
            foreach (Transform t in transform) t.DeepIterate(callback);
        }
    }
}