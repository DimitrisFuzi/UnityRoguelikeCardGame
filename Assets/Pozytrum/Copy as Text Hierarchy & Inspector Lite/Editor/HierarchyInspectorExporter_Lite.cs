// Assets/Pozytrum/HierarchyInspectorExporter_Lite/Editor/HierarchyInspectorExporter_Lite.cs
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;

namespace Pozytrum.HierarchyInspectorExporterLite.Editor
{
    /// <summary>
    /// Lite/Demo version of the Hierarchy & Inspector Exporter tool v1.1.
    /// Demonstrates basic copying. Disabled options link to the full version.
    /// Author: Pozytrum Michał Szczepanik
    /// </summary>
    public class HierarchyInspectorExporter_Lite : EditorWindow
    {
        // --- Enums ---
        private enum ToolSection { CopyProperties, CopyHierarchy }
        private enum PropertyCopyMode { DetailedReflection, VisibleSerialized } // Keep for UI structure

        // --- Settings ---
        private ToolSection currentToolSection = ToolSection.CopyProperties;
        private bool propShowTransform = true; // Option available in Lite
        private string hierarchyIndentString = "  "; // Option available in Lite
        private string fullVersionLink = "https://assetstore.unity.com/packages/tools/utilities/copy-as-text-hierarchy-inspector-316181";

        private Vector2 scrollPosition;
        private static readonly Color disabledOverlayColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent grey

        // --- Window Management ---
        [MenuItem("Tools/Pozytrum/Hierarchy & Inspector Exporter (Lite)")]
        static void ShowExporterWindow()
        {
            HierarchyInspectorExporter_Lite window = GetWindow<HierarchyInspectorExporter_Lite>(false, "H&I Exporter Lite");
            window.minSize = new Vector2(420, 420);
            window.Show();
        }

        void OnGUI()
        {
            currentToolSection = (ToolSection)GUILayout.Toolbar((int)currentToolSection, new string[] { "Copy Properties", "Copy Hierarchy" });
            EditorGUILayout.Space();

            GameObject activeObject = Selection.activeGameObject;
            GameObject[] selectedObjects = Selection.gameObjects;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (currentToolSection == ToolSection.CopyProperties)
            {
                if (activeObject == null) EditorGUILayout.HelpBox("Select a GameObject to copy its component properties.", MessageType.Info);
                else DrawPropertiesUI_Lite(activeObject);
            }
            else // ToolSection.CopyHierarchy
            {
                DrawHierarchyUI_Lite(selectedObjects);
            }

            EditorGUILayout.EndScrollView();

            // --- Footer with Upgrade Link ---
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This is the Lite version. Click disabled options or the button below to see the full version!", MessageType.Info);
            if (GUILayout.Button("Upgrade to Full Version on Asset Store"))
            {
                Application.OpenURL(fullVersionLink);
            }
        }

        // ================================
        // == Properties Copy UI (Lite) ==
        // ================================
        void DrawPropertiesUI_Lite(GameObject selectedObject)
        {
            EditorGUILayout.LabelField($"Copy Properties from: {selectedObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Copy ALL Component Properties", EditorStyles.boldLabel);

            // --- Mode Selection (Disabled Link) ---
            DrawDisabledOptionLink(() => {
                EditorGUILayout.EnumPopup(new GUIContent("Mode", "Choose 'Detailed' mode in Full Version"), PropertyCopyMode.VisibleSerialized); // Show Visible as selected
            }, "(Detailed Reflection mode available in full version)");

            EditorGUILayout.HelpBox("Visible (Serialized): Copies data handled by the Inspector system.", MessageType.None);

            propShowTransform = EditorGUILayout.Toggle(new GUIContent("Include Transform Component"), propShowTransform);

            // --- Disabled Reflection Options ---
            DrawDisabledOptionLink(() => {
                EditorGUILayout.Toggle(new GUIContent("Include Non-Public Members"), false);
            }, "(Available in Full Version)");

            DrawDisabledOptionLink(() => {
                EditorGUILayout.Toggle(new GUIContent("Group Fields/Properties"), true); // Show default state
            }, "(Available in Full Version)");

            EditorGUILayout.Space();

            // --- Action Button (Visible Mode Only) ---
            string buttonLabelProp = "Copy All Properties (Visible)";
            if (GUILayout.Button(buttonLabelProp))
            {
                CopyGameObjectProperties(selectedObject, PropertyCopyMode.VisibleSerialized, propShowTransform, false, false);
            }
            EditorGUILayout.Space(15);
            EditorGUILayout.HelpBox("Right-click on a component header to copy only that component's visible properties.", MessageType.Info);
        }

