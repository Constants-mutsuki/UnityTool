using UnityEditor;
using UnityEngine;

namespace Darkness
{
    public static class DirectorUtility
    {
        private static ClipAsset m_copyClipAsset;
        private static System.Type m_copyClipType;
        [System.NonSerialized]
        private static InspectorPreviewAsset m_currentInspectorPreviewAsset;
        [System.NonSerialized]
        private static ScriptableObject m_selectedObject;
        public static event System.Action<ScriptableObject> onSelectionChange;

        public static ClipAsset CopyClipAsset
        {
            get => m_copyClipAsset;
            set
            {
                m_copyClipAsset = value;
                m_copyClipType = value ? value.GetType() : default;
            }
        }

        public static System.Type GetCopyType()
        {
            return m_copyClipType;
        }

        public static void FlushCopyClip()
        {
            m_copyClipType = null;
            m_copyClipAsset = null;
        }

        public static void CutClip(ClipAsset clipAsset)
        {
            m_copyClipAsset = clipAsset;
            m_copyClipType = clipAsset.GetType();
            (clipAsset.Parent as TrackAsset)?.DeleteClip(clipAsset);
        }


        public static InspectorPreviewAsset CurrentInspectorPreviewAsset
        {
            get
            {
                if (m_currentInspectorPreviewAsset == null)
                {
                    m_currentInspectorPreviewAsset = ScriptableObject.CreateInstance<InspectorPreviewAsset>();
                }

                return m_currentInspectorPreviewAsset;
            }
        }


        public static ScriptableObject selectedObject
        {
            get => m_selectedObject;
            set
            {
                m_selectedObject = value;
                if (value)
                {
#if UNITY_EDITOR
                    Selection.activeObject = CurrentInspectorPreviewAsset;
                    EditorUtility.SetDirty(CurrentInspectorPreviewAsset);
#endif
                }

                onSelectionChange?.Invoke(value);
            }
        }
    }
}
