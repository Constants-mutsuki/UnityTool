using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    public class GroupAsset : DirectableAsset
    {
        [SerializeField, HideInInspector] private List<TrackAsset> tracks = new List<TrackAsset>();
        [SerializeField, HideInInspector] private bool isCollapsed = false;
        [SerializeField, HideInInspector] private bool active = true;
        [SerializeField, HideInInspector] private bool isLocked = false;


        public virtual TimelineGraphAsset Parent
        {
            get => (TimelineGraphAsset)m_parent;
            set => m_parent = value;
        }

        public override float StartTime => 0;
        public override float EndTime => Parent.Length;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public List<TrackAsset> Tracks
        {
            get => tracks;
            set => tracks = value;
        }

        public override bool IsActive
        {
            get => active;
            set
            {
                if (active != value)
                {
                    active = value;
                }
            }
        }

        public override bool IsCollapsed
        {
            get => isCollapsed;
            set => isCollapsed = value;
        }

        public override bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }


        #region 增删

        public bool CanAddTrack(TrackAsset trackAsset)
        {
            return trackAsset != null && CanAddTrackOfType(trackAsset.GetType());
        }

        public bool CanAddTrackOfType(Type type)
        {
            if (type == null || !type.IsSubclassOf(typeof(TrackAsset)) || type.IsAbstract)
            {
                return false;
            }

            if (type.IsDefined(typeof(UniqueAttribute), true) &&
                Tracks.FirstOrDefault(t => t.GetType() == type) != null)
            {
                return false;
            }

            var attachAtt = type.RTGetAttribute<AttachableAttribute>(true);
            if (attachAtt == null || attachAtt.Types == null || attachAtt.Types.All(t => t != this.GetType()))
            {
                return false;
            }

            return true;
        }

        public T AddTrack<T>(string _name = null) where T : TrackAsset
        {
            return (T)AddTrack(typeof(T), _name);
        }

        public TrackAsset AddTrack(Type type, string _name = null)
        {
            var newTrack = CreateInstance(type);
            if (newTrack is TrackAsset track)
            {
                track.Name = type.Name;
                track.Parent = this;
                Tracks.Add(track);

                CreateUtilities.SaveAssetIntoObject(track, this);
                DirectorUtility.selectedObject = track;

                return track;
            }

            return null;
        }

        public void DeleteTrack(TrackAsset trackAsset)
        {
            // Undo.RegisterCompleteObjectUndo(this, "Delete Track");
            Tracks.Remove(trackAsset);
            if (ReferenceEquals(DirectorUtility.selectedObject, trackAsset))
            {
                DirectorUtility.selectedObject = null;
            }

            // Undo.DestroyObjectImmediate(track);
            // EditorUtility.SetDirty(this);
            // root?.Validate();
            // root?.SaveToAssets();
        }


        public TrackAsset PasteTrack(TrackAsset trackAsset)
        {
            if (!CanAddTrack(trackAsset))
            {
                return null;
            }

            var newTrack = Instantiate(trackAsset);
            if (newTrack != null)
            {
                newTrack.Parent = this;
                Tracks.Add(newTrack);
                CreateUtilities.SaveAssetIntoObject(newTrack, this);
                newTrack.Clips.Clear();
                foreach (var clip in trackAsset.Clips)
                {
                    newTrack.PasteClip(clip);
                }
            }

            return newTrack;
        }

        #endregion
    }
}