using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    internal static class WbbQuickBuildMenu
    {
        private const string Root = "GameObject/" + WbbProduct.Name + "/";
        private const int Priority = 11;
        private static bool requestQueued;
        [MenuItem(Root + "Prop", false, Priority)]
        private static void BuildProp(MenuCommand command)
        {
            Request(WbbExportType.Prop);
        }

        [MenuItem(Root + "Prop", true)]
        private static bool CanBuildProp()
        {
            return CanRequest();
        }

        [MenuItem(Root + "Character", false, Priority + 1)]
        private static void BuildCharacter(MenuCommand command)
        {
            Request(WbbExportType.Character);
        }

        [MenuItem(Root + "Character", true)]
        private static bool CanBuildCharacter()
        {
            return CanRequest();
        }

        private static void Request(WbbExportType type)
        {
            if (requestQueued)
                return;
            requestQueued = true;
            EditorApplication.delayCall += delegate
            {
                requestQueued = false;
                WbbQuickBuildWindow.Open(type, CollectSelection(), null, "");
            };
        }

        private static bool CanRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return false;
            if (WbbBuildRunner.AnyRunning)
                return false;
            GameObject[] selected = Selection.gameObjects;
            for (int i = 0; i < selected.Length; i++)
            {
                if (IsSceneObject(selected[i]))
                    return true;
            }

            return false;
        }

        internal static List<GameObject> CollectSelection()
        {
            var result = new List<GameObject>();
            GameObject[] selected = Selection.gameObjects;
            for (int i = 0; i < selected.Length; i++)
            {
                if (IsSceneObject(selected[i]))
                    result.Add(selected[i]);
            }

            return result;
        }

        private static bool IsSceneObject(GameObject go)
        {
            return go != null && !EditorUtility.IsPersistent(go) && go.scene.IsValid();
        }
    }
}
