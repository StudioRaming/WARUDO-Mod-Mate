using UnityEditor;
using UnityEngine;

namespace StudioRaming.WarudoBatchBuilder
{
    public partial class WbbHistorySection
    {
        private float listTopObserved;
        private float footerHeightObserved = 80f;
        private float viewportHeightLayout;
        private void DrawFooterAndStatus()
        {
            Rect start = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
            DrawBottomBar();
            DrawStatusLine();
            Rect end = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                float height = end.y - start.y + WbbSize.SpaceEdge * 2f;
                if (height > 0f && Mathf.Abs(height - footerHeightObserved) > 1f)
                {
                    footerHeightObserved = height;
                    if (host != null)
                        host.RequestRepaint();
                }
            }
        }

        private void ObserveListTop(Rect probe)
        {
            if (Event.current.type != EventType.Repaint || probe.y <= 0f)
                return;
            if (Mathf.Abs(probe.y - listTopObserved) <= 1f)
                return;
            listTopObserved = probe.y;
            if (host != null)
                host.RequestRepaint();
        }
    }
}
