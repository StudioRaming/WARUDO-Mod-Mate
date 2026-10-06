using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbQuickBuild
    {
        private const int MaxInspectedProperties = 200000;
        public class SourceReport
        {
            public int externalReferences;
            public string externalSample = "";
            public int missingScripts;
            public bool truncated;
        }

        private static bool SaveSnapshot(GameObject source, string assetPath, WbbFixupOptions options, out WbbFixupReport report, out string error)
        {
            return WbbSnapshot.SaveFromSceneObject(source, assetPath, options, out report, out error);
        }

        public static SourceReport InspectSource(GameObject source)
        {
            var report = new SourceReport();
            if (source == null)
                return report;
            Transform rootTransform = source.transform;
            int visited = 0;
            report.missingScripts = WbbSnapshot.CountMissingScripts(source);
            Component[] components = source.GetComponentsInChildren<Component>(true);
            for (int c = 0; c < components.Length; c++)
            {
                Component component = components[c];
                if (component == null || component is Transform)
                    continue;
                using (var serialized = new SerializedObject(component))
                {
                    SerializedProperty it = serialized.GetIterator();
                    bool enter = true;
                    while (it.Next(enter))
                    {
                        if (++visited > MaxInspectedProperties)
                        {
                            report.truncated = true;
                            return report;
                        }

                        enter = true;
                        if (it.propertyType == SerializedPropertyType.String)
                        {
                            enter = false;
                            continue;
                        }

                        if (it.isArray)
                        {
                            enter = ArrayMayHoldReferences(it);
                            continue;
                        }

                        if (it.propertyType != SerializedPropertyType.ObjectReference)
                            continue;
                        GameObject targetObject = SceneObjectOf(it.objectReferenceValue);
                        if (targetObject == null)
                            continue;
                        Transform targetTransform = targetObject.transform;
                        if (targetTransform == rootTransform || targetTransform.IsChildOf(rootTransform))
                            continue;
                        report.externalReferences++;
                        if (report.externalSample.Length == 0)
                        {
                            report.externalSample = component.gameObject.name + " · " + component.GetType().Name + "." + it.displayName + " → " + targetObject.name;
                        }
                    }
                }
            }

            return report;
        }

        private static bool ArrayMayHoldReferences(SerializedProperty array)
        {
            if (array.arraySize == 0)
                return false;
            SerializedPropertyType elementType = array.GetArrayElementAtIndex(0).propertyType;
            return elementType == SerializedPropertyType.ObjectReference || elementType == SerializedPropertyType.Generic || elementType == SerializedPropertyType.ManagedReference;
        }

        private static GameObject SceneObjectOf(Object value)
        {
            if (value == null || EditorUtility.IsPersistent(value))
                return null;
            GameObject go = value as GameObject;
            if (go != null)
                return go;
            Component component = value as Component;
            return component != null ? component.gameObject : null;
        }
    }
}
