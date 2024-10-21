using CZToolKit;
using CZToolKit.GraphProcessor.Editors;

namespace LogicGraph.Runtime
{
    [CustomView(typeof(LogicGraphModel))]
    public class LogicGraphWindow : BaseGraphWindow
    {
        protected override BaseGraphView NewGraphView()
        {
            return new LoginGraphView(Graph, this, new CommandDispatcher());
        }

        protected override void AfterLoad()
        {
            titleContent.text = nameof(LogicGraph);
        }
    }
}
