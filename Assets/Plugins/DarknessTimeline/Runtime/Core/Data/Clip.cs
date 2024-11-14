using System;
using MemoryPack;
using Sirenix.OdinInspector;

namespace Darkness
{
    [Serializable]
    [MemoryPackable]
    public partial class Clip
    {
        [ReadOnly]
        public float startTime;
        [ReadOnly]
        public float length;
    }
}
