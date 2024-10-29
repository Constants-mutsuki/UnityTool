using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Darkness
{
    public class InspectorWindow : PopupWindowContent
    {
        private static Rect m_rect;
        private static bool m_willResample;
        private static Dictionary<IData, InspectorsBase> m_directableEditors = new();
        private static InspectorsBase m_currentDirectableEditor;
        private static ScriptableObject m_selection;
        private bool m_flag = true;
        private bool m_graphFoldout = true;


        public static void Initial()
        {
            DirectorUtility.onSelectionChange -= OnSelectionChange;
            DirectorUtility.onSelectionChange += OnSelectionChange;
        }

        private static void OnSelectionChange(ScriptableObject obj)
        {
            if (obj)
            {
                m_selection = obj;
                Show(new Rect(G.ScreenWidth - 5 - 400, Styles.ToolbarHeight + 5, 400, G.ScreenHeight - Styles.ToolbarHeight - 50));
            }
        }

        public static void Show(Rect rect)
        {
            m_rect = rect;
            PopupWindow.Show(new Rect(rect.x, rect.y, 0, 0), new InspectorWindow());
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(m_rect.width, m_rect.height);
        }

        public override void OnGUI(Rect rect)
        {
            if (!DirectorUtility.selectedObject)
            {
                EditorGUILayout.HelpBox(Lan.NotSelectAsset, MessageType.Info);
                return;
            }
            GUI.skin.GetStyle("label").richText = true;
            GUILayout.Space(5);
            DrawGraphInspector();
            DrawSelectionInspector();
            if (m_flag || Event.current.type == EventType.Repaint)
            {
                m_flag = false;
                m_rect.height = GUILayoutUtility.GetLastRect().yMax + 5;
            }
        
        }

        private void DrawGraphInspector()
        {
            if (!App.GraphAsset)
            {
                return;
            }
            var graphAsset = App.GraphAsset;
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;
            var title = "时间轴基础信息";
            GUILayout.Label($"{(m_graphFoldout ? "▼" : "▶")} {title}");
            GUILayout.EndHorizontal();
            var lastRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
                m_graphFoldout = !m_graphFoldout;
                Event.current.Use();
            }
            GUILayout.Space(1);
            if (m_graphFoldout)
            {
                if (!m_directableEditors.TryGetValue(graphAsset, out var drawer))
                {
                    m_directableEditors[graphAsset] = drawer = EditorInspectorFactory.GetInspector(graphAsset);
                }
                drawer?.OnInspectorGUI();
            }
        }

        private void DrawSelectionInspector()
        {
            if (!m_selection || !(m_selection is IData data)) return;

            if (!m_directableEditors.TryGetValue(data, out var drawer))
            {
                m_directableEditors[data] = drawer = EditorInspectorFactory.GetInspector(data);
            }

            if (m_currentDirectableEditor != drawer)
            {
                var enableMethod = drawer.GetType().GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (enableMethod != null)
                {
                    enableMethod.Invoke(drawer, null);
                }

                m_currentDirectableEditor = drawer;
            }

            EditorTools.BoldSeparator();
            GUILayout.Space(4);
            DrawSelectionInfo();
            m_currentDirectableEditor?.OnInspectorGUI();
        }

        private void DrawSelectionInfo()
        {
            if (!m_selection) return;
            var type = m_selection.GetType();
            NameAttribute nameAttribute = type.GetCustomAttribute<NameAttribute>();
            string nameInfo = nameAttribute != null ? nameAttribute.name : type.Name.SplitCamelCase();
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;
            GUILayout.Label(nameInfo);
            GUILayout.EndHorizontal();

            DescriptionAttribute description = type.GetCustomAttribute<DescriptionAttribute>();
            string desc = description != null ? description.description : string.Empty;
            if (!string.IsNullOrEmpty(desc))
            {
                EditorGUILayout.HelpBox(desc, MessageType.None);
            }
            GUILayout.Space(2);
        }
    }
}
