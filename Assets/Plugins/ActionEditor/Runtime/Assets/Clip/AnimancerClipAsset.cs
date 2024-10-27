using System;

namespace Darkness
{
    [Serializable]
    [Name("动画轨道")]
    [Description("播放一个动画片段")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(AbilityTrackAsset))]
    public class AnimancerClipAsset : ClipAsset
    {
    }
}