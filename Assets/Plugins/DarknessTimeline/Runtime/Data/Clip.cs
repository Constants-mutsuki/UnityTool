using MemoryPack;

namespace Darkness
{
    [MemoryPackable]
    [MemoryPackUnion(0,typeof(ExampleClip))]
    public abstract partial class Clip 
    {
        public float startTime;
        public float length;
    }
}
