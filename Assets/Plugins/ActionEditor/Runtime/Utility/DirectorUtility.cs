using UnityEditor;
using UnityEngine;

namespace Darkness
{
    public static class DirectorUtility
    {
        private static ActionClipAsset m_copyClipAsset;
        private static System.Type _copyClipType;
        
        [System.NonSerialized] private static InspectorPreviewAsset _currentInspectorPreviewAsset;

        [System.NonSerialized] private static ScriptableObject _selectedObject;
        public static event System.Action<ScriptableObject> onSelectionChange;
        
        public static ActionClipAsset CopyClipAsset
        {
            get => m_copyClipAsset;
            set
            {
                m_copyClipAsset = value;
                if (value != null)
                {
                    _copyClipType = value.GetType();
                }
                else
                {
                    _copyClipType = default;
                }
            }
        }

        public static System.Type GetCopyType()
        {
            return _copyClipType;
        }


        public static void FlushCopyClip()
        {
            _copyClipType = null;
            m_copyClipAsset = null;
        }


        public static void CutClip(ActionClipAsset clipAsset)
        {
            m_copyClipAsset = clipAsset;
            _copyClipType = clipAsset.GetType();
            clipAsset.Parent.DeleteAction(clipAsset);
        }
        

        public static InspectorPreviewAsset CurrentInspectorPreviewAsset
        {
            get
            {
                if (_currentInspectorPreviewAsset == null)
                {
                    _currentInspectorPreviewAsset = ScriptableObject.CreateInstance<InspectorPreviewAsset>();
                }

                return _currentInspectorPreviewAsset;
            }
        }


        public static ScriptableObject selectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                if (value != null)
                {
#if UNITY_EDITOR
                    Selection.activeObject = CurrentInspectorPreviewAsset;
                    EditorUtility.SetDirty(CurrentInspectorPreviewAsset);
#endif
                }

                if (onSelectionChange != null)
                {
                    onSelectionChange(value);
                }
            }
        }
    }
}