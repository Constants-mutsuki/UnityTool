using System;
using Sirenix.OdinInspector;

namespace Darkness
{
    [Serializable]
    public class Clip
    {
        [ReadOnly]
        public float startTime;
        [ReadOnly]
        public float length;
    }
}
