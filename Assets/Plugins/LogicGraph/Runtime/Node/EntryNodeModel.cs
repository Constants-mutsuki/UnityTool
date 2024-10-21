using CZToolKit;
using CZToolKit.GraphProcessor;

namespace LogicGraph.Runtime
{
    [NodeTooltip("入口节点")]
    [NodeMenu("入口")]
    public class EntryNodeModel : BaseNode
    {
    }

    [ViewModel(typeof(EntryNodeModel))]
    public class EntryNodeProcessor : BaseNodeProcessor
    {
        public EntryNodeProcessor(BaseNode model) : base(model)
        {
            AddPort(new BasePortProcessor("output", BasePort.Orientation.Horizontal, BasePort.Direction.Right, BasePort.Capacity.Single));
        }
    }
}
