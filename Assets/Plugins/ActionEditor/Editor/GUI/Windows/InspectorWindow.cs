using UnityEditor;
using UnityEngine;

namespace Darkness
{
    public class InspectorWindow : PopupWindowContent
    {
        private static Rect m_rect;

        public static void Show(Rect rect)
        {
            m_rect = rect;
            PopupWindow.Show(new Rect(rect.x, rect.y, 0, 0), new InspectorWindow());
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(m_rect.width, m_rect.height);
        }
    }
}
