using MemoryPack;

namespace Darkness
{
    [Name("ATrack")]
    [Category("Test")]
    [Description("ATrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(GroupAsset))]
    [MemoryPackable]
    public partial class ATrack : Track
    {
    }

    [Name("BTrack")]
    [Category("Test")]
    [Description("BTrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(GroupAsset))]
    [MemoryPackable]
    public partial class BTrack : Track
    {
    }

    [Name("CTrack")]
    [Category("Test")]
    [Description("CTrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(GroupAsset))]
    [MemoryPackable]
    public partial class CTrack : Track
    {
    }
}
