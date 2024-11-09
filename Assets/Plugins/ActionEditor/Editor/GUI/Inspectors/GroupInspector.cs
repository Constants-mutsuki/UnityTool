﻿using UnityEditor;
using UnityEngine;

namespace Darkness
{
    public abstract class GroupInspector<T> : ClipInspector where T : GroupAsset
    {
        protected T action => (T)m_target;
    }

    [CustomInspectors(typeof(GroupAsset), true)]
    public class GroupInspector : InspectorsBase
    {
        private GroupAsset action => (GroupAsset)m_target;

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