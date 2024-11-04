using System;

namespace Darkness
{
    [Serializable]
    [Name("子弹")]
    [Description("生成子弹")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(AbilityTrackAsset))]
    public class BulletClipAsset : ClipAsset
    {
    }
}