        // ================================
        // == Hierarchy Copy UI (Lite) ==
        // ================================
        void DrawHierarchyUI_Lite(GameObject[] selection)
        {
            if (selection.Length > 0) { if (selection.Length == 1) EditorGUILayout.LabelField($"Selected Object: {selection[0].name}", EditorStyles.boldLabel); else EditorGUILayout.LabelField($"Selected Objects: {selection.Length}", EditorStyles.boldLabel); }
            else { EditorGUILayout.LabelField("No objects selected (some options affect entire scene)", EditorStyles.boldLabel); }
            EditorGUILayout.Space();

            // --- General Options ---
            EditorGUILayout.LabelField("General Hierarchy Options", EditorStyles.boldLabel);
            hierarchyIndentString = EditorGUILayout.TextField(new GUIContent("Indentation String"), hierarchyIndentString);

            // --- Disabled General Options ---
            DrawDisabledOptionLink(() => {
                EditorGUILayout.Toggle(new GUIContent("Include Inactive Objects"), false);
            }, "(Available in Full Version)");
            DrawDisabledOptionLink(() => {
                EditorGUILayout.Toggle(new GUIContent("Use Foldout Markers (▶▼)"), false);
            }, "(Available in Full Version)");
            EditorGUILayout.Separator();

            // --- Section 1: Copy Full Hierarchy Below ---
            EditorGUILayout.LabelField("Option 1: Copy Hierarchy Below Selection", EditorStyles.boldLabel);
            bool selectionExists = selection.Length > 0;
            EditorGUI.BeginDisabledGroup(!selectionExists); // Base disable if no selection
            if (selection.Length > 1) EditorGUILayout.HelpBox("Copies the full hierarchy below *each* selected object individually.", MessageType.Info);

            DrawDisabledOptionLink(() => {
                EditorGUILayout.IntField(new GUIContent("Max Depth (-1 for All)"), -1);
            }, "(Available in Full Version)");

            EditorGUILayout.Space();
            if (GUILayout.Button("Copy Hierarchy Below (Basic)"))
            {
                // Call core logic with fixed parameters for Lite version
                CopyHierarchyBelowSelection(selection, false, -1, hierarchyIndentString, false);
            }
            EditorGUI.EndDisabledGroup();
            if (!selectionExists) EditorGUILayout.HelpBox("Select object(s) to enable.", MessageType.Warning);
            EditorGUILayout.Separator();

            // --- Section 2: Copy Selection Only ---
            EditorGUILayout.LabelField("Option 2: Copy Selection Only", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(!selectionExists);
            EditorGUILayout.HelpBox("Copies only the names of the selected objects, preserving relative structure via indentation.", MessageType.Info);
            EditorGUILayout.Space();
            if (GUILayout.Button("Copy Selected Structure Only"))
            {
                CopySelectedHierarchyOnly(selection, hierarchyIndentString, false);
            }
            EditorGUI.EndDisabledGroup();
            if (!selectionExists) EditorGUILayout.HelpBox("Select object(s) to enable.", MessageType.Warning);
            EditorGUILayout.Separator();

            // --- Section 3: Copy Entire Scene (Disabled Link) ---
            EditorGUILayout.LabelField("Option 3: Copy Entire Scene/Prefab Hierarchy", EditorStyles.boldLabel);
            // Wrap the button itself in the disabled link logic
            DrawDisabledOptionLink(() => {
                // Draw a disabled-looking button
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button("Copy Entire Hierarchy");
                }
            }, "(Available in Full Version)");
            EditorGUILayout.HelpBox("Copies the hierarchy of all root objects in the active scene or prefab stage.", MessageType.Info);

            EditorGUILayout.Space(15);
            EditorGUILayout.HelpBox("Right-click on object(s) in Hierarchy window for basic copy options.", MessageType.Info);
        }

