using UnityEditor;

namespace Darkness
{
    [CustomInspectors(typeof(TimelineGraphAsset), true)]
    public class AssetInspector : InspectorsBase
    {
        private TimelineGraphAsset action => (TimelineGraphAsset)m_target;

        public override void OnInspectorGUI()
        {
            ShowCommonInspector();
            base.OnInspectorGUI();
        }

        protected void ShowCommonInspector(bool showBaseInspector = true)
        {
            // action.version = EditorGUILayout.TextField("Name", action.version);
        }
    }
}