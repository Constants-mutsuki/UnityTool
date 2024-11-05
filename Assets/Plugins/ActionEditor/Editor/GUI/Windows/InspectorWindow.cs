﻿using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Darkness
{
    public class InspectorWindow : EditorWindow
    {
        private static InspectorWindow m_instance;
        private static Rect m_rect;
        private static bool m_willResample;
        private static Dictionary<IData, InspectorsBase> m_directableEditors = new();
        private static InspectorsBase m_currentDirectableEditor;
        private static ScriptableObject m_selection;
        private bool m_flag = true;
        private bool m_graphFoldout = true;

        public static void Init()
        {
            DirectorUtility.OnSelectionChange -= OnSelectionChange;
            DirectorUtility.OnSelectionChange += OnSelectionChange;
        }

        private static void ShowWindow()
        {
            m_instance = GetWindow<InspectorWindow>("Inspector Window");
            m_instance.minSize = new Vector2(400, 200);
        }

        private void OnDisable()
        {
            DirectorUtility.OnSelectionChange -= OnSelectionChange;
        }

        private static void OnSelectionChange(ScriptableObject obj)
        {
            if (!m_instance)
            {
                ShowWindow();
            }
            if (obj)
            {
                m_selection = obj;
                m_instance?.Repaint();
            }
        }

        private void OnGUI()
        {
            if (!DirectorUtility.SelectedObject)
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
                minSize = new Vector2(400, GUILayoutUtility.GetLastRect().yMax + 10);
            }
        }
        
        private void DrawGraphInspector()
        {
            if (!App.GraphAsset) return;

            var graphAsset = App.GraphAsset;
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;
            var inspectorTitle = "时间轴基础信息";
            GUILayout.Label($"{(m_graphFoldout ? "▼" : "▶")} {inspectorTitle}");
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
                enableMethod?.Invoke(drawer, null);

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
            var nameAttribute = type.GetCustomAttribute<NameAttribute>();
            string nameInfo = nameAttribute != null ? nameAttribute.name : type.Name.SplitCamelCase();

            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;
            GUILayout.Label(nameInfo);
            GUILayout.EndHorizontal();

            var description = type.GetCustomAttribute<DescriptionAttribute>();
            string desc = description?.description ?? string.Empty;

            if (!string.IsNullOrEmpty(desc))
            {
                EditorGUILayout.HelpBox(desc, MessageType.None);
            }
            GUILayout.Space(2);
        }
    }
}