        // Helper function to draw a control and overlay a clickable link if disabled
        private void DrawDisabledOptionLink(Action drawControlAction, string tooltip)
        {
            // Store current state to restore later
            Color originalContentColor = GUI.contentColor;
            Color originalBackgroundColor = GUI.backgroundColor;
            bool wasEnabled = GUI.enabled;

            // Draw the control normally first to get its rect
            // Need to ensure it doesn't actually modify value if clicked
            drawControlAction();
            Rect controlRect = GUILayoutUtility.GetLastRect();

            // Manually disable GUI elements visually and functionally
            GUI.enabled = false; // Affects subsequent draws in this scope if not reset
            GUI.contentColor *= new Color(1f, 1f, 1f, 0.5f); // Make content semi-transparent

            // Re-draw the control visually disabled (this might not perfectly match all control styles)
            // This is the tricky part - redrawing accurately is hard.
            // A simpler alternative is just drawing the overlay.

            // Draw the semi-transparent overlay
            Color previousColor = GUI.color;
            GUI.color = disabledOverlayColor;
            GUI.DrawTexture(controlRect, EditorGUIUtility.whiteTexture);
            GUI.color = previousColor;

            // Restore GUI state
            GUI.contentColor = originalContentColor;
            GUI.backgroundColor = originalBackgroundColor;
            GUI.enabled = wasEnabled; // Restore original enabled state

            // Check for click within the control's bounds
            if (Event.current.type == EventType.MouseDown && controlRect.Contains(Event.current.mousePosition))
            {
                // Show tooltip or directly open URL
                //Debug.Log($"Clicked disabled option: {tooltip}");
                if (EditorUtility.DisplayDialog(
                    "Full Version Feature",
                    $"This option {tooltip}\nWould you like to visit the Asset Store page?",
                    "Open Asset Store", "Cancel"))
                {
                    Application.OpenURL(fullVersionLink);
                }
                Event.current.Use(); // Consume the click event
            }
            // Add a tooltip to the rect area
            GUI.Label(controlRect, new GUIContent("", $"Requires Full Version {tooltip}"));
        }


        // ========================================
        // == Context Menu Items (Lite Version) ==
        // ========================================

        // --- Component Context Menu ---
        private const string LITE_COMPONENT_CONTEXT_MENU_PATH = "CONTEXT/Component/Copy Component Data (Lite)/";
        [MenuItem(LITE_COMPONENT_CONTEXT_MENU_PATH + "Copy This (Visible Properties)", false, 1000)]
        private static void CtxCompVisThisLite(MenuCommand command) { CopySingleComponentProperties(command.context as Component, true); } // Always visible, respect transform flag maybe? Set to true for simplicity.
        [MenuItem(LITE_COMPONENT_CONTEXT_MENU_PATH + "Copy All on GameObject (Visible Properties) (Full Version)", false, 1001)] // Premium Feature Link
        private static void CtxCompVisAllLite(MenuCommand command) { OpenStoreLinkFromMenu("Copying all components"); }
        [MenuItem(LITE_COMPONENT_CONTEXT_MENU_PATH + "Copy This (Detailed - All Members) (Full Version)", false, 1010)] // Premium Feature Link
        private static void CtxCompDetAllLite(MenuCommand command) { OpenStoreLinkFromMenu("Detailed component copying (Reflection)"); }

        // Validation (Only enable the one working feature)
        [MenuItem(LITE_COMPONENT_CONTEXT_MENU_PATH + "Copy This (Visible Properties)", true)]
        private static bool ValCtxCompLite(MenuCommand command) => command.context is Component;
        // Disable others explicitly via validation returning false
        [MenuItem(LITE_COMPONENT_CONTEXT_MENU_PATH + "Copy All on GameObject (Visible Properties) (Full Version)", true)]
        [MenuItem(LITE_COMPONENT_CONTEXT_MENU_PATH + "Copy This (Detailed - All Members) (Full Version)", true)]
        private static bool ValCtxPremiumCompLite(MenuCommand command) => true; // Keep enabled to allow clicking for store link


