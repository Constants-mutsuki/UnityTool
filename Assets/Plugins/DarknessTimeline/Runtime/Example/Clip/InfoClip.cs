using System;
using MemoryPack;

namespace Darkness
{
    [MemoryPackable]
    [Serializable]
    [Name("打印信息")]
    [Attachable(typeof(ATrack))]
    public partial class InfoClip : Clip
    {
        public string Info;
    }
}