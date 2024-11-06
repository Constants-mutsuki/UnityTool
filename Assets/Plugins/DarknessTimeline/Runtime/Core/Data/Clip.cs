using System;
using MemoryPack;

namespace Darkness
{
    [Serializable]
    [MemoryPackable]
    public partial class Clip
    {
        public float startTime;
        public float length;
    }
}