        // --- Hierarchy Context Menu ---
        private const string LITE_HIERARCHY_MENU_ROOT = "GameObject/Copy Object Names (Lite)/";
        [MenuItem(LITE_HIERARCHY_MENU_ROOT + "Hierarchy Below (Full Version)", false, 10)] // Premium Feature Link
        private static void ContextMenuCopyHierarchyBelowLite(MenuCommand command) { OpenStoreLinkFromMenu("Copying full hierarchy below"); }
        [MenuItem(LITE_HIERARCHY_MENU_ROOT + "Selected Objects Only (Basic Structure)", false, 11)]
        private static void ContextMenuCopySelectedOnlyLite(MenuCommand command) { GameObject[] sel = Selection.gameObjects; if (sel.Length > 0) CopySelectedHierarchyOnly(sel, "  ", false); } // Basic indent, no markers
        [MenuItem(LITE_HIERARCHY_MENU_ROOT + "Entire Scene Hierarchy(Full Version)", false, 12)] // Premium Feature Link
        private static void ContextMenuCopyEntireHierarchyLite(MenuCommand command) { OpenStoreLinkFromMenu("Copying entire scene hierarchy"); }

        // Validation (Enable selected only if something is selected, others link to store)
        [MenuItem(LITE_HIERARCHY_MENU_ROOT + "Hierarchy Below (Full Version)", true)]
        [MenuItem(LITE_HIERARCHY_MENU_ROOT + "Entire Scene Hierarchy (Full Version)", true)]
        private static bool ValidateContextMenuPremiumHierLite(MenuCommand command) => true; // Keep enabled for link
        [MenuItem(LITE_HIERARCHY_MENU_ROOT + "Selected Objects Only (Basic Structure)", true)]
        private static bool ValidateContextMenuCopySelectedLite(MenuCommand command) => Selection.gameObjects.Length > 0;

        // Helper to open store link from context menu
        private static void OpenStoreLinkFromMenu(string featureName)
        {
            string link = GetWindow<HierarchyInspectorExporter_Lite>()?.fullVersionLink ?? "https://assetstore.unity.com/packages/tools/utilities/copy-as-text-hierarchy-inspector-316181"; // Fallback link
            if (EditorUtility.DisplayDialog(
                   "Full Version Feature",
                   $"The feature '{featureName}' requires the full version of Hierarchy & Inspector Exporter.\nWould you like to visit the Asset Store page?",
                   "Open Asset Store", "Cancel"))
            {
                Application.OpenURL(link);
            }
        }


        // ============================================================
        // == Static Core Logic (Minimal for Lite, Keep Unused Safe) ==
        // ============================================================
        // Methods needed for enabled Lite features:
        // CopySingleComponentProperties (called with VisibleSerialized)
        // ProcessComponentSerialized
        // GetPropertyValueAsStringSerialized
        // CopySelectedHierarchyOnly
        // GetHierarchyPath
        // CalculateDepth
        // AppendHeader

        // Unused methods (can be commented out or removed, but kept here for reference)
        // private static void CopyGameObjectProperties(...) {}
        // private static void ProcessComponentReflection(...) {}
        // private static void AppendMembersReflection(...) {}
        // private static string FormatValueReflection(...) {}
        // private static void CopyHierarchyBelowSelection(...) {}
        // private static void CopyGameObjectHierarchy(...) {}
        // private static void ProcessHierarchyRecursive(...) {}
        // private static void CopyEntireHierarchy(...) {}

