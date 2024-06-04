using UnityEngine;

namespace Le3DTilemap {
    [ExecuteInEditMode]
    public class LevelGridHook : MonoBehaviour {

        [SerializeField] private LevelGridData gridData;
        public LevelGridData GridData => gridData;


        #if UNITY_EDITOR
        public void EDITOR_SetGrid(LevelGridData data) {
            gridData = data;
        }
        #endif
    }
}