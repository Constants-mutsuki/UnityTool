using MemoryPack;

namespace Darkness
{
    [MemoryPackable]
    [Name("例子轨道")]
    [Category("Test")]
    [Description("ExampleTrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    public partial class ExampleTrack : Track
    {
    }
}