        // --- Property Copy (Single Component, Visible Only) ---
        private static void CopySingleComponentProperties(Component targetComponent, bool showTransform)
        {
            if (targetComponent == null) return; StringBuilder sb = new StringBuilder(); string nl = System.Environment.NewLine; AppendHeader(sb, $"Component: {targetComponent.GetType().Name} on GameObject: {targetComponent.gameObject.name}"); sb.Append(nl); ProcessComponentSerialized(sb, targetComponent, showTransform); EditorGUIUtility.systemCopyBuffer = sb.ToString(); Debug.Log($"[H&I Exporter Lite] Copied visible properties for {targetComponent.GetType().Name} on {targetComponent.gameObject.name}.");
        }
        // --- ProcessComponentSerialized (Used by Lite) ---
        private static void ProcessComponentSerialized(StringBuilder sb, Component component, bool showTransform)
        {
            string nl = System.Environment.NewLine; if (component == null) { AppendHeader(sb, "--- Missing Component ---"); sb.Append(nl); return; }
            if (!showTransform && component is Transform) return; System.Type componentType = component.GetType(); AppendHeader(sb, $"--- {componentType.Name} (Visible) ---"); SerializedObject serializedObject = null; try { serializedObject = new SerializedObject(component); SerializedProperty iterator = serializedObject.GetIterator(); bool enterChildren = true; while (iterator.NextVisible(enterChildren)) { if (iterator.propertyPath == "m_Script" && iterator.propertyType == SerializedPropertyType.ObjectReference) { enterChildren = false; continue; } string displayName = iterator.displayName; string valueString = GetPropertyValueAsStringSerialized(iterator); sb.Append(String.Concat(Enumerable.Repeat("  ", iterator.depth + 1))); sb.Append($"{displayName}: {valueString}{nl}"); enterChildren = false; } } catch (System.Exception ex) { sb.Append($"  <Error processing serialized properties: {ex.GetType().Name}>{nl}"); } finally { (serializedObject as IDisposable)?.Dispose(); }
        }
        // --- GetPropertyValueAsStringSerialized (Used by Lite) ---
        private static string GetPropertyValueAsStringSerialized(SerializedProperty prop)
        {
            try { switch (prop.propertyType) { case SerializedPropertyType.Integer: return prop.longValue.ToString(); case SerializedPropertyType.Boolean: return prop.boolValue.ToString(); case SerializedPropertyType.Float: return prop.doubleValue.ToString("F3"); case SerializedPropertyType.String: return $"\"{prop.stringValue}\""; case SerializedPropertyType.Color: Color clrVal = prop.colorValue; return $"RGBA({clrVal.r:F3}, {clrVal.g:F3}, {clrVal.b:F3}, {clrVal.a:F3})"; case SerializedPropertyType.ObjectReference: UnityEngine.Object o = prop.objectReferenceValue; try { bool exists = o; return exists ? $"{o.name} ({o.GetType().Name})" : "None (Object Reference)"; } catch (MissingReferenceException) { return "Missing/Destroyed (Object Reference)"; } case SerializedPropertyType.LayerMask: return $"(LayerMask int: {prop.intValue})"; case SerializedPropertyType.Enum: return (prop.enumValueIndex >= 0 && prop.enumValueIndex < prop.enumDisplayNames.Length) ? prop.enumDisplayNames[prop.enumValueIndex] : $"(Enum Invalid Index: {prop.enumValueIndex})"; case SerializedPropertyType.Vector2: Vector2 v2 = prop.vector2Value; return $"({v2.x:F3}, {v2.y:F3})"; case SerializedPropertyType.Vector3: Vector3 v3 = prop.vector3Value; return $"({v3.x:F3}, {v3.y:F3}, {v3.z:F3})"; case SerializedPropertyType.Vector4: Vector4 v4 = prop.vector4Value; return $"({v4.x:F3}, {v4.y:F3}, {v4.z:F3}, {v4.w:F3})"; case SerializedPropertyType.Rect: Rect r = prop.rectValue; return $"(X:{r.x:F1}, Y:{r.y:F1}, W:{r.width:F1}, H:{r.height:F1})"; case SerializedPropertyType.ArraySize: return $"Size: {prop.arraySize}"; case SerializedPropertyType.Character: return $"'{(char)prop.intValue}'"; case SerializedPropertyType.AnimationCurve: return "(AnimationCurve)"; case SerializedPropertyType.Bounds: Bounds b = prop.boundsValue; return $"Center:({b.center.x:F3},{b.center.y:F3},{b.center.z:F3}), Extents:({b.extents.x:F3},{b.extents.y:F3},{b.extents.z:F3})"; case SerializedPropertyType.Gradient: return "(Gradient)"; case SerializedPropertyType.Quaternion: Quaternion q = prop.quaternionValue; Vector3 e = q.eulerAngles; return $"Euler({e.x:F1}, {e.y:F1}, {e.z:F1})"; case SerializedPropertyType.Generic: return "(Generic Value/Struct)"; case SerializedPropertyType.ManagedReference: return string.IsNullOrEmpty(prop.managedReferenceFullTypename) ? "(Managed Reference: null)" : $"(Managed Ref: {prop.managedReferenceFullTypename.Split(' ').Last()})"; case SerializedPropertyType.Vector2Int: Vector2Int v2i = prop.vector2IntValue; return $"({v2i.x}, {v2i.y})"; case SerializedPropertyType.Vector3Int: Vector3Int v3i = prop.vector3IntValue; return $"({v3i.x}, {v3i.y}, {v3i.z})"; case SerializedPropertyType.RectInt: RectInt ri = prop.rectIntValue; return $"(X:{ri.x}, Y:{ri.y}, W:{ri.width}, H:{ri.height})"; case SerializedPropertyType.BoundsInt: BoundsInt bi = prop.boundsIntValue; return $"Pos:({bi.position.x},{bi.position.y},{bi.position.z}), Size:({bi.size.x},{bi.size.y},{bi.size.z})"; default: return $"(Unhandled Type: {prop.propertyType})"; } } catch (System.Exception ex) { return $"<Error Reading Value: {ex.GetType().Name}>"; }
        }

