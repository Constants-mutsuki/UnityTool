using CZToolKit;
using CZToolKit.GraphProcessor;

namespace LogicGraph.Runtime
{
    [ViewModel(typeof(LogicGraphModel))]
    public class LogicGraphProcessor : BaseGraphProcessor
    {
        public LogicGraphProcessor(LogicGraphModel model) : base(model)
        {
        }
    }
}
