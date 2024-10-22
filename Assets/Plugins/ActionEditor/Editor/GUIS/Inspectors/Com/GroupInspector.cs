using UnityEditor;
using UnityEngine;

namespace Darkness.Design.Editor.Inspectors
{
    public abstract class GroupInspector<T> : ActionClipInspector where T : GroupAsset
    {
        protected T action => (T)target;
    }

    [CustomInspectors(typeof(GroupAsset), true)]
    public class GroupInspector : InspectorsBase
    {
        private GroupAsset action => (GroupAsset)target;

        public override void OnInspectorGUI()
        {
            ShowCommonInspector();
        }

        protected void ShowCommonInspector(bool showBaseInspector = true)
        {
            action.Name = EditorGUILayout.TextField("Name", action.Name);
            if (showBaseInspector)
            {
                base.OnInspectorGUI();
            }
        }
    }
}