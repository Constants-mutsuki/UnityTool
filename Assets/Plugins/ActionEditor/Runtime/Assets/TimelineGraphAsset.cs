using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    [Name("时间轴")]
    [ShowIcon(typeof(Animator))]
    public sealed class TimelineGraphAsset : ScriptableObject, IData
    {

        private TimelineGraph m_timelineGraph;
        
        [SerializeField]
        private float length = 5f;

        [SerializeField]
        private float viewTimeMin = 0f;

        [SerializeField]
        private float viewTimeMax = 5f;

        [NonSerialized]
        private TrackAsset[] m_cacheOutputTracks;

        [SerializeReference]
        public List<GroupAsset> groups = new();

        public float Length
        {
            get => length;
            set => length = Mathf.Max(value, 0.1f);
        }

        public float ViewTimeMin
        {
            get => viewTimeMin;
            set
            {
                if (ViewTimeMax > 0) viewTimeMin = Mathf.Min(value, ViewTimeMax - 0.25f);
            }
        }

        public float ViewTimeMax
        {
            get => viewTimeMax;
            set => viewTimeMax = Mathf.Max(value, ViewTimeMin + 0.25f, 0);
        }

        public float MaxTime => Mathf.Max(ViewTimeMax, Length);
        public float ViewTime => ViewTimeMax - ViewTimeMin;

        public List<DirectableAsset> Directables { get; private set; }
        
        public void SetUp(TimelineGraph graph)
        {
            m_timelineGraph = graph;
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].SetUp(graph.groups[i]);
            }
        }

        public T AddGroup<T>() where T : GroupAsset, new()
        {
            var newGroup = CreateInstance<T>();
            newGroup.Name = "New Group";
            newGroup.Root = this;
            groups.Add(newGroup);
            CreateUtilities.SaveAssetIntoObject(newGroup, this);
            DirectorUtility.SelectedObject = newGroup;
            return newGroup;
        }

        public void DeleteGroup(GroupAsset groupAsset)
        {
            groups.Remove(groupAsset);
        }

        public GroupAsset PasteGroup(GroupAsset groupAsset)
        {
            var newGroup = Instantiate(groupAsset);
            if (newGroup != null)
            {
                newGroup.Root = this;
                groups.Add(newGroup);
                CreateUtilities.SaveAssetIntoObject(newGroup, this);
                newGroup.Tracks.Clear();
                foreach (var track in groupAsset.Tracks)
                {
                    newGroup.PasteTrack(track);
                }
            }

            return newGroup;
        }

        public void Validate()
        {
            foreach (var groupAsset in groups)
            {
                groupAsset.Root = this;
            }
        }

        public void SaveToAssets()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}