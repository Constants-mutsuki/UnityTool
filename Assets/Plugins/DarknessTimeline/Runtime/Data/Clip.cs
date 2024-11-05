using System;
using MemoryPack;

namespace Darkness
{
    [Serializable]
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(ExampleClip))]
    public abstract partial class Clip
    {
        public float startTime;
        public float length;
    }
}
