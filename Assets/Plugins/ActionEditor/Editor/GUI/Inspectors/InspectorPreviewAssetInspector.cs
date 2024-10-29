using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Darkness
{
    [CustomEditor(typeof(InspectorPreviewAsset))]
    public class InspectorPreviewAssetInspector : Editor
    {
        private bool m_optionsAssetFold = true;
        private static TimelineGraphAsset m_lastTimelineGraphAsset;
        private static bool m_willResample;
        private static Dictionary<IData, InspectorsBase> m_directableEditors = new();
        private static InspectorsBase m_currentDirectableEditor;
        private static InspectorsBase m_currentAssetEditor;


        void OnEnable()
        {
            m_currentDirectableEditor = null;
            m_willResample = false;
        }

        void OnDisable()
        {
            m_currentDirectableEditor = null;
            m_directableEditors.Clear();
            m_willResample = false;
        }

        protected override void OnHeaderGUI()
        {
            GUILayout.Space(18f);
        }

        public override void OnInspectorGUI()
        {
            var ow = target as InspectorPreviewAsset;
            if (ow == null || DirectorUtility.selectedObject == null)
            {
                EditorGUILayout.HelpBox(Lan.NotSelectAsset, MessageType.Info);
                return;
            }

            GUI.skin.GetStyle("label").richText = true;

            GUILayout.Space(5);

            DoAssetInspector();
            DoSelectionInspector();


            if (m_willResample)
            {
                m_willResample = false;
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("cutscene.ReSample();");
                };
            }

            Repaint();
        }


        void DoAssetInspector()
        {
            if (App.GraphAsset == null) return;
            var assetData = App.GraphAsset;
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;
            var title = string.Format(Lan.InsBaseInfo, Prefs.GetAssetTypeName(assetData.GetType()));
            GUILayout.Label($"<b><size=18>{(m_optionsAssetFold ? "▼" : "▶")} {title}</size></b>");
            GUILayout.EndHorizontal();

            var lastRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
                m_optionsAssetFold = !m_optionsAssetFold;
                Event.current.Use();
            }

            GUILayout.Space(2);
            if (m_optionsAssetFold)
            {
                if (!m_directableEditors.TryGetValue(assetData, out var newEditor))
                {
                    m_directableEditors[assetData] = newEditor = EditorInspectorFactory.GetInspector(assetData);
                }

                if (m_currentAssetEditor != newEditor)
                {
                    m_currentAssetEditor = newEditor;
                }

                m_currentAssetEditor?.OnInspectorGUI();
            }
        }

        void DoSelectionInspector()
        {
            var selection = DirectorUtility.selectedObject;

            if (selection == null)
            {
                m_currentDirectableEditor = null;
                return;
            }

            if (!(selection is IData data)) return;

            if (!m_directableEditors.TryGetValue(data, out var newEditor))
            {
                m_directableEditors[data] = newEditor = EditorInspectorFactory.GetInspector(data);
            }

            if (m_currentDirectableEditor != newEditor)
            {
                var enableMethod = newEditor.GetType().GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (enableMethod != null)
                {
                    enableMethod.Invoke(newEditor, null);
                }

                m_currentDirectableEditor = newEditor;
            }

            EditorTools.BoldSeparator();
            GUILayout.Space(4);
            ShowPreliminaryInspector();

            if (m_currentDirectableEditor != null) m_currentDirectableEditor.OnInspectorGUI();
        }

        /// <summary>
        /// 选中对象基本信息
        /// </summary>
        void ShowPreliminaryInspector()
        {
            if (App.GraphAsset == null) return;
            var type = DirectorUtility.selectedObject.GetType();
            var nameAtt = type.GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault() as NameAttribute;
            var name = nameAtt != null ? nameAtt.name : type.Name.SplitCamelCase();

            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;

            GUILayout.Label($"<b><size=18>{name}</size></b>");


            GUILayout.EndHorizontal();

            var desAtt = type.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            var description = desAtt != null ? desAtt.description : string.Empty;
            if (!string.IsNullOrEmpty(description))
            {
                EditorGUILayout.HelpBox(description, MessageType.None);
            }

            GUILayout.Space(2);
        }
    }
}
