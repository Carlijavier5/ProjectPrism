using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CJUtils;

namespace Le3DTilemap {
    public class TileCreationWindow : EditorWindow {

        private TileCreationWindowSettings settings;

        private enum SetupMode { None, InPlace, Variant }
        private SetupMode setupMode = SetupMode.Variant;

        private bool isDynamic;

        public event System.Action<TileData> OnTileCreation;

        private Editor preview;
        private GameObject prefab;

        private TileInfo info;
        private PrefabAssetType prefabType;
        private string tileName = "No Prefab Selected";
        private string prefabName = "No Prefab Selected";
        private Vector2 globalScroll;

        private Texture2D iconHelp, iconGood, iconSearch,
                          iconPlus;

        public static TileCreationWindow ShowAuxiliary(GameObject prefab) {
            TileCreationWindow window = GetWindow<TileCreationWindow>("New Tile Data");
            window.prefab = prefab;
            if (prefab) window.ProcessPrefab(prefab);
            window.ShowAuxWindow();
            return window;
        }

        void OnEnable() {
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyCleanup;
            if (settings is null) {
                AssetUtils.TryRetrieveAsset(out settings);
            } LoadIcons();
        }

        void OnGUI() {
            if (settings == null) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope()) {
                        GUILayout.FlexibleSpace();
                        if (settings == null) {
                            EditorGUILayout.GetControlRect(GUILayout.Height(1));
                            SceneViewUtils.DrawMissingSettingsPrompt(ref settings,
                                                    "Missing Window Settings",
                                                    "New Tile Creation Settings",
                                                    iconSearch, iconPlus);
                            EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        } GUILayout.FlexibleSpace();
                    } GUILayout.FlexibleSpace();
                } return;
            }

            AssetUtils.InvalidNameCondition tileNameValid = AssetUtils.ValidateFilename(tileName);
            AssetUtils.InvalidNameCondition prefabNameValid = AssetUtils.ValidateFilename(prefabName);
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(globalScroll)) {
                globalScroll = scrollScope.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(150))) {
                    EditorGUILayout.GetControlRect(GUILayout.Width(1));
                    using (new EditorGUILayout.VerticalScope()) {
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));
                        if (prefab is null) {
                            GUIUtils.DrawScopeCenteredText("Select a GameObject Prefab");
                        } else {
                            if (preview is null ) {
                                Editor.CreateCachedEditor(prefab, null, ref preview);
                            } preview.DrawPreview(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true),
                                                                                 GUILayout.ExpandHeight(true)));
                        } EditorGUILayout.GetControlRect(GUILayout.Height(1));
                    } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                }
                
                EditorGUILayout.GetControlRect(GUILayout.Height(2));
                GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                EditorGUILayout.GetControlRect(false, 4);

                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.GetControlRect(GUILayout.Width(2));
                    using (new EditorGUILayout.VerticalScope()) {
                        using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            GUILayout.Label("Prefab:");
                            using (var scope = new EditorGUI.ChangeCheckScope()) {
                                prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), false) as GameObject;
                                if (scope.changed) {
                                    if (prefab != null) {
                                        ProcessPrefab(prefab);
                                    } else {
                                        tileName = "No Prefab Selected";
                                        prefabName = "No Prefab Selected";
                                    }
                                }
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        GUI.enabled = prefab != null;
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Tile Settings", UIStyles.CenteredLabelBold);
                        } EditorGUIUtility.labelWidth = 80;
                        using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            using (new EditorGUILayout.VerticalScope()) {
                                using (new EditorGUILayout.HorizontalScope()) {
                                    GUILayout.Label("Tile Name:");
                                    GUI.color = tileNameValid == AssetUtils.InvalidNameCondition.None ? Color.white
                                                                                                     : UIColors.DarkRed;
                                    tileName = EditorGUILayout.TextField(tileName, GUILayout.Width(165));
                                    DrawTooltipLabel(tileNameValid);
                                    GUI.color = Color.white;
                                }
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        GUI.enabled = prefab != null;
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Setup Mode", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.HorizontalScope()) {
                            string tooltip = GUI.enabled ? "Do not perform any prefab setup" : string.Empty;
                            GUIContent content = new GUIContent("None", tooltip);
                            GUIStyle style = new(GUI.skin.button) { margin = { right = 0 } };
                            Rect rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                       GUILayout.Width(90),
                                                                       GUILayout.ExpandWidth(true));
                            rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                            DrawSetupButton(rect, SetupMode.None, content, style);
                            bool allowInPlace = prefabType == PrefabAssetType.Regular
                                             || prefabType == PrefabAssetType.Variant;
                            GUI.enabled = allowInPlace;
                            tooltip = GUI.enabled ? allowInPlace ? "Modify the pre-existing target prefab"
                                                                 : "Raw models do not allow in-place modifications"
                                                  : string.Empty;
                            content = new GUIContent("In-Place", tooltip);
                            style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                            rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                  GUILayout.Width(90),
                                                                  GUILayout.ExpandWidth(true));
                            rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                            DrawSetupButton(rect, SetupMode.InPlace, content, style);
                            GUI.enabled = prefab != null;
                            tooltip = GUI.enabled ? "Create a new prefab containing the target" : string.Empty;
                            content = new GUIContent("Variant", tooltip);
                            style = new(GUI.skin.button) { margin = { left = 0 } };
                            rect = EditorGUILayout.GetControlRect(false, 19, style,
                                                                  GUILayout.Width(90),
                                                                  GUILayout.ExpandWidth(true));
                            rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                            DrawSetupButton(rect, SetupMode.Variant, content, style);
                            GUI.backgroundColor = Color.white;
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Prefab Settings", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.VerticalScope()) {
                            EditorGUILayout.GetControlRect(false, 2, GUILayout.Width(1));
                            if (setupMode == 0) GUI.enabled = false;
                            using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Prefab Name:");
                                GUI.color = prefabNameValid == AssetUtils.InvalidNameCondition.None ? Color.white
                                                                                                 : UIColors.DarkRed;
                                prefabName = EditorGUILayout.TextField(prefabName, GUILayout.Width(165));
                                DrawTooltipLabel(prefabNameValid);
                                GUI.color = Color.white;
                            } using (new EditorGUILayout.HorizontalScope()) {
                                GUILayout.Label("Engine Complexity:");
                                GUIContent content = new GUIContent("Static", GUI.enabled ? "Static Tiles are:\n"
                                                                  + "- Mergeable into larger meshes for optimization\n"
                                                                  + "- Significantly more performant than dynamic tiles\n"
                                                                  + "Static Tiles cannot:\n"
                                                                  + "- Move, rotate, or scale <b><i>at runtime</i></b>\n"
                                                                  + "- Perform animations involving transform changes"
                                                                  : string.Empty);
                                GUIStyle style = new(GUI.skin.button) { margin = { right = 0, left = 0 } };
                                Rect rect = EditorGUILayout.GetControlRect(false, 18, style,
                                                                           GUILayout.Width(92),
                                                                           GUILayout.ExpandWidth(true));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawComplexityButton(rect, false, content, style);
                                content = new GUIContent("Dynamic", GUI.enabled ? "Dynamic Tiles are:\n"
                                                       + "- Excluded from mesh-merging optimization\n"
                                                       + "- Significantly less performant than static tiles\n"
                                                       + "Tiles should be Dynamic if they:\n"
                                                       + "- Must be moved, rotated, or scaled <b><i>at runtime</i></b>\n"
                                                       + "- Contain animations involving transform changes"
                                                       : string.Empty);
                                style = new(GUI.skin.button) { margin = { left = 0 } };
                                rect = EditorGUILayout.GetControlRect(false, 18, style,
                                                                      GUILayout.Width(92),
                                                                      GUILayout.ExpandWidth(true));
                                rect = new(rect) { y = rect.y - 1, height = rect.height + 2 };
                                DrawComplexityButton(rect, true, content, style);
                                GUI.backgroundColor = Color.white;
                            } EditorGUILayout.GetControlRect(false, 2, GUILayout.Width(1));
                            GUI.enabled = true;
                            EditorGUIUtility.labelWidth = 0;
                        }

                        EditorGUILayout.GetControlRect(GUILayout.Height(3));
                        GUIUtils.DrawSeparatorLine(UIColors.DarkGray);
                        EditorGUILayout.GetControlRect(GUILayout.Height(1));

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                            GUILayout.Label("Messages", UIStyles.CenteredLabelBold);
                        } using (new EditorGUILayout.HorizontalScope()) {
                            EditorGUILayout.GetControlRect(GUILayout.Width(1));
                            using (new EditorGUILayout.VerticalScope()) {
                                DisplayMessages();
                            } EditorGUILayout.GetControlRect(GUILayout.Width(1));
                        }
                    } EditorGUILayout.GetControlRect(GUILayout.Width(2));
                }
            } using (new EditorGUILayout.HorizontalScope()) {
                GUI.enabled = prefab != null && tileNameValid == 0 && prefabNameValid == 0;
                GUI.color = UIColors.Green;
                if (GUILayout.Button("Create")) {
                    CommitSetup();
                } GUI.enabled = true;
                GUI.color = UIColors.Red;
                if (GUILayout.Button("Cancel")) Close();
            }
        }

        private void ProcessPrefab(GameObject prefab) {
            prefabType = PrefabUtility.GetPrefabAssetType(prefab);
            info = prefab.GetComponentInChildren<TileInfo>();
            if (info) setupMode = SetupMode.None;
            tileName = prefab.name;
            prefabName = prefab.name;
        }

        private void DrawSetupButton(Rect rect, SetupMode mode, 
                                     GUIContent content, GUIStyle style) {
            GUI.backgroundColor = GUI.enabled && setupMode == mode ? UIColors.DefinedBlue 
                                                                   : Color.white;
            if (GUI.Button(rect, content, style)) setupMode = mode;
        }

        private void DrawComplexityButton(Rect rect, bool isDynamic,
                                          GUIContent content, GUIStyle style) {
            GUI.backgroundColor = this.isDynamic == isDynamic 
                                                 && GUI.enabled ? UIColors.DefinedBlue
                                                                : Color.white;
            if (GUI.Button(rect, content, style)) this.isDynamic = isDynamic;
        }

        private void DisplayMessages() {
            using (new EditorGUILayout.ScrollViewScope(Vector2.zero)) {
                GUIStyle style = new(UIStyles.HelpBoxLabel) { padding = { top = 3, bottom = 3 } };
                if (prefab != null) {
                    if (info) {
                        GUIUtils.DrawCustomHelpBox("The target prefab already contains TileInfo\n"
                                                 + "Further setup is safe, but not required", iconHelp, style);
                        if (setupMode > 0) {
                            GUIUtils.DrawCustomHelpBox("Tile Info will be re-parented to preserve integrity\n"
                                                     + "Check for unwanted nesting post-setup", iconHelp, style);
                        }
                    } string message = setupMode switch {
                        SetupMode.Variant => "Setup modifications will output a new asset",
                        SetupMode.InPlace => "The target prefab will be modified in-place",
                        _ => "No changes will be made to the target prefab",
                    }; GUIUtils.DrawCustomHelpBox(message, iconGood, style);
                } else {
                    GUIUtils.DrawCustomHelpBox("A Prefab is required to generate Tile Data", iconHelp, style);
                }
            }
        }

        private void DrawTooltipLabel(AssetUtils.InvalidNameCondition nameValidity) {
            string tooltip = GUI.enabled ? nameValidity switch {
                AssetUtils.InvalidNameCondition.None => "Name is valid",
                AssetUtils.InvalidNameCondition.Empty => "Name cannot be empty or whitespace",
                _ => "Naming convention violated:"
                   + "\n- First letter must be capitalized or an underscore"
                   + "\n- Cannot contain invalid characters" } : "";
            GUIStyle style = new(EditorStyles.label) { contentOffset = new(0, -2) };
            GUILayout.Label(new GUIContent(iconHelp, tooltip),
                            style, GUILayout.Width(19), GUILayout.Height(19));
        }

        private void CommitSetup() {
            TileData newAsset = AssetUtils.CreateAsset<TileData>("New Tile Data", tileName.Trim());
            GameObject newPrefab = prefab;
            if (newAsset) {
                if (setupMode > 0) {
                    string path = AssemblePrefab(out newPrefab, out bool success);
                    if (!success) {
                        Debug.LogWarning($"Could not create/modify asset at {path}");
                        return;
                    }
                } newAsset.Prefab = newPrefab;
                EditorUtility.SetDirty(newAsset);
                OnTileCreation?.Invoke(newAsset);
                Close();
            }
        }

        private string AssemblePrefab(out GameObject newAsset, out bool success) {

            bool inPlace = setupMode == SetupMode.InPlace;
            GameObject mainRoot;

            if (info && info.MeshRoot) {
                mainRoot = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            } else {
                mainRoot = new(prefabName);
                GameObject meshRoot = PrefabUtility.InstantiatePrefab(prefab, mainRoot.transform) as GameObject;
                if (info) {
                    info = meshRoot.GetComponentInChildren<TileInfo>();
                    while (PrefabUtility.IsPartOfAnyPrefab(info.transform.parent)) {
                        GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(info.transform.parent);
                        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.OutermostRoot,
                                                           InteractionMode.AutomatedAction);
                    } info.transform.SetParent(mainRoot.transform);
                } else {
                    GameObject infoInstance = PrefabUtility.InstantiatePrefab(settings.tileInfoPrefab,
                                                                              mainRoot.transform) as GameObject;
                    infoInstance.TryGetComponent(out info);
                } info.MeshRoot = meshRoot.transform;

                if (inPlace && PrefabUtility.IsPartOfAnyPrefab(meshRoot)) {
                        PrefabUtility.UnpackPrefabInstance(meshRoot, PrefabUnpackMode.Completely,
                                                           InteractionMode.AutomatedAction);
                } meshRoot.name = $"Mesh Root ({prefabName})";
            }

            string path = AssetDatabase.GetAssetPath(prefab);
            string folder = path.RemovePathEnd("\\/");

            bool directOverride = inPlace && prefabName == prefab.name;
            string newName = directOverride ? prefab.name
                           : AssetUtils.ProduceValidAssetName(folder, prefabName, ".prefab");
            string newPath = directOverride ? path
                           : AssetUtils.ProduceValidAssetNotation(folder, newName, ".prefab");

            if (inPlace) AssetDatabase.RenameAsset(path, newName);
            mainRoot.transform.DeepIterate((t) => { t.gameObject.isStatic = !isDynamic;
                                                    t.gameObject.layer = 6; });
            newAsset = PrefabUtility.SaveAsPrefabAsset(mainRoot, newPath, out success);
            DestroyImmediate(mainRoot);
            return path;
        }

        void AssemblyCleanup() {
            AssemblyReloadEvents.beforeAssemblyReload -= AssemblyCleanup;
            DestroyImmediate(preview);
            Close();
        }

        void OnDisable() {
            OnTileCreation = null;
            DestroyImmediate(preview);
            settings = null;
            Resources.UnloadUnusedAssets();
        }

        private void LoadIcons() {
            EditorUtils.LoadIcon(ref iconHelp, EditorUtils.ICON_HELP);
            EditorUtils.LoadIcon(ref iconGood, EditorUtils.ICON_CHECK_BLUE);
            EditorUtils.LoadIcon(ref iconSearch, EditorUtils.ICON_SEARCH);
            EditorUtils.LoadIcon(ref iconPlus, EditorUtils.ICON_PLUS);
        }
    }
}