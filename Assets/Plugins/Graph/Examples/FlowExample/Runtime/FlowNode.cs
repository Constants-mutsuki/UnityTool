﻿using CZToolKit.GraphProcessor;
using CZToolKit;

public abstract class FlowNode : BaseNode
{
    
}

[ViewModel(typeof(FlowNode))]
public abstract class FlowNodeProcessor : BaseNodeProcessor
{
    public FlowNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new BasePortProcessor(ConstValues.FLOW_IN_PORT_NAME, BasePort.Orientation.Horizontal, BasePort.Direction.Left, BasePort.Capacity.Multi));
        AddPort(new BasePortProcessor(ConstValues.FLOW_OUT_PORT_NAME, BasePort.Orientation.Horizontal, BasePort.Direction.Right, BasePort.Capacity.Single));
    }

    protected abstract void Execute();

    public void FlowNext()
    {
        FlowTo(ConstValues.FLOW_OUT_PORT_NAME);
    }

    public void FlowTo(string port)
    {
        foreach (var baseNodeProcessor in GetConnections(ConstValues.FLOW_OUT_PORT_NAME))
        {
            var item = (FlowNodeProcessor)baseNodeProcessor;
            if (item == null)
                continue;
            
            item.Execute();
        }
    }
}