        // --- Hierarchy Copy (Selected Only - Used by Lite) ---
        private static void CopySelectedHierarchyOnly(GameObject[] selection, string indentString, bool useMarkers)
        {
            if (selection == null || selection.Length == 0) return; StringBuilder sb = new StringBuilder(); string nl = System.Environment.NewLine; var selectionSet = new HashSet<GameObject>(selection.Where(go => go != null)); var sortedSelection = selectionSet.OrderBy(go => GetHierarchyPath(go.transform)).ToList(); if (!sortedSelection.Any()) return; Dictionary<int, int> depthMap = new Dictionary<int, int>(); int minDepth = int.MaxValue; Transform stageRoot = null; var prefabStage = PrefabStageUtility.GetCurrentPrefabStage(); if (prefabStage != null) stageRoot = prefabStage.prefabContentsRoot.transform; foreach (var go in sortedSelection) { int depth = CalculateDepth(go.transform, stageRoot); depthMap[go.GetInstanceID()] = depth; if (depth < minDepth) minDepth = depth; }
            foreach (var go in sortedSelection) { int absoluteDepth = depthMap[go.GetInstanceID()]; int relativeDepth = absoluteDepth - minDepth; sb.Append(String.Concat(Enumerable.Repeat(indentString, relativeDepth))); if (useMarkers) { bool hasSelectedChildren = false; bool hasAnyChildren = go.transform.childCount > 0; if (hasAnyChildren) { foreach (Transform child in go.transform) { if (child != null && selectionSet.Contains(child.gameObject)) { hasSelectedChildren = true; break; } } } sb.Append(hasSelectedChildren ? "▼ " : (hasAnyChildren ? "▶ " : "  ")); } sb.Append(go.name); if (!go.activeSelf) sb.Append(" (inactive)"); sb.Append(nl); }
            EditorGUIUtility.systemCopyBuffer = sb.ToString(); Debug.Log($"[H&I Exporter Lite] Copied structure of {selectionSet.Count} selected object(s).");
        }
        private static string GetHierarchyPath(Transform t) => (t == null) ? "" : ((t.parent == null) ? $"/{(t.GetSiblingIndex()):D4}_{t.name}" : $"{GetHierarchyPath(t.parent)}/{(t.GetSiblingIndex()):D4}_{t.name}");
        private static int CalculateDepth(Transform t, Transform stageRoot) { int d = 0; Transform current = t; while (current != null && current.parent != null && current != stageRoot) { d++; current = current.parent; } return d; }
        private static void AppendHeader(StringBuilder sb, string headerText) => sb.Append($"{headerText}{System.Environment.NewLine}");

        // --- Dummy methods for features only in Full version (called by Context Menu logic) ---
        // These could potentially be removed if the context menus pointing to them are also removed.
        // Kept for now to show how context menu items link to store.
        private static void CopyGameObjectProperties(GameObject targetObject, PropertyCopyMode mode, bool showTransform, bool includeNonPublic, bool groupByType)
        {
            // This method is only fully functional in the paid version for Detailed mode or Copy All.
            // Lite context menu calls CopySingleComponentProperties directly for the one allowed case.
            if (mode == PropertyCopyMode.DetailedReflection) OpenStoreLinkFromMenu("Copy All Detailed Properties");
            else CopySingleComponentProperties(targetObject?.GetComponent<Component>(), showTransform); // Fallback for "Copy All Visible" from context? Risky. Better to disable context menu.
        }
        private static void CopyHierarchyBelowSelection(GameObject[] selection, bool includeInactive, int maxDepth, string indentString, bool useMarkers)
        {
            OpenStoreLinkFromMenu("Copying full hierarchy with options");
        }
        private static void CopyGameObjectHierarchy(GameObject rootObject, bool includeInactive, int maxDepth, string indentString, bool useMarkers)
        {
            OpenStoreLinkFromMenu("Copying full hierarchy below");
        }


    }
}

#endif // UNITY_EDITOR